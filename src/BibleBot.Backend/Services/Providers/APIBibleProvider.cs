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

namespace BibleBot.Backend.Services.Providers
{
    public partial class APIBibleProvider : IBibleProvider
    {
        public string Name { get; set; }

        private readonly MDABBookMap _nameMapping;

        private readonly HttpClient _cachingHttpClient;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly HtmlParser _htmlParser;

        private readonly string _baseURL = "https://api.scripture.api.bible/v1/";
        private readonly string _getURI = "bibles/{0}/search?query={1}&limit=100";
        private readonly string _getBookURI = "bibles/{0}/books/{1}";
        private readonly string _searchURI = "bibles/{0}/search?query={1}&limit=100&sort=relevance";

        public APIBibleProvider(MetadataFetchingService metadataFetchingService)
        {
            Name = "ab";

            _nameMapping = metadataFetchingService.GetAPIBibleMapping();

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
            string defaultBookName = reference.Book;

            string[] solidTextClasses = ["d", "m", "cls", "mi"];
            string[] prefixTextClasses = ["q", "p", "add", "l"];

            if (reference.Book != "str")
            {
                reference.Book = _nameMapping.Keys.FirstOrDefault(key => _nameMapping[key] == reference.BookDataName);

                if (reference.Version.Abbreviation is "ELXX" or "LXX")
                {
                    if (reference.BookDataName == "dan")
                    {
                        reference.Book = "DAG";

                        // For whatever reason, the ELXX we use lists Daniel as a book
                        // but it actually doesn't exist, so we defer to the "updated" ELXX.
                        if (reference.Version.Abbreviation == "ELXX")
                        {
                            reference.Version.ApiBibleId = "6bab4d6c61b31b80-01";
                        }
                    }
                    else if (reference.BookDataName is "ezra" or "neh")
                    {
                        reference.Book = "EZR";
                        defaultBookName = "Ezra/Nehemiah";
                    }
                }

                reference.AsString = reference.ToString();
            }

            string url = string.Format(_getURI, reference.Version.ApiBibleId, reference.AsString);

            ABSearchData resp = await _cachingHttpClient.GetJsonContentAs<ABSearchData>(url, _jsonOptions);

            if (resp == null)
            {
                return null;
            }

            if (resp.Passages == null)
            {
                Log.Error($"{reference.Version.Abbreviation} machine broke");
                return null;
            }

            if (resp.Passages.Count == 0)
            {
                Log.Error($"{reference.Version.Abbreviation} machine broke");
                return null;
            }

            if (resp.Passages[0].BibleId != reference.Version.ApiBibleId)
            {
                Log.Error($"{reference.Version.Abbreviation} machine broke - version no longer available");
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
                        el.NextElementSibling.Prepend(verseEl.Clone(true));

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
                                if (matches[0].Groups[1].Value == "1")
                                {
                                    el.TextContent = " <**1**> ";
                                }
                                else if (matches[0].Groups[1].Value == $"{reference.StartingChapter}")
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
                texts.Add(string.Join("\n", document.GetElementsByTagName("p").Where(el => solidTextClasses.Contains(el.ClassName) || prefixTextClasses.Any(prefix => el.ClassName.StartsWith(prefix))).Select(el => el.TextContent.Trim())));
            }

            string text = string.Join("\n", texts);

            if (reference.Version.Abbreviation == "NLD1939" && text.Contains("tuchtmeester geweest tot Christus’ 3:opdat we"))
            {
                text = text.Replace("tuchtmeester geweest tot Christus’ 3:opdat we", "tuchtmeester geweest tot Christus' komst, opdat we");
            }

            // As the verse reference could have a non-English name...
            string bookUrl = string.Format(_getBookURI, reference.Version.ApiBibleId, resp.Passages[0].BookId);
            ABBookData bookResp = await _cachingHttpClient.GetJsonContentAs<ABBookData>(bookUrl, _jsonOptions);

            string properBookName = bookResp.Name.EndsWith('.') ? bookResp.NameLong : bookResp.Name;

            if (reference.Version.Abbreviation == "ELXX")
            {
                // Don't like version-specific workarounds, but given the naming convention
                // wackyness they've got here, this seems like the best course of action.
                properBookName = defaultBookName;
            }

            reference.AsString = resp.Passages[0].Reference.Replace(bookResp.Name, properBookName);

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

            reference.Book = defaultBookName;

            return new VerseResult { Reference = reference, Title = PurifyText(title), PsalmTitle = "", Text = PurifyText(text) };
        }

        public async Task<VerseResult> GetVerse(string reference, bool titlesEnabled, bool verseNumbersEnabled, Version version) => await GetVerse(new Reference { Book = "str", Version = version, AsString = reference }, titlesEnabled, verseNumbersEnabled);

        public async Task<List<SearchResult>> Search(string query, Version version)
        {
            string url = string.Format(_searchURI, version.ApiBibleId, query);

            ABSearchResponse resp = await _httpClient.GetJsonContentAs<ABSearchResponse>(url, _jsonOptions);

            List<SearchResult> results = [];

            if (resp.Data != null)
            {
                foreach (ABVerse verse in resp.Data.Verses)
                {
                    results.Add(new SearchResult
                    {
                        Reference = verse.Reference,
                        Text = PurifyText(verse.Text).Replace(query, $"**{query}**")
                    });
                }
            }

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
                { "’",     "'" }, // Fonts may make it look like this is no different than the line above, but it's a different codepoint in Unicode.
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

            foreach (KeyValuePair<string, string> pair in nuisances)
            {
                if (text.Contains(pair.Key))
                {
                    text = text.Replace(pair.Key, pair.Value);
                }
            }

            text = MultipleWhitespacesGeneratedRegex().Replace(text, " ");

            return text.Trim();
        }
    }
}
