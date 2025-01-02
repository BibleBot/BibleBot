/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;
using AngleSharp.Html.Dom;
using AngleSharp.Dom;
using BibleBot.Models;
using System;

namespace BibleBot.Backend.Services.Providers
{
    public partial class BibleGatewayProvider : IBibleProvider, IDisposable
    {
        public string Name { get; set; }
        private CancellationTokenSource _cancellationToken;
        private readonly HttpClient _cachingHttpClient;
        private readonly HttpClient _httpClient;
        private readonly HtmlParser _htmlParser;
        private readonly string _baseURL = "https://www.biblegateway.com/";
        private readonly string _getURI = "passage/?search={0}&version={1}&interface=print";
        private readonly string _searchURI = "quicksearch/?quicksearch={0}&qs_version={1}&resultspp=5000&interface=print";

        public BibleGatewayProvider()
        {
            Name = "bg";

            _cancellationToken = new CancellationTokenSource();

            _cachingHttpClient = CachingClient.GetTrimmedCachingClient(_baseURL, true);
            _cachingHttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.246");

            _httpClient = new HttpClient { BaseAddress = new Uri(_baseURL) };
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.246");

            _htmlParser = new HtmlParser();
        }

        public async Task<Verse> GetVerse(Reference reference, bool titlesEnabled, bool verseNumbersEnabled)
        {
            if (reference.Book != "str")
            {
                reference.AsString = reference.ToString();
            }

            string url = string.Format(_getURI, reference.AsString, reference.Version.Abbreviation);

            HttpResponseMessage req = await _cachingHttpClient.GetAsync(url);
            _cancellationToken.Token.ThrowIfCancellationRequested();

            if (req.StatusCode != System.Net.HttpStatusCode.OK) // bad request or not a verse
            {
                return null;
            }

            Stream resp = await req.Content.ReadAsStreamAsync();
            _cancellationToken.Token.ThrowIfCancellationRequested();

            IHtmlDocument document = await _htmlParser.ParseDocumentAsync(resp);
            _cancellationToken.Token.ThrowIfCancellationRequested();

            if (document == null)
            {
                return null;
            }

            foreach (IElement el in document.QuerySelectorAll(".chapternum"))
            {
                if (verseNumbersEnabled)
                {
                    el.TextContent = " <**1**> ";
                }
                else
                {
                    el.Remove();
                }
            }

            foreach (IElement el in document.QuerySelectorAll(".versenum"))
            {
                if (verseNumbersEnabled)
                {
                    IElement previousElement = el.PreviousElementSibling;

                    if (previousElement != null)
                    {
                        if (previousElement.ClassList.Contains("chapternum"))
                        {
                            // Prevent number duplication for verse 1s.
                            el.Remove();
                        }
                    }
                    else
                    {
                        el.TextContent = $" <**{el.TextContent.Substring(0, el.TextContent.Length - 1)}**> ";
                    }

                }
                else
                {
                    el.Remove();
                }
            }

            foreach (IElement el in document.QuerySelectorAll("br"))
            {
                el.Before(document.CreateTextNode("\n"));
                el.Remove();
            }

            foreach (IElement el in document.QuerySelectorAll(
                ".crossreference, .footnote, .footnotes, .copyright-table, .translation-note, .inline-h3, .psalm-acrostic, .psalm-acrostic-title, .psalm-book, h2"))
            {
                el.Remove();
            }

            // In the event that the line-break replacements above don't account for everything...
            foreach (IElement el in document.QuerySelectorAll(".text"))
            {
                el.TextContent = $" {el.TextContent} ";
            }

            string title = "";
            string psalmTitle = "";
            if (titlesEnabled)
            {
                title = string.Join(" / ", document.GetElementsByTagName("h3").Select(el => el.TextContent.Trim()));
                psalmTitle = string.Join(" / ", document.GetElementsByClassName("psalm-title").Select(el => el.TextContent.Trim()));
            }

            IEnumerable<IElement> headingElements = new[] {
                document.GetElementsByTagName("h3"), document.GetElementsByTagName("h4"),
                document.GetElementsByTagName("h5"), document.GetElementsByTagName("h6")
            }.SelectMany(x => x);

            foreach (IElement el in headingElements)
            {
                el.Remove();
            }

            foreach (IElement el in document.GetElementsByClassName("psalm-title"))
            {
                el.Remove();
            }

            string text = string.Join("\n", document.GetElementsByClassName("text").Select(el => el.TextContent.Trim()));

            // As the verse reference could have a non-English name...
            reference.AsString = document.GetElementsByClassName("dropdown-display-text").FirstOrDefault().TextContent.Trim();

            bool isISV = reference.Version.Abbreviation == "ISV";

            return new Verse { Reference = reference, Title = PurifyText(title, isISV), PsalmTitle = PurifyText(psalmTitle, isISV), Text = PurifyText(text, isISV) };
        }

