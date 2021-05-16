using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

using AngleSharp;

using BibleBot.Backend.Models;

namespace BibleBot.Backend.Services
{
    public class NameFetchingService
    {
        private readonly Dictionary<string, string> _apiBibleNames;
        private readonly Dictionary<string, List<string>> _abbreviations;
        private Dictionary<string, List<string>> _bookNames = new Dictionary<string, List<string>>();
        private List<string> _defaultNames;
        private readonly List<string> _nuisances;

        private WebClient _webClient;

        public NameFetchingService()
        {
            string apibibleNamesText = File.ReadAllText("./Data/NameFetching/apibible_names.json");
            _apiBibleNames = JsonSerializer.Deserialize<Dictionary<string, string>>(apibibleNamesText);

            string abbreviationsText = File.ReadAllText("./Data/NameFetching/abbreviations.json");
            _abbreviations = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(abbreviationsText);

            string defaultNamesText = File.ReadAllText("./Data/NameFetching/default_names.json");
            _defaultNames = JsonSerializer.Deserialize<List<string>>(defaultNamesText);

            string nuisancesText = File.ReadAllText("./Data/NameFetching/nuisances.json");
            _nuisances = JsonSerializer.Deserialize<List<string>>(nuisancesText);

            _webClient = new WebClient();
        }

        public Dictionary<string, List<string>> GetBookNames()
        {
            if (_bookNames.Count == 0)
            {
                string bookNamesText = File.ReadAllText("./Data/NameFetching/book_names.json");
                _bookNames = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(bookNamesText);
            }

            return _bookNames;
        }

        public List<string> GetDefaultBookNames()
        {
            if (_defaultNames.Count == 0)
            {
                string defaultNamesText = File.ReadAllText("./Data/NameFetching/default_names.json");
                _defaultNames = JsonSerializer.Deserialize<List<string>>(defaultNamesText);
            }

            return _defaultNames;
        }

        public async Task FetchBookNames(/*string apiBibleKey, */bool isDryRun)
        {
            if (isDryRun)
            {
                Console.WriteLine("NameFetchingService: DryRun");

                if (!File.Exists("./Data/NameFetching/book_names.json"))
                {
                    Console.WriteLine("NameFetchingService: Book names file does NOT exist, some references may not process.");
                }
                return;
            }

            Console.WriteLine("NameFetchingService: Getting BibleGateway versions...");
            var bgVersions = await GetBibleGatewayVersions();

            Console.WriteLine("NameFetchingService: Getting BibleGateway book names...");
            var bgNames = await GetBibleGatewayNames(bgVersions);

            if (File.Exists("./Data/NameFetching/book_names.json"))
            {
                File.Delete("./Data/NameFetching/book_names.json");
                Console.WriteLine("NameFetchingService: Removed old names file...");
            }

            var completedNames = MergeDictionaries(new List<Dictionary<string, List<string>>>{bgNames, _abbreviations});

            Console.WriteLine("NameFetchingService: Serializing and writing to file...");
            string serializedNames = JsonSerializer.Serialize(completedNames, new JsonSerializerOptions{ PropertyNameCaseInsensitive = false });
            File.WriteAllText("./Data/NameFetching/book_names.json", serializedNames);

            Console.WriteLine("NameFetchingService: Done.");
        }

        private async Task<Dictionary<string, string>> GetBibleGatewayVersions()
        {
            Dictionary<string, string> versions = new Dictionary<string, string>();

            string resp = _webClient.DownloadString("https://www.biblegateway.com/versions/");
            var document = await BrowsingContext.New().OpenAsync(req => req.Content(resp));
            
            var translationElements = document.All.Where(el => el.ClassList.Contains("translation-name"));
            foreach (var el in translationElements)
            {
                var targets = el.GetElementsByTagName("a");

                if (targets.Length == 1)
                {
                    if (targets[0].HasAttribute("href"))
                    {
                        versions.Add(targets[0].TextContent, $"https://www.biblegateway.com{targets[0].GetAttribute("href")}");
                    }
                }
            }

            return versions;
        }

        private async Task<Dictionary<string, List<string>>> GetBibleGatewayNames(Dictionary<string, string> versions)
        {
            Dictionary<string, List<string>> names = new Dictionary<string, List<string>>();

            List<string> threeMaccVariants = new List<string>{"3macc", "3m"};
            List<string> fourMaccVariants = new List<string>{"4macc", "4m"};
            List<string> greekEstherVariants = new List<string>{"gkesth", "adest", "addesth", "gkes"};
            List<string> prayerAzariahVariants = new List<string>{"sgthree", "sgthr", "prazar"};

            foreach (KeyValuePair<string, string> version in versions)
            {
                string resp = _webClient.DownloadString(version.Value);
                var document = await BrowsingContext.New().OpenAsync(req => req.Content(resp));

                var bookNames = document.All.Where(el => el.ClassList.Contains("book-name"));
                foreach (var el in bookNames)
                {
                    foreach (var span in el.GetElementsByTagName("span"))
                    {
                        span.Remove();
                    }

                    if (el.HasAttribute("data-target"))
                    {
                        string dataName = el.GetAttribute("data-target").Substring(1, el.GetAttribute("data-target").Length - 6);
                        string bookName = el.TextContent.Trim();

                        if (threeMaccVariants.Contains(dataName))
                        {
                            dataName = "3ma";
                        }
                        else if (fourMaccVariants.Contains(dataName))
                        {
                            dataName = "4ma";
                        }
                        else if (greekEstherVariants.Contains(dataName))
                        {
                            dataName = "gkest";
                        }
                        else if (prayerAzariahVariants.Contains(dataName))
                        {
                            dataName = "praz";
                        }
                        else if (dataName == "epjer")
                        {
                            continue;
                        }

                        if (!IsNuisance(bookName))
                        {
                            if (names.ContainsKey(dataName))
                            {
                                if (!names[dataName].Contains(bookName))
                                {
                                    names[dataName].Add(bookName);
                                }
                            }
                            else
                            {
                                names[dataName] = new List<string>{bookName};
                            }
                        }
                    }
                }
            }

            return names;
        }

        /*private async Task<Dictionary<string, string>> GetAPIBibleVersions(string authenticationToken)
        {
            Dictionary<string, string> versions = new Dictionary<string, string>();
            _webClient.DownloadString

        }

        private async Task<Dictionary<string, List<string>>> GetAPIBibleNames(string authenticationToken)
        {

        }*/

        private bool IsNuisance(string word)
        {
            return _nuisances.Contains(word.ToLowerInvariant()) || _nuisances.Contains($"{word.ToLowerInvariant()}.");
        }

        private Dictionary<string, List<string>> MergeDictionaries(List<Dictionary<string, List<string>>> dicts)
        {
            return dicts.SelectMany(dict => dict)
                        .ToLookup(pair => pair.Key, pair => pair.Value)
                        .ToDictionary(group => group.Key,
                                      group => group.SelectMany(list => list).ToList());
        }
    }
}