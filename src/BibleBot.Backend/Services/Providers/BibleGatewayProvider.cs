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
        private readonly string _searchURI = "/quicksearch/?search={0}&version={1}&searchtype=all&limit=50000&interface=print";

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

            foreach (var el in document.QuerySelectorAll(".footnotes"))
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

            string title = titlesEnabled ? System.String.Join(" / ", container.GetElementsByTagName("h3").Select(el => el.TextContent.Trim())) : "";
            string text = System.String.Join("\n", container.GetElementsByTagName("p").Select(el => el.TextContent.Trim()));
            string psalmTitle = titlesEnabled ? System.String.Join(" / ", container.GetElementsByClassName("psalm-title").Select(el => el.TextContent.Trim())) : "";

            // As the verse reference could have a non-English name...
            reference.AsString = document.GetElementsByClassName("bcv").FirstOrDefault().TextContent.Trim();

            return new Verse { Reference = reference, Title = title, PsalmTitle = psalmTitle, Text = PurifyVerseText(text) };
        }

        public async Task<Verse> GetVerse(string reference, bool titlesEnabled, bool verseNumbersEnabled, Version version)
        {
            return await GetVerse(new Reference{ Book = "str", Version = version, AsString = reference }, titlesEnabled, verseNumbersEnabled);
        }

        public async Task<List<SearchResult>> Search(string query, Version version)
        {
            string url = _baseURL + System.String.Format(_searchURI, query, version.Abbreviation);

            HttpResponseMessage req = await _httpClient.GetAsync(url);
            _cancellationToken.Token.ThrowIfCancellationRequested();

            Stream resp = await req.Content.ReadAsStreamAsync();
            _cancellationToken.Token.ThrowIfCancellationRequested();

            var document = await _htmlParser.ParseDocumentAsync(resp);
            _cancellationToken.Token.ThrowIfCancellationRequested();

            var results = new List<SearchResult>();

            foreach (var row in document.QuerySelectorAll(".row"))
            {
                foreach (var el in row.GetElementsByClassName("bible-item-extras"))
                {
                    el.Remove();
                }

                foreach (var el in row.GetElementsByTagName("h3"))
                {
                    el.Remove();
                }

                var referenceElement = row.GetElementsByClassName("bible-item-title").FirstOrDefault();
                var textElement = row.GetElementsByClassName("bible-item-text").FirstOrDefault();

                if (referenceElement != null && textElement != null)
                {
                    results.Add(new SearchResult
                    {
                        Reference = referenceElement.TextContent,
                        Text = PurifyVerseText(textElement.TextContent.Substring(1, textElement.TextContent.Length - 1))
                    });
                }
            }

            return results;
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