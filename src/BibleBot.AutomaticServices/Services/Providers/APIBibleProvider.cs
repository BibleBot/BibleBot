/*
* Copyright (C) 2016-2021 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;
using BibleBot.AutomaticServices.Models;
using BibleBot.Lib;
using RestSharp;
using RestSharp.Serializers.SystemTextJson;
using Serilog;

namespace BibleBot.AutomaticServices.Services.Providers
{
    public class APIBibleProvider : IBibleProvider
    {
        public string Name { get; set; }
        private readonly RestClient _restClient;
        private readonly HtmlParser _htmlParser;

        private readonly Dictionary<string, string> _versionTable;

        private readonly string _baseURL = "https://api.scripture.api.bible/v1";
        private readonly string _getURI = "bibles/{0}/search?query={1}&limit=100";

        public APIBibleProvider()
        {
            Name = "ab";

            _restClient = new RestClient(_baseURL);
            _restClient.UseSystemTextJson(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            _htmlParser = new HtmlParser();

            _versionTable = new Dictionary<string, string>
            {
                { "KJVA", "de4e12af7f28f599-01" }, // King James Version with Apocrypha
                { "FBV", "65eec8e0b60e656b-01" } // Free Bible Version
            };
        }

        public async Task<Verse> GetVerse(Reference reference, bool titlesEnabled, bool verseNumbersEnabled)
        {
            if (reference.Book != "str")
            {
                if (reference.Version.Abbreviation == "KJVA" && reference.Book == "Song of Songs")
                {
                    reference.Book = "Song of Solomon";
                }
                reference.AsString = reference.ToString();
            }

            string url = System.String.Format(_getURI, _versionTable[reference.Version.Abbreviation], reference.AsString);

            var req = new RestRequest(url);
            req.AddHeader("api-key", System.Environment.GetEnvironmentVariable("APIBIBLE_TOKEN"));

            ABSearchResponse resp = await _restClient.GetAsync<ABSearchResponse>(req);

            if (resp.Data == null)
            {
                return null;
            }

            if (resp.Data.Passages[0].BibleId != _versionTable[reference.Version.Abbreviation])
            {
                Log.Error($"{reference.Version.Abbreviation} machine broke");
                return null;
            }

            if (resp.Data.Passages[0].Content.Length < 1)
            {
                return null;
            }

            var document = await _htmlParser.ParseDocumentAsync(resp.Data.Passages[0].Content);

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

            string title = titlesEnabled ? System.String.Join(" / ", document.GetElementsByTagName("h3").Select(el => el.TextContent.Trim())) : "";
            string text = System.String.Join("\n", document.GetElementsByTagName("p").Select(el => el.TextContent.Trim()));

            // As the verse reference could have a non-English name...
            reference.AsString = resp.Data.Passages[0].Reference;

            return new Verse { Reference = reference, Title = PurifyText(title), PsalmTitle = "", Text = PurifyText(text) };
        }

        public async Task<Verse> GetVerse(string reference, bool titlesEnabled, bool verseNumbersEnabled, Version version)
        {
            return await GetVerse(new Reference { Book = "str", Version = version, AsString = reference }, titlesEnabled, verseNumbersEnabled);
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
