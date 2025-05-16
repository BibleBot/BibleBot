/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using BibleBot.Models;
using Version = BibleBot.Models.Version;

namespace BibleBot.Backend.Services.Providers.Content
{
    public partial class NLTAPIProvider : IContentProvider, IDisposable
    {
        public string Name { get; set; }
        private CancellationTokenSource _cancellationToken;
        private readonly HttpClient _cachingHttpClient;
        private readonly HttpClient _httpClient;
        private readonly HtmlParser _htmlParser;
        private readonly string _baseURL = "https://api.nlt.to/api/";
        private readonly string _getURI = "passages?ref={0}&key={1}&version=NLT";
        private readonly string _searchURI = "search?text={0}&key={1}&version=NLT";

        public NLTAPIProvider()
        {
            Name = "nlt";

            _cancellationToken = new CancellationTokenSource();

            _cachingHttpClient = new HttpClient { BaseAddress = new Uri(_baseURL) };
            _httpClient = new HttpClient { BaseAddress = new Uri(_baseURL) };

            _htmlParser = new HtmlParser();
        }

        [GeneratedRegex("[a-zA-Z0-9]{4}_([0-9]{1,3})_([0-9]{1,3})")]
        private static partial Regex VerseIdRegex();

        public async Task<VerseResult> GetVerse(Reference reference, bool titlesEnabled, bool verseNumbersEnabled)
        {
            reference.AsString ??= reference.ToString();

            string url = string.Format(_getURI, reference.AsString, Environment.GetEnvironmentVariable("NLTAPI_TOKEN"));

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

            foreach (IElement el in document.QuerySelectorAll(".chapter-number"))
            {
                if (verseNumbersEnabled)
                {
                    string chapterNum = el.QuerySelector(".cw_ch").TextContent;

                    el.TextContent = chapterNum != "1" && chapterNum != $"{reference.StartingChapter}" ? $" <**{chapterNum}:1**> " : " <**1**> ";

                }
                else
                {
                    el.Remove();
                }
            }

            foreach (IElement el in document.QuerySelectorAll(".vn"))
            {
                if (verseNumbersEnabled)
                {
                    IElement previousElement = el.ParentElement.PreviousElementSibling;

                    if (previousElement != null)
                    {
                        if (previousElement.ClassList.Contains("subhead"))
                        {
                            previousElement = previousElement.PreviousElementSibling;
                        }

                        if (previousElement.ClassList.Contains("chapter-number"))
                        {
                            // Prevent number duplication for verse 1s.
                            previousElement.Remove();
                        }
                    }

                    if (el.TextContent == "1")
                    {
                        IElement parentElement = el.ParentElement.ParentElement;
                        string verseId = parentElement.GetAttribute("orig");

                        if (verseId != null)
                        {
                            MatchCollection matches = VerseIdRegex().Matches(verseId);

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
#pragma warning restore IDE0045 // Convert to conditional expression
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

            foreach (IElement el in document.QuerySelectorAll("br"))
            {
                el.Before(document.CreateTextNode("\n"));
                el.Remove();
            }

            foreach (IElement el in document.QuerySelectorAll(".a-tn, .tn, .tn-ref"))
            {
                el.Remove();
            }

            foreach (IElement el in document.GetElementsByClassName("bk_ch_vs_header"))
            {
                if (el.ParentElement.TagName == "VERSE_EXPORT")
                {
                    IElement nextSection = el.ParentElement.ParentElement.NextElementSibling;

                    nextSection.InsertBefore(el.Clone());

                    el.Remove();
                }
            }

            string title = "";
            string psalmTitle = "";
            if (titlesEnabled)
            {
                title = string.Join(" / ", document.GetElementsByTagName("h3").Select(el => el.TextContent.Trim()));
                psalmTitle = string.Join(" / ", document.GetElementsByClassName("psalm-title").Select(el => el.TextContent.Trim()));
            }

            foreach (IElement el in document.GetElementsByTagName("h3"))
            {
                el.Remove();
            }

            StringBuilder textBuilder = new();
            foreach (string textPiece in document.GetElementsByTagName("VERSE_EXPORT").Select(el => el.TextContent.Trim()))
            {
                if (!textBuilder.ToString().Contains(textPiece))
                {
                    textBuilder.AppendLine(textPiece);
                }
            }
            string text = textBuilder.ToString().Trim();

            string refString = document.GetElementsByClassName("bk_ch_vs_header").FirstOrDefault().TextContent;
            reference.AsString = refString.Substring(0, refString.Length - 5);

            if (reference.AppendedVerses.Count > 0)
            {
                foreach (IElement referenceEl in document.GetElementsByClassName("bk_ch_vs_header").Skip(1))
                {
                    string referenceTrimmed = referenceEl.TextContent.Substring(0, referenceEl.TextContent.Length - 5);

                    if (referenceTrimmed.Contains(':') && referenceTrimmed.Contains(reference.AsString.Split(" ")[0]))
                    {
                        string[] colonSplit = referenceTrimmed.Split(":");

                        reference.AsString += $", {colonSplit[1]}";
                    }
                }
            }
            else if (reference.StartingChapter != reference.EndingChapter && reference.EndingVerse == 1)
            {
                reference.AsString += ":1";
            }

            return new VerseResult { Reference = reference, Title = PurifyText(title), PsalmTitle = PurifyText(psalmTitle), Text = PurifyText(text) };
        }

        public async Task<VerseResult> GetVerse(string reference, bool titlesEnabled, bool verseNumbersEnabled, Version version) => await GetVerse(new Reference { Book = null, Version = version, AsString = reference }, titlesEnabled, verseNumbersEnabled);

        public async Task<List<SearchResult>> Search(string query, Version version)
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
                    string text = PurifyText(textElement.TextContent.Substring(1, textElement.TextContent.Length - 1));
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
