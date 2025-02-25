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

namespace BibleBot.Backend.Services.Providers
{
    public partial class NLTAPIProvider : IBibleProvider, IDisposable
    {
        public string Name { get; set; }

        private readonly Dictionary<string, string> _nameMapping;

        private CancellationTokenSource _cancellationToken;
        private readonly HttpClient _cachingHttpClient;
        private readonly HttpClient _httpClient;
        private readonly HtmlParser _htmlParser;

        private readonly string _baseURL = "https://api.nlt.to/api/";
        private readonly string _getURI = "passages?ref={0}&key={1}&version=NLT";
        private readonly string _searchURI = "search?text={0}&key={1}&version=NLT";

        public NLTAPIProvider(NameFetchingService nameFetchingService)
        {
            Name = "nlt";

            _nameMapping = nameFetchingService.GetNLTMapping();

            _cancellationToken = new CancellationTokenSource();

            _cachingHttpClient = new HttpClient { BaseAddress = new Uri(_baseURL) };
            _httpClient = new HttpClient { BaseAddress = new Uri(_baseURL) };

            _htmlParser = new HtmlParser();
        }

        [GeneratedRegex("[a-zA-Z0-9]{4}_([0-9]{1,3})_([0-9]{1,3})")]
        private static partial Regex VerseIdRegex();

        public async Task<Verse> GetVerse(Reference reference, bool titlesEnabled, bool verseNumbersEnabled)
        {
            if (reference.Book != "str")
            {
                reference.AsString = reference.ToString();
            }

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

            return new Verse { Reference = reference, Title = PurifyText(title, false), PsalmTitle = PurifyText(psalmTitle, false), Text = PurifyText(text, false) };
        }

        public async Task<Verse> GetVerse(string reference, bool titlesEnabled, bool verseNumbersEnabled, Models.Version version) => await GetVerse(new Reference { Book = "str", Version = version, AsString = reference }, titlesEnabled, verseNumbersEnabled);

        [GeneratedRegex("\\*[0-9:A-Za-z\\ .\\;].*$")]
        private static partial Regex CrossReferenceRegex();

        public async Task<List<SearchResult>> Search(string query, Models.Version version)
        {
            string url = string.Format(_searchURI, query, Environment.GetEnvironmentVariable("NLTAPI_TOKEN"));

            HttpResponseMessage req = await _httpClient.GetAsync(url);
            _cancellationToken.Token.ThrowIfCancellationRequested();

            Stream resp = await req.Content.ReadAsStreamAsync();
            _cancellationToken.Token.ThrowIfCancellationRequested();

            IHtmlDocument document = await _htmlParser.ParseDocumentAsync(resp);
            _cancellationToken.Token.ThrowIfCancellationRequested();

            List<SearchResult> results = [];

            foreach (IElement row in document.QuerySelectorAll("tr"))
            {
                foreach (IElement el in row.GetElementsByTagName("td"))
                {
                    Match crossRef = CrossReferenceRegex().Match(el.TextContent);
                    if (crossRef.Success)
                    {
                        el.TextContent = el.TextContent.Replace(crossRef.Value, "");
                    }
                }

                IElement referenceElement = row.GetElementsByTagName("td").FirstOrDefault();
                IElement textElement = row.GetElementsByTagName("td").Skip(1).FirstOrDefault();

                if (referenceElement != null && textElement != null)
                {
                    string text = PurifyText(textElement.TextContent, false);
                    text = text.Replace(query, $"**{query}**");

                    if (text.Contains(query))
                    {
                        results.Add(new SearchResult
                        {
                            Reference = referenceElement.TextContent,
                            Text = text
                        });
                    }
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
