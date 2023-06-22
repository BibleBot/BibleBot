/*
* Copyright (C) 2016-2023 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AngleSharp;
using BibleBot.Models;
using RestSharp;
using Serilog;

namespace BibleBot.Backend.Services
{
    public class NameFetchingService
    {
        private readonly Dictionary<string, string> _apiBibleNames;
        private readonly Dictionary<string, List<string>> _abbreviations;
        private Dictionary<string, List<string>> _bookNames = new Dictionary<string, List<string>>();
        private List<string> _defaultNames;
        private readonly List<string> _nuisances;

        private readonly HttpClient _httpClient;
        private readonly RestClient _restClient;

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

            _httpClient = new HttpClient();
            _restClient = new RestClient("https://api.scripture.api.bible/v1");
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
                Log.Information("NameFetchingService: Dry run enabled, we will not fetch book names for this session.");

                if (!File.Exists("./Data/NameFetching/book_names.json"))
                {
                    Log.Warning("NameFetchingService: Book names file does NOT exist, some references may not process");
                }
                return;
            }

            Log.Information("NameFetchingService: Getting BibleGateway versions...");
            var bgVersions = await GetBibleGatewayVersions();

            Log.Information("NameFetchingService: Getting BibleGateway book names...");
            var bgNames = await GetBibleGatewayNames(bgVersions);

            // todo: actually get API.Bible Names
            // Log.Information("NameFetchingService: Getting API.Bible versions...");
            // var abVersions = await GetBibleGatewayVersions();

            // Log.Information("NameFetchingService: Getting API.Bible book names...");
            // var abNames = await GetBibleGatewayNames(abVersions);

            if (File.Exists("./Data/NameFetching/book_names.json"))
            {
                File.Delete("./Data/NameFetching/book_names.json");
                Log.Information("NameFetchingService: Removed old names file...");
            }

            var completedNames = MergeDictionaries(new List<Dictionary<string, List<string>>> { bgNames, /*abNames,*/ _abbreviations });

            Log.Information("NameFetchingService: Serializing and writing to file...");
            string serializedNames = JsonSerializer.Serialize(completedNames, new JsonSerializerOptions { PropertyNameCaseInsensitive = false });
            File.WriteAllText("./Data/NameFetching/book_names.json", serializedNames);

            Log.Information("NameFetchingService: Finished.");
        }

        private async Task<Dictionary<string, string>> GetBibleGatewayVersions()
        {
            Dictionary<string, string> versions = new Dictionary<string, string>();

            string resp = await _httpClient.GetStringAsync("https://www.biblegateway.com/versions/");
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

            List<string> threeMaccVariants = new List<string> { "3macc", "3m" };
            List<string> fourMaccVariants = new List<string> { "4macc", "4m" };
            List<string> greekEstherVariants = new List<string> { "gkesth", "adest", "addesth", "gkes" };
            List<string> prayerAzariahVariants = new List<string> { "sgthree", "sgthr", "prazar" };

            foreach (KeyValuePair<string, string> version in versions)
            {
                string resp = await _httpClient.GetStringAsync(version.Value);
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
                                names[dataName] = new List<string> { bookName };
                            }
                        }
                    }
                }
            }

            return names;
        }

        private async Task<Dictionary<string, string>> GetAPIBibleVersions()
        {
            Dictionary<string, string> versions = new Dictionary<string, string>();

            var req = new RestRequest("bibles");
            req.AddHeader("api-key", System.Environment.GetEnvironmentVariable("APIBIBLE_TOKEN"));

            ABBibleResponse resp = await _restClient.GetAsync<ABBibleResponse>(req);

            foreach (var version in resp.Data)
            {
                versions.Add(version.Name, $"bibles/{version.Id}/books");
            }

            return versions;
        }

        private async Task<Dictionary<string, List<string>>> GetAPIBibleNames(Dictionary<string, string> versions)
        {
            Dictionary<string, List<string>> names = new Dictionary<string, List<string>>();

            List<string> latterKings = new List<string> { "3 Kings", "4 Kings" };

            foreach (var version in versions)
            {
                var req = new RestRequest(version.Value);
                req.AddHeader("api-key", System.Environment.GetEnvironmentVariable("APIBIBLE_TOKEN"));

                List<ABBookData> resp = await _restClient.GetAsync<List<ABBookData>>(req);

                foreach (var book in resp)
                {
                    if (book.Name == null)
                    {
                        continue;
                    }

                    book.Name = book.Name.Trim();

                    if (!_apiBibleNames.ContainsKey(book.Id))
                    {
                        continue;
                    }

                    var internalId = _apiBibleNames[book.Id];

                    if ((internalId == "1sam" && book.Name == "1 Kings") || (internalId == "2sam" && book.Name == "2 Kings") || (latterKings.Contains(book.Abbreviation)))
                    {
                        continue;
                    }

                    if (names.ContainsKey(internalId))
                    {
                        if (!names[internalId].Contains(book.Name))
                        {
                            names[internalId].Add(book.Name);
                        }
                    }
                    else
                    {
                        names.Add(internalId, new List<string> { book.Name });
                    }
                }
            }

            return names;
        }

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
