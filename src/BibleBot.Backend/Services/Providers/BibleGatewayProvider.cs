using System.Net;
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
        private readonly WebClient _webClient;
        private readonly string _baseURL = "https://www.biblegateway.com";
        private readonly string _getURI = "/passage/?search={0}&version={1}&interface=print";

        public BibleGatewayProvider()
        {
            Name = "bg";
            _webClient = new WebClient();
        }

        public async Task<Verse> GetVerse(Reference reference, bool titlesEnabled, bool verseNumbersEnabled)
        {
            string url = _baseURL + System.String.Format(_getURI, reference.ToString(), reference.Version.Abbreviation);
            string resp = _webClient.DownloadString(url);

            var document = await BrowsingContext.New().OpenAsync(req => req.Content(resp));
            var container = document.QuerySelector(".result-text-style-normal");

            var chapterNumbers = container.QuerySelectorAll(".chapternum");
            var verseNumbers = container.QuerySelectorAll(".versenum");

            var undesirableContent = document.All.Where(el => el.TagName == "br" || el.ClassList.Contains("crossreference") || el.ClassList.Contains("footnote"));

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

            foreach (var el in undesirableContent)
            {
                if (el.TagName == "br")
                {
                    el.Before(document.CreateTextNode("\n"));
                }

                el.Remove();
            }

            // Logic would suggest that I can just add "el.ClassList.Contains("copyright-table")" to line 37's lookup,
            // but for some reason it creates an ArgumentOutOfRangeException, so I have to do this.
            foreach (var el in document.QuerySelectorAll(".copyright-table"))
            {
                el.Remove();
            }

            string title = titlesEnabled ? System.String.Join(" / ", container.GetElementsByTagName("h3").Select(el => el.TextContent.Trim())) : null;
            string text = System.String.Join("\n", container.GetElementsByTagName("p").Select(el => el.TextContent.Trim()));
            string psalmTitle = null;

            foreach (var el in container.QuerySelectorAll(".psalm-title"))
            {
                psalmTitle = el.TextContent;
            }

            return new Verse { Reference = reference, Title = title, PsalmTitle = psalmTitle, Text = PurifyVerseText(text) };
        }

        public async Task<Dictionary<string, string>> Search(string query, Version version)
        {
            // TODO
            return new Dictionary<string, string>();
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