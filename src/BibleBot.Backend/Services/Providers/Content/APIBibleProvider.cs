/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using BibleBot.Models;
using Sentry;
using Serilog;
using Version = BibleBot.Models.Version;

namespace BibleBot.Backend.Services.Providers.Content
{
    public partial class APIBibleProvider : IContentProvider
    {
        public string Name { get; set; }

        private readonly HttpClient _cachingHttpClient;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly HtmlParser _htmlParser;

        private readonly string _baseURL = "https://api.scripture.api.bible/v1/";
        private readonly string _getURI = "bibles/{0}/search?query={1}&limit=100";
        private readonly string _searchURI = "bibles/{0}/search?query={1}&limit=100&sort=relevance";

        public APIBibleProvider()
        {
            Name = "ab";

            _cachingHttpClient = CachingClient.GetTrimmedCachingClient(_baseURL, false);
            _cachingHttpClient.DefaultRequestHeaders.Add("api-key", Environment.GetEnvironmentVariable("APIBIBLE_TOKEN"));
            _httpClient = new HttpClient { BaseAddress = new Uri(_baseURL) };
            _httpClient.DefaultRequestHeaders.Add("api-key", Environment.GetEnvironmentVariable("APIBIBLE_TOKEN"));

            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

            _htmlParser = new HtmlParser();
        }

        [GeneratedRegex("[a-zA-Z]{3} ([0-9]{1,3}):([0-9]{1,3})", RegexOptions.Compiled)]
        private static partial Regex VerseIdRegex();



