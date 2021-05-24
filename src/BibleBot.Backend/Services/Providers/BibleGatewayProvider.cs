using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using AngleSharp;

using BibleBot.Lib;
using BibleBot.Backend.Models;

namespace BibleBot.Backend.Services.Providers
{
    public class BibleGatewayProvider : IBibleProvider
    {
        public string Name { get; set; }
        private readonly HttpClient _httpClient;
        private readonly string _baseURL = "https://www.biblegateway.com";
        private readonly string _getURI = "/passage/?search={0}&version={1}&interface=print";
        private readonly string _votdURI = "/reading-plans/verse-of-the-day/next";

        public BibleGatewayProvider()
        {
            Name = "bg";
            _httpClient = new HttpClient();
        }

        public async Task<Verse> GetVerse(Reference reference, bool titlesEnabled, bool verseNumbersEnabled)
        {
            if (reference.Book != "str")
            {
                reference.AsString = reference.ToString();
            }

            string url = _baseURL + System.String.Format(_getURI, reference.AsString, reference.Version.Abbreviation);
            string resp = await _httpClient.GetStringAsync(url);

            var document = await BrowsingContext.New().OpenAsync(req => req.Content(resp));
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

        public async Task<string> GetDailyVerse()
        {
            string url = _baseURL + _votdURI;
            string resp = await _httpClient.GetStringAsync(url);

            var document = await BrowsingContext.New().OpenAsync(req => req.Content(resp));
            
            return document.GetElementsByClassName("rp-passage-display").FirstOrDefault().TextContent;
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