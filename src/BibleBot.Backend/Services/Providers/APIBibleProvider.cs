/*
* Copyright (C) 2016-2023 Kerygma Digital Co.
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
using AngleSharp.Html.Parser;
using BibleBot.Models;
using Serilog;

namespace BibleBot.Backend.Services.Providers
{
    public class APIBibleProvider : IBibleProvider
    {
        public string Name { get; set; }
        private readonly HttpClient _cachingHttpClient;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly HtmlParser _htmlParser;

        private readonly Dictionary<string, string> _versionTable;

        private readonly string _baseURL = "https://api.scripture.api.bible/v1/";
        private readonly string _getURI = "bibles/{0}/search?query={1}&limit=100";
        private readonly string _searchURI = "bibles/{0}/search?query={1}&limit=100&sort=relevance";

        public APIBibleProvider()
        {
            Name = "ab";

            _cachingHttpClient = CachingClient.GetTrimmedCachingClient(_baseURL, false);
            _cachingHttpClient.DefaultRequestHeaders.Add("api-key", System.Environment.GetEnvironmentVariable("APIBIBLE_TOKEN"));
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("api-key", System.Environment.GetEnvironmentVariable("APIBIBLE_TOKEN"));

            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            _htmlParser = new HtmlParser();

            _versionTable = new Dictionary<string, string>
            {
                { "KJVA", "de4e12af7f28f599-01" }, // King James Version with Apocrypha
                { "FBV", "65eec8e0b60e656b-01" }, // Free Bible Version
                { "WEB", "9879dbb7cfe39e4d-01" }, // World English Bible (includes Apocrypha)
                { "LXX", "c114c33098c4fef1-01" }, // Brenton's Greek Septuagint
                { "ELXX", "6bab4d6c61b31b80-01" }, // Brenton's English Septuagint
                { "PAT1904", "901dcd9744e1bf69-01" } // Patriarchal Text of 1904
            };
        }

        public async Task<Verse> GetVerse(Reference reference, bool titlesEnabled, bool verseNumbersEnabled)
        {
            // todo: handle Psalm 151 properly (if not already usable)
            if (reference.Book != "str")
            {
                if (reference.Version.Abbreviation == "KJVA" && reference.Book == "Song of Songs")
                {
                    reference.Book = "Song of Solomon";
                }

                if (reference.Version.Abbreviation == "ELXX" || reference.Version.Abbreviation == "LXX")
                {
                    if (reference.Book == "Daniel")
                    {
                        reference.Book = "DAG";
                    }
                }

                reference.AsString = reference.ToString();
            }

            string url = string.Format(_getURI, _versionTable[reference.Version.Abbreviation], reference.AsString);

            // Benchmarking/Debugging, TODO: remove when ready
            System.Console.WriteLine("---");
            System.Console.WriteLine($"{reference}");

            var resp = await _cachingHttpClient.GetJsonContentAs<ABSearchData>(url, _jsonOptions);

            if (resp == null)
            {
                return null;
            }

            if (resp.Passages.Count() == 0)
            {
                Log.Error($"{reference.Version.Abbreviation} machine broke");
                return null;
            }

            if (resp.Passages[0].BibleId != _versionTable[reference.Version.Abbreviation])
            {
                Log.Error($"{reference.Version.Abbreviation} machine broke - version no longer available");
                return null;
            }

            if (resp.Passages[0].Content.Length < 1)
            {
                return null;
            }

            var document = await _htmlParser.ParseDocumentAsync(resp.Passages[0].Content);

            var numbers = document.QuerySelectorAll(".v");

            foreach (var el in numbers)
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

            string title = titlesEnabled ? string.Join(" / ", document.GetElementsByTagName("h3").Select(el => el.TextContent.Trim())) : "";
            string text = string.Join("\n", document.GetElementsByTagName("p").Select(el => el.TextContent.Trim()));

            // As the verse reference could have a non-English name...
            reference.AsString = resp.Passages[0].Reference;

            if (reference.AsString.Contains("Daniel (Greek)") || reference.AsString.Contains("ΔΑΝΙΗΛ (Ελληνικά)"))
            {
                reference.Book = "Daniel";
                reference.AsString = reference.ToString();
            }

            return new Verse { Reference = reference, Title = PurifyText(title), PsalmTitle = "", Text = PurifyText(text) };
        }

        public async Task<Verse> GetVerse(string reference, bool titlesEnabled, bool verseNumbersEnabled, Version version)
        {
            return await GetVerse(new Reference { Book = "str", Version = version, AsString = reference }, titlesEnabled, verseNumbersEnabled);
        }

        public async Task<List<SearchResult>> Search(string query, Version version)
        {
            string url = string.Format(_searchURI, _versionTable[version.Abbreviation], query);

            ABSearchResponse resp = await _httpClient.GetJsonContentAs<ABSearchResponse>(url, _jsonOptions);

            var results = new List<SearchResult>();

            if (resp != null)
            {
                foreach (var verse in resp.Data.Verses)
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

        private string PurifyText(string text)
        {
            Dictionary<string, string> nuisances = new Dictionary<string, string>
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
                { " . ",   " " },
            };

            if (text.Contains("Selah."))
            {
                text = text.Replace("Selah.", " *(Selah)* ");
            }
            else if (text.Contains("Selah"))
            {
                text = text.Replace("Selah", " *(Selah)* ");
            }

            foreach (var pair in nuisances)
            {
                if (text.Contains(pair.Key))
                {
                    text = text.Replace(pair.Key, pair.Value);
                }
            }

            text = Regex.Replace(text, @"\s+", " ");

            return text.Trim();
        }
    }
}
