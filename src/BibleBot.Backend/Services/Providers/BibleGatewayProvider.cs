using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using AngleSharp.Html.Parser;

using BibleBot.Lib;
using BibleBot.Backend.Models;

namespace BibleBot.Backend.Services.Providers
{
    public class BibleGatewayProvider : IBibleProvider
    {
        public string Name { get; set; }
        private readonly CancellationTokenSource _cancellationToken;
        private readonly HttpClient _httpClient;
        private readonly HtmlParser _htmlParser;
        private readonly string _baseURL = "https://www.biblegateway.com";
        private readonly string _getURI = "/passage/?search={0}&version={1}&interface=print";
        private readonly string _votdURI = "/reading-plans/verse-of-the-day/next";

        private static readonly System.Random _random = new System.Random();

        public BibleGatewayProvider()
        {
            Name = "bg";

            _cancellationToken = new CancellationTokenSource();
            _httpClient = new HttpClient();
            _htmlParser = new HtmlParser();
        }

        public async Task<Verse> GetVerse(Reference reference, bool titlesEnabled, bool verseNumbersEnabled)
        {
            if (reference.Book != "str")
            {
                reference.AsString = reference.ToString();
            }

            string url = _baseURL + System.String.Format(_getURI, reference.AsString, reference.Version.Abbreviation);
            
            HttpResponseMessage req = await _httpClient.GetAsync(url);
            _cancellationToken.Token.ThrowIfCancellationRequested();

            Stream resp = await req.Content.ReadAsStreamAsync();
            _cancellationToken.Token.ThrowIfCancellationRequested();

            var document = await _htmlParser.ParseDocumentAsync(resp);
            _cancellationToken.Token.ThrowIfCancellationRequested();

            var container = document.QuerySelector(".result-text-style-normal");

            if (container == null)
            {
                return null;
            }

            var chapterNumbers = container.QuerySelectorAll(".chapternum");
            var verseNumbers = container.QuerySelectorAll(".versenum");

            foreach (var el in chapterNumbers) 
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

            foreach (var el in verseNumbers)
            {
                if (verseNumbersEnabled)
                {
                    el.TextContent = $" <**{el.TextContent.Substring(0, el.TextContent.Length - 1)}**> ";
                }
                else
                {
                    el.Remove();
                }
            }

            foreach (var el in document.QuerySelectorAll("br"))
            {
                el.Before(document.CreateTextNode("\n"));
                el.Remove();
            }

            foreach (var el in document.QuerySelectorAll(".crossreference"))
            {
                el.Remove();
            }

            foreach (var el in document.QuerySelectorAll(".footnote"))
            {
                el.Remove();
            }

            foreach (var el in document.QuerySelectorAll(".copyright-table"))
            {
                el.Remove();
            }

            // In the event that the line-break replacements above don't account for everything...
            foreach (var el in document.QuerySelectorAll(".text"))
            {
                el.TextContent = $" {el.TextContent} ";
            }

            string title = titlesEnabled ? System.String.Join(" / ", container.GetElementsByTagName("h3").Select(el => el.TextContent.Trim())) : null;
            string text = System.String.Join("\n", container.GetElementsByTagName("p").Select(el => el.TextContent.Trim()));
            string psalmTitle = titlesEnabled ? System.String.Join(" / ", container.GetElementsByClassName("psalm-title").Select(el => el.TextContent.Trim())) : null;

            return new Verse { Reference = reference, Title = title, PsalmTitle = psalmTitle, Text = PurifyVerseText(text) };
        }

        public async Task<Verse> GetVerse(string reference, bool titlesEnabled, bool verseNumbersEnabled, Version version)
        {
            return await GetVerse(new Reference{ Book = "str", Version = version, AsString = reference }, titlesEnabled, verseNumbersEnabled);
        }

        public async Task<Dictionary<string, string>> Search(string query, Version version)
        {
            // TODO
            return new Dictionary<string, string>();
        }

        // These next three functions should probably get moved elsewhere.
        public async Task<string> GetDailyVerse()
        {
            string url = _baseURL + _votdURI;
            
            HttpResponseMessage req = await _httpClient.GetAsync(url);
            _cancellationToken.Token.ThrowIfCancellationRequested();

            Stream resp = await req.Content.ReadAsStreamAsync();
            _cancellationToken.Token.ThrowIfCancellationRequested();

            var document = await _htmlParser.ParseDocumentAsync(resp);
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

            var document = await _htmlParser.ParseDocumentAsync(resp);
            _cancellationToken.Token.ThrowIfCancellationRequested();
            
            return document.GetElementsByClassName("b1").FirstOrDefault().GetElementsByClassName("vr").FirstOrDefault().GetElementsByClassName("vc").FirstOrDefault().TextContent;
        }

        public async Task<string> GetTrulyRandomVerse()
        {
            var verseNumber = _random.Next(0, 31102);
            string url = $"https://biblebot.github.io/RandomVersesData/{verseNumber}.txt";
            
            HttpResponseMessage req = await _httpClient.GetAsync(url);
            _cancellationToken.Token.ThrowIfCancellationRequested();

            string resp = await req.Content.ReadAsStringAsync();
            _cancellationToken.Token.ThrowIfCancellationRequested();

            var verseArray = resp.Split(" ").Take(2);

            var bookMap = new Dictionary<string, string>
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

            var book = verseArray.ElementAt(0);

            if (bookMap.ContainsKey(book))
            {
                book = bookMap[book];
            }

            return $"{book} {verseArray.ElementAt(1)}";
        }

        private string PurifyVerseText(string text)
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