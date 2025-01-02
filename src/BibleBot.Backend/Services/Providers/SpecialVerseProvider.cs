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
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;
using AngleSharp.Html.Dom;
using System;

namespace BibleBot.Backend.Services.Providers
{
    public class SpecialVerseProvider : IDisposable
    {
        private CancellationTokenSource _cancellationToken;
        private readonly HttpClient _httpClient;
        private readonly HtmlParser _htmlParser;

        public SpecialVerseProvider()
        {
            _cancellationToken = new CancellationTokenSource();
            _httpClient = new HttpClient();
            _htmlParser = new HtmlParser();
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

        public async Task<string> GetDailyVerse()
        {
            string url = "https://www.biblegateway.com/reading-plans/verse-of-the-day/next";

            HttpResponseMessage req = await _httpClient.GetAsync(url);
            _cancellationToken.Token.ThrowIfCancellationRequested();

            Stream resp = await req.Content.ReadAsStreamAsync();
            _cancellationToken.Token.ThrowIfCancellationRequested();

            IHtmlDocument document = await _htmlParser.ParseDocumentAsync(resp);
            _cancellationToken.Token.ThrowIfCancellationRequested();

            return document.GetElementsByClassName("rp-passage-display").FirstOrDefault().TextContent;
        }

        public async Task<string> GetRandomVerse()
        {
            string url = "https://dailyverses.net/random-bible-verse";

            HttpResponseMessage req = await _httpClient.GetAsync(url);
            _cancellationToken.Token.ThrowIfCancellationRequested();

            Stream resp = await req.Content.ReadAsStreamAsync();
            _cancellationToken.Token.ThrowIfCancellationRequested();

            IHtmlDocument document = await _htmlParser.ParseDocumentAsync(resp);
            _cancellationToken.Token.ThrowIfCancellationRequested();

            return document.GetElementsByClassName("b1").FirstOrDefault().GetElementsByClassName("vr").FirstOrDefault().GetElementsByClassName("vc").FirstOrDefault().TextContent;
        }

        public async Task<string> GetTrulyRandomVerse()
        {
            int verseNumber = RandomNumberGenerator.GetInt32(0, 31102);
            string url = $"https://biblebot.github.io/RandomVersesData/{verseNumber}.txt";

            HttpResponseMessage req = await _httpClient.GetAsync(url);
            _cancellationToken.Token.ThrowIfCancellationRequested();

            string resp = await req.Content.ReadAsStringAsync();
            _cancellationToken.Token.ThrowIfCancellationRequested();

            IEnumerable<string> verseArray = resp.Split(" ").Take(2);

            Dictionary<string, string> bookMap = new()
            {
                { "Sa1", "1 Samuel" },
                { "Sa2", "2 Samuel" },
                { "Kg1", "1 Kings" },
                { "Kg2", "2 Kings" },
                { "Ch1", "1 Chronicles" },
                { "Ch2", "2 Chronicles" },
                { "Co1", "1 Corinthians" },
                { "Co2", "2 Corinthians" },
                { "Th1", "1 Thessalonians" },
                { "Th2", "2 Thessalonians" },
                { "Ti1", "1 Timothy" },
                { "Ti2", "2 Timothy" },
                { "Pe1", "1 Peter" },
                { "Pe2", "2 Peter" },
                { "Jo1", "1 John" },
                { "Jo2", "2 John" },
                { "Jo3", "3 John" }
            };

            string book = verseArray.ElementAt(0);

            if (bookMap.ContainsKey(book))
            {
                book = bookMap[book];
            }

            return $"{book} {verseArray.ElementAt(1)}";
        }
    }
}
