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

            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            _htmlParser = new HtmlParser();
        }

        public async Task<Verse> GetVerse(Reference reference, bool titlesEnabled, bool verseNumbersEnabled)
        {
            string[] oddTextClasses = ["m", "cls", "mi"];

            // todo: handle Psalm 151 properly (if not already usable)
            if (reference.Book != "str")
            {
                if (reference.Version.Abbreviation == "KJVA" && reference.Book == "Song of Songs")
                {
                    reference.Book = "Song of Solomon";
                }

                if (reference.Version.Abbreviation is "ELXX" or "LXX")
                {
                    if (reference.Book == "Daniel")
                    {
                        reference.Book = "DAG";
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
                    while (el.ChildNodes.Length > 1)
                    {
                        el.RemoveChild(el.ChildNodes[1]);
                    }
                }

                IHtmlCollection<IElement> numbers = document.QuerySelectorAll(".v");

                foreach (IElement el in numbers)
                {
                    if (verseNumbersEnabled)
                    {
                        el.TextContent = $" <**{el.TextContent}**> ";
                    }
                    else
                    {
                        el.Remove();
                    }
                }

                title += titlesEnabled ? string.Join(" / ", document.GetElementsByClassName("s1").Select(el => el.TextContent.Trim())) : "";
                texts.Add(string.Join("\n", document.GetElementsByTagName("p").Where(el => oddTextClasses.Contains(el.ClassName) || el.ClassName.StartsWith('q') || el.ClassName.StartsWith('p')).Select(el => el.TextContent.Trim())));
            }

            string text = string.Join("\n", texts);

            // As the verse reference could have a non-English name...
            reference.AsString = resp.Passages[0].Reference;

            if (resp.Passages.Count > 1)
            {
                for (int i = 1; i < resp.Passages.Count; i++)
                {
                    string[] colonSplit = resp.Passages[i].Reference.Split(':');

                    reference.AsString += $", {colonSplit[1]}";
                }
            }

            if (reference.AsString.Contains("Daniel (Greek)") || reference.AsString.Contains("ΔΑΝΙΗΛ (Ελληνικά)"))
            {
                reference.Book = "Daniel";
                reference.AsString = reference.ToString();
            }

            return new Verse { Reference = reference, Title = PurifyText(title), PsalmTitle = "", Text = PurifyText(text) };
        }

        public async Task<Verse> GetVerse(string reference, bool titlesEnabled, bool verseNumbersEnabled, Version version) => await GetVerse(new Reference { Book = "str", Version = version, AsString = reference }, titlesEnabled, verseNumbersEnabled);

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