        public async Task<Verse> GetVerse(string reference, bool titlesEnabled, bool verseNumbersEnabled, Models.Version version) => await GetVerse(new Reference { Book = "str", Version = version, AsString = reference }, titlesEnabled, verseNumbersEnabled);

        public async Task<List<SearchResult>> Search(string query, Models.Version version)
        {
            string url = string.Format(_searchURI, query, version.Abbreviation);

            HttpResponseMessage req = await _httpClient.GetAsync(url);
            _cancellationToken.Token.ThrowIfCancellationRequested();

            Stream resp = await req.Content.ReadAsStreamAsync();
            _cancellationToken.Token.ThrowIfCancellationRequested();

            IHtmlDocument document = await _htmlParser.ParseDocumentAsync(resp);
            _cancellationToken.Token.ThrowIfCancellationRequested();

            List<SearchResult> results = [];

            foreach (IElement row in document.QuerySelectorAll(".row"))
            {
                foreach (IElement el in row.GetElementsByClassName("bible-item-extras"))
                {
                    el.Remove();
                }

                foreach (IElement el in row.GetElementsByTagName("h3"))
                {
                    el.Remove();
                }

                IElement referenceElement = row.GetElementsByClassName("bible-item-title").FirstOrDefault();
                IElement textElement = row.GetElementsByClassName("bible-item-text").FirstOrDefault();

                if (referenceElement != null && textElement != null)
                {
                    string text = PurifyText(textElement.TextContent.Substring(1, textElement.TextContent.Length - 1), version.Abbreviation == "ISV");
                    text = text.Replace(query, $"**{query}**");

                    results.Add(new SearchResult
                    {
                        Reference = referenceElement.TextContent,
                        Text = text
                    });
                }
            }

            return results;
        }

        [GeneratedRegex(@"\s+")]
        private static partial Regex MultipleWhitespacesGeneratedRegex();
        private static string PurifyText(string text, bool isISV)
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
                { "' s",     "'s" },
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

            // I hate that I have to do this, but if I don't then ISV output gets fscked up...
            //
            // If you'd believe it, the ISV inserts Hebrew verse numbers into Exodus 20:1-17.
            // That's fine and all, but for some reason the subsequent verse number is placed
            // into the *preceding* verse. It's not even placed into the .versenum class, they
            // just append it into the previous verse's text. This is so stupidly hacky that
            // whoever implemented this needs to relearn HTML.
            //
            // The kicker? They use the transliterated name of the Hebrew letters in Psalm
            // 119 titles...
            if (isISV)
            {
                Dictionary<string, string> hebrewChars = new()
                {
                    { "א", "" },
                    { "ב", "" },
                    { "ג", "" },
                    { "ד", "" },
                    { "ה", "" },
                    { "ו", "" },
                    { "ז", "" },
                    { "ח", "" },
                    { "ט", "" },
                    { "י", "" },
                };

                foreach (KeyValuePair<string, string> pair in hebrewChars)
                {
                    if (text.Contains(pair.Key))
                    {
                        text = text.Replace(pair.Key, pair.Value);
                    }
                }
            }

            text = MultipleWhitespacesGeneratedRegex().Replace(text, " ");

            return text.Trim();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_cancellationToken != null)
                {
                    _cancellationToken.Dispose();
                    _cancellationToken = null;
                }
            }
        }
    }
}