        public async Task<VerseResult> GetVerse(Reference reference, bool titlesEnabled, bool verseNumbersEnabled)
        {
            SentrySdk.ConfigureScope(scope => { scope.Contexts["reference"] = reference; });

            string originalProperName;

            string[] solidTextClasses = ["d", "m", "cls", "mi", "nb"];
            string[] prefixTextClasses = ["q", "p", "add", "l"];

            if (reference.AsString == null)
            {
                try
                {
                    originalProperName = reference.Book.ProperName;
                }
                catch (NullReferenceException err)
                {
                    // The version used does not have book data populated, no references will work until resolved.
                    SentrySdk.CaptureException(err);

                    return null;
                }

                if (reference.Version.Abbreviation is "ELXX" or "LXX")
                {
                    switch (reference.Book.Name)
                    {
                        case "DAN":
                            {
                                reference.Book.ProperName = "DAG";

                                // For whatever reason, the ELXX we use lists Daniel as a book, but
                                // it actually doesn't exist, so we defer to the "updated" ELXX.
                                if (reference.Version.Abbreviation == "ELXX")
                                {
                                    reference.Version.InternalId = "6bab4d6c61b31b80-01";
                                }

                                break;
                            }
                        case "EZR" or "NEH":
                            reference.Book.ProperName = "EZR";
                            originalProperName = "Ezra/Nehemiah";
                            break;
                        default:
                            reference.Book.ProperName = reference.Book.Name;
                            break;
                    }
                }
                else
                {
                    reference.Book.ProperName = reference.Book.Name;
                }

                reference.AsString = reference.ToString();
                reference.Book.Name = reference.Book.ProperName;
                reference.Book.ProperName = originalProperName;
            }
            else
            {
                string[] tokenizedReference = reference.AsString.Split(" ");
                originalProperName = string.Join(" ", tokenizedReference.Take(tokenizedReference.Length - 1));

                try
                {
                    reference.Book = reference.Version.Books.First(book => book.ProperName == originalProperName);
                    reference.AsString = reference.AsString.Replace(originalProperName, reference.Book.Name);
                }
                catch (InvalidOperationException ioEx)
                {
                    if (ioEx.Message == "Sequence contains no matching element")
                    {
                        Log.Error($"couldn't find book '{originalProperName}' in {reference.Version.Name}");
                        SentrySdk.CaptureException(ioEx);
                    }
                    else
                    {
                        Log.Error($"received unhandled InvalidOperationException for {reference.Version.Name}");
                        SentrySdk.CaptureException(ioEx);
                    }
                }
            }

            string url = string.Format(_getURI, reference.Version.InternalId, reference.AsString);

            ABSearch resp = await _cachingHttpClient.GetJsonContentAs<ABSearch>(url, _jsonOptions);

            if (resp == null)
            {
                return null;
            }

            if (resp.Passages == null || resp.Passages.Count == 0)
            {
                Log.Error($"APIBibleProvider: Received no passages for '{reference.AsString}' with {reference.Version.Abbreviation}.");
                return null;
            }

            if (resp.Passages[0].BibleId != reference.Version.InternalId)
            {
                Log.Error($"APIBibleProvider: {reference.Version.Abbreviation} is no longer available.");
                return null;
            }

            if (resp.Passages[0].Content.Length < 1)
            {
                return null;
            }

            string title = "";
            List<string> texts = [];

            foreach (ABPassage passage in resp.Passages)
            {
                IHtmlDocument document = await _htmlParser.ParseDocumentAsync(passage.Content);

                IHtmlCollection<IElement> otherData = document.QuerySelectorAll(".d");

                foreach (IElement el in otherData)
                {
                    IElement verseEl = el.QuerySelector("span.v");
                    bool shouldRemoveElements = true;

                    if (verseEl != null)
                    {
                        if (el.NextElementSibling != null)
                        {
                            el.NextElementSibling.Prepend(verseEl.Clone());
                        }
                        else
                        {
                            // This is a scenario where a descriptive title has a verse yet we have
                            // no other place to put the verse, so we want to preserve the elements.
                            //
                            // I could move the verse element removal line to the case above, but
                            // for the sake of showing what's going on, I'm keeping this.
                            shouldRemoveElements = false;
                        }

                        if (shouldRemoveElements) { verseEl.Remove(); }
                    }

                    if (shouldRemoveElements) { el.Remove(); }
                }

                IHtmlCollection<IElement> numbers = document.QuerySelectorAll(".v");

                foreach (IElement el in numbers)
                {
                    if (verseNumbersEnabled)
                    {
                        string id = el.GetAttribute("data-sid") ?? el.GetAttribute("data-number");
                        MatchCollection matches = VerseIdRegex().Matches(id);

                        if (matches.Count > 0)
                        {
                            string chapter = matches[0].Groups[1].Value;
                            string verse = matches[0].Groups[2].Value;

                            bool isSubsequentChapter = int.TryParse(chapter, out int chapterNum) && chapterNum > reference.StartingChapter;
                            bool shouldShowChapter = reference.StartingChapter != reference.EndingChapter && verse == "1" && isSubsequentChapter;

                            el.TextContent = shouldShowChapter ? $" <**{chapter}:{verse}**> " : $" <**{verse}**> ";
                        }
                        else
                        {
                            el.TextContent = $" <**{el.TextContent}**> ";
                        }
                    }
                    else
                    {
                        el.Remove();
                    }
                }

                title += titlesEnabled ? string.Join(" / ", document.GetElementsByClassName("s1").Select(el => el.TextContent.Trim())) : "";
                texts.Add(string.Join("\n", document.GetElementsByTagName("p").Where(el => solidTextClasses.Contains(el.ClassName) || prefixTextClasses.Any(prefix => el.ClassName!.StartsWith(prefix))).Select(el => el.TextContent.Trim())));
            }

            string text = string.Join("\n", texts);

            if (reference.Version.Abbreviation == "NLD1939" && text.Contains("tuchtmeester geweest tot Christus’ 3:opdat we"))
            {
                text = text.Replace("tuchtmeester geweest tot Christus’ 3:opdat we", "tuchtmeester geweest tot Christus' komst, opdat we");
            }

            reference.Book ??= new Book();
            reference.Book.ProperName = originalProperName;
            reference.Book.PreferredName ??= originalProperName;

            // As the verse reference could have a non-English name...
            string properBookName = reference.Book.PreferredName;

            if (reference.Version.Abbreviation == "ELXX")
            {
                // Don't like version-specific workarounds, but given the naming convention
                // wackyness they've got here, this seems like the best course of action.
                properBookName = originalProperName;
            }

            string[] providedReferenceTokenized = resp.Passages[0].Reference.Split(" ");
            reference.AsString = $"{properBookName} {string.Join(" ", providedReferenceTokenized.TakeLast(1))}";

            if (resp.Passages.Count > 1)
            {
                for (int i = 1; i < resp.Passages.Count; i++)
                {
                    string[] colonSplit = resp.Passages[i].Reference.Split(':');

                    reference.AsString += $", {colonSplit[1]}";
                }
            }

            // For some reason something like Psalm 1:1-2:1 comes back with the
            // reference Psalm 1:1-21 despite the text itself being correct.
            // if (reference.AppendedVerses.Count == 0)
            // {
            //     if (reference.StartingChapter != reference.EndingChapter)
            //     {
            //         int referenceStrLen = reference.AsString.Length;
            //         reference.AsString = $"{reference.AsString.Substring(0, referenceStrLen - 1)}:{reference.AsString.Substring(referenceStrLen - 1)}";
            //     }
            // }

            if (reference.StartingChapter != reference.EndingChapter)
            {
                string spanToken = reference.AsString.Split(" ").FirstOrDefault(tok => tok.Contains(':'));

                if (spanToken == $"{reference.StartingChapter}:{reference.StartingVerse}-{reference.EndingChapter}{reference.EndingVerse}")
                {
                    string newSpanToken = $"{reference.StartingChapter}:{reference.StartingVerse}-{reference.EndingChapter}:{reference.EndingVerse}";
                    reference.AsString = reference.AsString.Replace(spanToken, newSpanToken);
                }
            }

            return new VerseResult { Reference = reference, Title = PurifyText(title), PsalmTitle = "", Text = PurifyText(text) };
        }

        public async Task<VerseResult> GetVerse(string reference, bool titlesEnabled, bool verseNumbersEnabled, Version version) => await GetVerse(new Reference { Book = null, Version = version, AsString = reference }, titlesEnabled, verseNumbersEnabled);

        public async Task<List<SearchResult>> Search(string query, Version version)
        {
            string url = string.Format(_searchURI, version.InternalId, query);

            ABSearchResponse resp = await _httpClient.GetJsonContentAs<ABSearchResponse>(url, _jsonOptions);
            List<SearchResult> results = [];

            if (resp != null && resp.Data != null && resp.Data.Verses != null)
            {
                results.AddRange(resp.Data.Verses.Select(verse => new SearchResult
                {
                    Reference = verse.Reference,
                    Text = PurifyText(verse.Text).Replace(query, $"**{query}**")
                }));
            }

            return results;
        }

        private static string PurifyText(string text) => TextPurificationService.PurifyText(text);
    }
}
