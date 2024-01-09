/*
* Copyright (C) 2016-2024 Kerygma Digital Co.
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
using AngleSharp.Dom;
using BibleBot.Models;
// using RestSharp;
using Serilog;

namespace BibleBot.Backend.Services
{
    public class NameFetchingService
    {
        // private readonly Dictionary<string, string> _apiBibleNames;
        private readonly Dictionary<string, List<string>> _abbreviations;
        private Dictionary<string, List<string>> _bookNames = new();
        private List<string> _defaultNames;
        private readonly Dictionary<string, Dictionary<string, string>> _bookMap;
        private readonly List<string> _bookMapDataNames;
        private readonly List<string> _nuisances;

        private readonly HttpClient _httpClient;
        // private readonly RestClient _restClient;

        public NameFetchingService()
        {
            // string apibibleNamesText = File.ReadAllText("./Data/NameFetching/apibible_names.json");
            // _apiBibleNames = JsonSerializer.Deserialize<Dictionary<string, string>>(apibibleNamesText);

            string abbreviationsText = File.ReadAllText("./Data/NameFetching/abbreviations.json");
            _abbreviations = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(abbreviationsText);

            string defaultNamesText = File.ReadAllText("./Data/NameFetching/default_names.json");
            _defaultNames = JsonSerializer.Deserialize<List<string>>(defaultNamesText);

            string nuisancesText = File.ReadAllText("./Data/NameFetching/nuisances.json");
            _nuisances = JsonSerializer.Deserialize<List<string>>(nuisancesText);

            string bookMapText = File.ReadAllText("./Data/book_map.json");
            _bookMap = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(bookMapText);

            _bookMapDataNames = _bookMap.Select(b => b.Value).SelectMany(b => b.Keys).ToList();

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.246");
            // _restClient = new RestClient("https://api.scripture.api.bible/v1");
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
            Dictionary<string, string> bgVersions = await GetBibleGatewayVersions();

            Log.Information("NameFetchingService: Getting BibleGateway book names...");
            Dictionary<string, List<string>> bgNames = await GetBibleGatewayNames(bgVersions);

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

            Dictionary<string, List<string>> completedNames = MergeDictionaries(new List<Dictionary<string, List<string>>> { bgNames, /*abNames,*/ _abbreviations });

            Log.Information("NameFetchingService: Serializing and writing to file...");
            string serializedNames = JsonSerializer.Serialize(completedNames, new JsonSerializerOptions { PropertyNameCaseInsensitive = false });
            File.WriteAllText("./Data/NameFetching/book_names.json", serializedNames);

            Log.Information("NameFetchingService: Finished.");
        }

        private async Task<Dictionary<string, string>> GetBibleGatewayVersions()
        {
            Dictionary<string, string> versions = new();

            string resp = await _httpClient.GetStringAsync("https://www.biblegateway.com/versions/");
            IDocument document = await BrowsingContext.New().OpenAsync(req => req.Content(resp));

            IEnumerable<IElement> translationElements = document.All.Where(el => el.ClassList.Contains("translation-name"));
            foreach (IElement el in translationElements)
            {
                IHtmlCollection<IElement> targets = el.GetElementsByTagName("a");

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
            Dictionary<string, List<string>> names = new();

            List<string> threeMaccVariants = new() { "3ma", "3macc", "3m" };
            List<string> fourMaccVariants = new() { "4ma", "4macc", "4m" };
            List<string> greekEstherVariants = new() { "gkest", "gkesth", "gkes" };
            List<string> addEstherVariants = new() { "addesth", "adest" };
            List<string> prayerAzariahVariants = new() { "praz", "prazar" };
            List<string> songThreeYouthsVariants = new() { "sgthr", "sgthree" };

            foreach (KeyValuePair<string, string> version in versions)
            {
                string resp = await _httpClient.GetStringAsync(version.Value);
                IDocument document = await BrowsingContext.New().OpenAsync(req => req.Content(resp));

                IEnumerable<IElement> bookNames = document.All.Where(el => el.ClassList.Contains("book-name"));
                foreach (IElement el in bookNames)
                {
                    foreach (IElement span in el.GetElementsByTagName("span"))
                    {
                        span.Remove();
                    }

                    if (el.HasAttribute("data-target"))
                    {
                        string dataName = el.GetAttribute("data-target").Substring(1, el.GetAttribute("data-target").Length - 6);
                        string bookName = el.TextContent.Trim();

                        bool usesVariant = false;
                        string origDataName = "";

                        if (threeMaccVariants.Contains(dataName))
                        {
                            usesVariant = true;
                            origDataName = dataName;
                            dataName = "3ma";
                        }
                        else if (fourMaccVariants.Contains(dataName))
                        {
                            usesVariant = true;
                            origDataName = dataName;
                            dataName = "4ma";
                        }
                        else if (greekEstherVariants.Contains(dataName))
                        {
                            usesVariant = true;
                            origDataName = dataName;
                            dataName = "gkest";
                        }
                        else if (addEstherVariants.Contains(dataName))
                        {
                            usesVariant = true;
                            origDataName = dataName;
                            dataName = "addesth";
                        }
                        else if (prayerAzariahVariants.Contains(dataName))
                        {
                            usesVariant = true;
                            origDataName = dataName;
                            dataName = "praz";
                        }
                        else if (songThreeYouthsVariants.Contains(dataName))
                        {
                            usesVariant = true;
                            origDataName = dataName;
                            dataName = "sgthr";
                        }
                        else if (dataName == "epjer")
                        {
                            continue;
                        }

                        if (usesVariant && dataName != origDataName)
                        {
                            Log.Warning($"NameFetchingService: \"{version.Key}\" uses variant data name \"{origDataName}\", replaced with \"{dataName}\".");
                        }
                        else if (usesVariant)
                        {
                            Log.Warning($"NameFetchingService: \"{version.Key}\" uses data name \"{dataName}\".");
                        }

                        if (!_bookMapDataNames.Contains(dataName))
                        {
                            Log.Warning($"NameFetchingService: Data name \"{dataName}\" for \"{version.Key}\" not in book_map.json.");
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

        public async Task<Dictionary<BookCategories, Dictionary<string, string>>> GetBibleGatewayVersionBookList(Version version)
        {
            // TODO: We need to find a cleaner solution for these booknames that isn't nested Dictionaries.
            Dictionary<BookCategories, Dictionary<string, string>> names = new();

            List<string> threeMaccVariants = new() { "3macc", "3m" };
            List<string> fourMaccVariants = new() { "4macc", "4m" };
            List<string> greekEstherVariants = new() { "gkesth", "adest", "addesth", "gkes" };
            List<string> prayerAzariahVariants = new() { "sgthree", "sgthr", "prazar" };

            string versionListResp = await _httpClient.GetStringAsync("https://www.biblegateway.com/versions/");
            IDocument versionListDocument = await BrowsingContext.New().OpenAsync(req => req.Content(versionListResp));

            IEnumerable<IElement> translationElements = versionListDocument.All.Where(el => el.ClassList.Contains("translation-name"));

            string url = null;
            foreach (IElement el in translationElements)
            {
                IHtmlCollection<IElement> targets = el.GetElementsByTagName("a");

                if (targets.Length == 1)
                {
                    if (targets[0].HasAttribute("href") && targets[0].TextContent == version.Name)
                    {
                        url = $"https://www.biblegateway.com{targets[0].GetAttribute("href")}";
                    }
                }
            }

            if (url == null)
            {
                return null;
            }

            string bookListResp = await _httpClient.GetStringAsync(url);
            IDocument bookListDocument = await BrowsingContext.New().OpenAsync(req => req.Content(bookListResp));

            IEnumerable<IElement> bookNames = bookListDocument.All.Where(el => el.ClassList.Contains("book-name"));
            foreach (IElement el in bookNames)
            {
                foreach (IElement span in el.GetElementsByTagName("span"))
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
                        BookCategories category;

                        if (_bookMap["ot"].ContainsKey(dataName))
                        {
                            category = BookCategories.OldTestament;
                        }
                        else if (_bookMap["nt"].ContainsKey(dataName))
                        {
                            category = BookCategories.NewTestament;
                        }
                        else if (_bookMap["deu"].ContainsKey(dataName))
                        {
                            category = BookCategories.Deuterocanon;
                        }
                        else
                        {
                            Log.Warning($"NameFetchingService: Data name \"{dataName}\" for \"{version.Name}\" not in book_map.json.");
                            continue;
                        }

                        if (dataName == "ps151")
                        {
                            names[BookCategories.OldTestament]["ps"] = $"{names[BookCategories.OldTestament]["ps"]} <151>";
                        }

                        if (!names.ContainsKey(category))
                        {
                            names.Add(category, new Dictionary<string, string>());
                        }

                        names[category].Add(dataName, bookName);
                    }
                }
            }

            return names;
        }

        // private async Task<Dictionary<string, string>> GetAPIBibleVersions()
        // {
        //     Dictionary<string, string> versions = new();
        //
        //     RestRequest req = new("bibles");
        //     req.AddHeader("api-key", System.Environment.GetEnvironmentVariable("APIBIBLE_TOKEN"));
        //
        //     ABBibleResponse resp = await _restClient.GetAsync<ABBibleResponse>(req);
        //
        //     foreach (ABBibleData version in resp.Data)
        //     {
        //         versions.Add(version.Name, $"bibles/{version.Id}/books");
        //     }
        //
        //     return versions;
        // }

        // private async Task<Dictionary<string, List<string>>> GetAPIBibleNames(Dictionary<string, string> versions)
        // {
        //     Dictionary<string, List<string>> names = new();
        //
        //     List<string> latterKings = new() { "3 Kings", "4 Kings" };
        //
        //     foreach (KeyValuePair<string, string> version in versions)
        //     {
        //         RestRequest req = new(version.Value);
        //         req.AddHeader("api-key", System.Environment.GetEnvironmentVariable("APIBIBLE_TOKEN"));
        //
        //         List<ABBookData> resp = await _restClient.GetAsync<List<ABBookData>>(req);
        //
        //         foreach (ABBookData book in resp)
        //         {
        //             if (book.Name == null)
        //             {
        //                 continue;
        //             }
        //
        //             book.Name = book.Name.Trim();
        //
        //             if (!_apiBibleNames.ContainsKey(book.Id))
        //             {
        //                 continue;
        //             }
        //
        //             string internalId = _apiBibleNames[book.Id];
        //
        //             if ((internalId == "1sam" && book.Name == "1 Kings") || (internalId == "2sam" && book.Name == "2 Kings") || latterKings.Contains(book.Abbreviation))
        //             {
        //                 continue;
        //             }
        //
        //             if (names.ContainsKey(internalId))
        //             {
        //                 if (!names[internalId].Contains(book.Name))
        //                 {
        //                     names[internalId].Add(book.Name);
        //                 }
        //             }
        //             else
        //             {
        //                 names.Add(internalId, new List<string> { book.Name });
        //             }
        //         }
        //     }
        //
        //     return names;
        // }

        private bool IsNuisance(string word) => _nuisances.Contains(word.ToLowerInvariant()) || _nuisances.Contains($"{word.ToLowerInvariant()}.");

        private Dictionary<string, List<string>> MergeDictionaries(List<Dictionary<string, List<string>>> dicts) => dicts.SelectMany(dict => dict)
                                                                                                                         .ToLookup(pair => pair.Key, pair => pair.Value)
                                                                                                                         .ToDictionary(group => group.Key,
                                                                                                                         group => group.SelectMany(list => list).ToList());
    }
}
