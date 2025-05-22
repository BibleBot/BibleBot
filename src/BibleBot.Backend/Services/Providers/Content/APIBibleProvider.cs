/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

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
using Serilog;

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
            _cachingHttpClient.DefaultRequestHeaders.Add("api-key", System.Environment.GetEnvironmentVariable("APIBIBLE_TOKEN"));
            _httpClient = new HttpClient { BaseAddress = new System.Uri(_baseURL) };
            _httpClient.DefaultRequestHeaders.Add("api-key", System.Environment.GetEnvironmentVariable("APIBIBLE_TOKEN"));

            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

            _htmlParser = new HtmlParser();
        }

        [GeneratedRegex("[a-zA-Z]{3} ([0-9]{1,3}):([0-9]{1,3})")]
        private static partial Regex VerseIdRegex();

        public async Task<VerseResult> GetVerse(Reference reference, bool titlesEnabled, bool verseNumbersEnabled)
        {
            string originalProperName;

            string[] solidTextClasses = ["d", "m", "cls", "mi"];
            string[] prefixTextClasses = ["q", "p", "add", "l"];

            if (reference.AsString == null)
            {
                try
                {
                    originalProperName = reference.Book.ProperName;
                }
                catch (System.NullReferenceException)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        Log.Error($"***** {reference.Version.Name} does not have book data populated, no references will work until resolved! *****");
                    }

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
            }
            else
            {
                string[] tokenizedReference = reference.AsString.Split(" ");
                originalProperName = string.Join(" ", tokenizedReference.Take(tokenizedReference.Length - 1));

                reference.Book = reference.Version.Books.First(book => book.ProperName == originalProperName);
                reference.AsString = reference.AsString.Replace(originalProperName, reference.Book.Name);
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

                    if (verseEl != null)
                    {
                        el.NextElementSibling.Prepend(verseEl.Clone());

                        verseEl.Remove();
                    }

                    el.Remove();
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
#pragma warning disable IDE0045 // Convert to conditional expression
                            if (matches[0].Groups[2].Value == "1")
                            {
                                if (matches[0].Groups[1].Value == "1" || matches[0].Groups[1].Value == $"{reference.StartingChapter}")
                                {
                                    el.TextContent = " <**1**> ";
                                }
                                else
                                {
                                    el.TextContent = $" <**{matches[0].Groups[1].Value}:1**> ";
                                }
                            }
                            else
                            {
                                el.TextContent = $" <**{el.TextContent}**> ";
                            }
                        }
                        else
                        {
                            el.TextContent = $" <**{el.TextContent}**> ";
                        }
#pragma warning restore IDE0045 // Convert to conditional expression
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

            if (resp.Data == null)
            {
                return results;
            }

            results.AddRange(resp.Data.Verses.Select(verse => new SearchResult
            {
                Reference = verse.Reference,
                Text = PurifyText(verse.Text).Replace(query, $"**{query}**")
            }));

            return results;
        }

        [GeneratedRegex(@"\s+")]
        private static partial Regex MultipleWhitespacesGeneratedRegex();
        private static string PurifyText(string text)
        {
            Dictionary<string, string> nuisances = new()
            {
                { "“",     "\"" },
                { "”",     "\"" },
                { "\n",    " " },
                { "\t",    " " },
                { "\v",    " " },
                { "\f",    " " },
                { "\r",    " " },
                { "¶ ",    "" },
                { " , ",   ", " },
                { " .",    "." },
                { "′",     "'" },
                { "‘",     "'" },
                { "’",     "'" }, // Fonts may make it look like this is no different from the line above, but it's a different codepoint in Unicode.
                { "' s",   "'s" },
                { "' \"",  "'\""},
                { " . ",   " " },
                { "*",     "\\*" },
                { "_",     "\\_" },
                { "\\*\\*", "**" },
                { "\\_\\_", "__" },
                { "\\*(Selah)\\*", "*(Selah)*"}
            };

            if (text.Contains("Selah."))
            {
                text = text.Replace("Selah.", " *(Selah)* ");
            }
            else if (text.Contains("Selah"))
            {
                text = text.Replace("Selah", " *(Selah)* ");
            }

            foreach (KeyValuePair<string, string> pair in nuisances.Where(pair => text.Contains(pair.Key)))
            {
                text = text.Replace(pair.Key, pair.Value);
            }

            text = MultipleWhitespacesGeneratedRegex().Replace(text, " ");

            return text.Trim();
        }
    }
}
