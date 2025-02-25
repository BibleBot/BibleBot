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
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using BibleBot.Models;
using MongoDB.Driver;
using MongoDB.Driver.Search;
using RestSharp;
using Serilog;

namespace BibleBot.Backend.Services
{
    public class NameFetchingService
    {
        private Dictionary<string, string> _apiBibleNames;
        private readonly Dictionary<string, List<string>> _abbreviations;
        private Dictionary<string, List<string>> _bookNames = [];
        private List<string> _defaultNames;
        private readonly Dictionary<string, Dictionary<string, string>> _bookMap;
        private readonly List<string> _bookMapDataNames;
        private readonly List<string> _nuisances;
        private Dictionary<string, string> _nltNamesText;

        private static readonly JsonSerializerOptions _serializerOptions = new() { PropertyNameCaseInsensitive = false };
        private readonly string _filePrefix = ".";

        private readonly HttpClient _httpClient;
        private readonly RestClient _restClient;
        private readonly MongoService _mongoService;

        public NameFetchingService(MongoService mongoService, bool isForAutoServ)
        {
            if (isForAutoServ)
            {
                _filePrefix = "../BibleBot.Backend";
            }

            string apibibleNamesText = File.ReadAllText($"{_filePrefix}/Data/NameFetching/apibible_names.json");
            _apiBibleNames = JsonSerializer.Deserialize<Dictionary<string, string>>(apibibleNamesText);

            string abbreviationsText = File.ReadAllText($"{_filePrefix}/Data/NameFetching/abbreviations.json");
            _abbreviations = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(abbreviationsText);

            string defaultNamesText = File.ReadAllText($"{_filePrefix}/Data/NameFetching/default_names.json");
            _defaultNames = JsonSerializer.Deserialize<List<string>>(defaultNamesText);

            string nuisancesText = File.ReadAllText($"{_filePrefix}/Data/NameFetching/nuisances.json");
            _nuisances = JsonSerializer.Deserialize<List<string>>(nuisancesText);

            string nltNamesText = File.ReadAllText($"{_filePrefix}/Data/NameFetching/nlt_names.json");
            _nltNamesText = JsonSerializer.Deserialize<Dictionary<string, string>>(nltNamesText);

            string bookMapText = File.ReadAllText($"{_filePrefix}/Data/book_map.json");
            _bookMap = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(bookMapText);

            _bookMapDataNames = [.. _bookMap.Select(b => b.Value).SelectMany(b => b.Keys)];

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.246");
            _restClient = new RestClient("https://api.scripture.api.bible/v1");

            _mongoService = mongoService;
        }

        public Dictionary<string, List<string>> GetBookNames()
        {
            if (_bookNames.Count == 0)
            {
                string bookNamesText = File.ReadAllText($"{_filePrefix}/Data/NameFetching/book_names.json");
                _bookNames = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(bookNamesText);
            }

            return _bookNames;
        }

        public List<string> GetDefaultBookNames()
        {
            if (_defaultNames.Count == 0)
            {
                string defaultNamesText = File.ReadAllText($"{_filePrefix}/Data/NameFetching/default_names.json");
                _defaultNames = JsonSerializer.Deserialize<List<string>>(defaultNamesText);
            }

            return _defaultNames;
        }

        public Dictionary<string, string> GetAPIBibleMapping()
        {
            if (_nltNamesText.Count == 0)
            {
                string nltNamesText = File.ReadAllText($"{_filePrefix}/Data/NameFetching/nlt_names.json");
                _nltNamesText = JsonSerializer.Deserialize<Dictionary<string, string>>(nltNamesText);
            }

            return _nltNamesText;
        }

        public Dictionary<string, string> GetNLTMapping()
        {
            if (_apiBibleNames.Count == 0)
            {
                string apiBibleNamesText = File.ReadAllText($"{_filePrefix}/Data/NameFetching/apibible_names.json");
                _apiBibleNames = JsonSerializer.Deserialize<Dictionary<string, string>>(apiBibleNamesText);
            }

            return _apiBibleNames;
        }

        public async Task FetchBookNames(bool isDryRun)
        {
            if (isDryRun)
            {
                Log.Information("NameFetchingService: Dry run enabled, we will not fetch book names for this session.");

                if (!File.Exists($"{_filePrefix}/Data/NameFetching/book_names.json"))
                {
                    Log.Warning("NameFetchingService: Book names file does NOT exist, some references may not process");
                }
                return;
            }

            Log.Information("NameFetchingService: Getting BibleGateway versions...");
            Dictionary<string, string> bgVersions = await GetBibleGatewayVersions();

            Log.Information("NameFetchingService: Getting BibleGateway book names...");
            Dictionary<string, List<string>> bgNames = await GetBibleGatewayNames(bgVersions);

            Log.Information("NameFetchingService: Getting API.Bible versions...");
            SearchDefinition<Version> abVersionQuery = Builders<Version>.Search.Equals(version => version.Source, "ab");
            List<Version> abVersions = await _mongoService.Search(abVersionQuery);

            Log.Information("NameFetchingService: Getting API.Bible book names...");
            Dictionary<string, List<string>> abNames = await GetAPIBibleNames(abVersions);

            if (File.Exists($"{_filePrefix}/Data/NameFetching/book_names.json"))
            {
                File.Delete($"{_filePrefix}/Data/NameFetching/book_names.json");
                Log.Information("NameFetchingService: Removed old names file...");
            }

            Dictionary<string, List<string>> completedNames = MergeDictionaries([bgNames, abNames, _abbreviations]);

            Log.Information("NameFetchingService: Serializing and writing to file...");
            string serializedNames = JsonSerializer.Serialize(completedNames, _serializerOptions);
            File.WriteAllText($"{_filePrefix}/Data/NameFetching/book_names.json", serializedNames);

            Log.Information("NameFetchingService: Finished.");
        }

        private async Task<Dictionary<string, string>> GetBibleGatewayVersions()
        {
            Dictionary<string, string> versions = [];

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
            Dictionary<string, List<string>> names = [];

            List<string> threeMaccVariants = ["3ma", "3macc", "3m"];
            List<string> fourMaccVariants = ["4ma", "4macc", "4m"];
            List<string> greekEstherVariants = ["gkest", "gkesth", "gkes"];
            List<string> addEstherVariants = ["addesth", "adest"];
            List<string> prayerAzariahVariants = ["praz", "prazar"];
            List<string> songThreeYouthsVariants = ["sgthr", "sgthree"];

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
                                names[dataName] = [bookName];
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
            Dictionary<BookCategories, Dictionary<string, string>> names = [];

            List<string> threeMaccVariants = ["3ma", "3macc", "3m"];
            List<string> fourMaccVariants = ["4ma", "4macc", "4m"];
            List<string> greekEstherVariants = ["gkest", "gkesth", "gkes"];
            List<string> addEstherVariants = ["addesth", "adest"];
            List<string> prayerAzariahVariants = ["praz", "prazar"];
            List<string> songThreeYouthsVariants = ["sgthr", "sgthree"];

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
                    else if (addEstherVariants.Contains(dataName))
                    {
                        dataName = "addesth";
                    }
                    else if (prayerAzariahVariants.Contains(dataName))
                    {
                        dataName = "praz";
                    }
                    else if (songThreeYouthsVariants.Contains(dataName))
                    {
                        dataName = "sgthr";
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
                            names.Add(category, []);
                        }

                        names[category].Add(dataName, bookName);
                    }
                }
            }

            return names;
        }

        private async Task<Dictionary<string, List<string>>> GetAPIBibleNames(List<Version> versions)
        {
            Dictionary<string, List<string>> names = [];

            List<string> latterKings = ["3 Kings", "4 Kings"];
            List<string> workaroundIds = ["DAG", "PS2"];

            foreach (Version version in versions)
            {
                RestRequest req = new($"bibles/{version.ApiBibleId}/books");
                req.AddHeader("api-key", System.Environment.GetEnvironmentVariable("APIBIBLE_TOKEN"));

                ABBooksResponse resp = null;

                try
                {
                    resp = await _restClient.GetAsync<ABBooksResponse>(req);
                }
                catch (HttpRequestException ex)
                {
                    if (ex.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            Log.Warning("NameFetchingService: Received unauthorized from API.Bible, WE MIGHT BE RATE LIMITED");
                        }
                        return [];
                    }
                }

                if (resp != null)
                {
                    foreach (ABBookData book in resp.Data)
                    {
                        if (book.Name == null || IsNuisance(book.Name))
                        {
                            continue;
                        }

                        book.Name = book.Name.Trim();

                        if (!_apiBibleNames.ContainsKey(book.Id) && workaroundIds.Contains(book.Id))
                        {
                            Log.Warning($"NameFetchingService: Id \"{book.Id}\" for '{book.Name}' in {version.Name} ({version.ApiBibleId}) does not exist in apibible_names.json.");
                            continue;
                        }

                        string internalId = _apiBibleNames[book.Id];

                        if ((internalId == "1sam" && book.Name == "1 Kings") || (internalId == "2sam" && book.Name == "2 Kings") || latterKings.Contains(book.Abbreviation))
                        {
                            // TODO(srp): So, the first two conditions ultimately avoid parsing
                            // a default name, but I don't know why the third one exists or
                            // what it achieves.
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
                            names.Add(internalId, [book.Name]);
                        }
                    }
                }
            }

            return names;
        }

        public async Task<Dictionary<BookCategories, Dictionary<string, string>>> GetAPIBibleVersionBookList(Version version)
        {
            // TODO: We need to find a cleaner solution for these booknames that isn't nested Dictionaries.
            Dictionary<BookCategories, Dictionary<string, string>> names = [];

            RestRequest req = new($"bibles/{version.ApiBibleId}/books");
            req.AddHeader("api-key", System.Environment.GetEnvironmentVariable("APIBIBLE_TOKEN"));

            ABBooksResponse resp = null;

            try
            {
                resp = await _restClient.GetAsync<ABBooksResponse>(req);
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == HttpStatusCode.Unauthorized)
                {
                    Log.Warning("NameFetchingService: Received Unauthorized from API.Bible, skipping...");
                    return [];
                }
            }

            if (resp != null)
            {
                foreach (ABBookData book in resp.Data)
                {
                    // We use these for renaming the ELXX books.
                    bool isOT = false;
                    bool isDEU = false;

                    if (book.Name == null || IsNuisance(book.Name))
                    {
                        continue;
                    }

                    book.Name = book.Name.Trim();

                    if (!_apiBibleNames.ContainsKey(book.Id))
                    {
                        continue;
                    }

                    string internalId = _apiBibleNames[book.Id];

                    BookCategories category;

                    if (_bookMap["ot"].ContainsKey(internalId))
                    {
                        isOT = true;
                        category = BookCategories.OldTestament;
                    }
                    else if (_bookMap["nt"].ContainsKey(internalId))
                    {
                        category = BookCategories.NewTestament;
                    }
                    else if (_bookMap["deu"].ContainsKey(internalId))
                    {
                        isDEU = true;
                        category = BookCategories.Deuterocanon;
                    }
                    else
                    {
                        if ((version.Abbreviation is "ELXX" or "LXX") && internalId == "DAG")
                        {
                            isOT = true;
                            category = BookCategories.OldTestament;
                            internalId = "DAN";
                        }

                        Log.Warning($"NameFetchingService: API.Bible translation \"{book.Id}\" for \"{version.Name}\" not in apibible_names.json.");
                        continue;
                    }

                    if (version.Abbreviation is "ELXX" or "LXX")
                    {
                        if (isOT)
                        {
                            book.Name = _bookMap["ot"][internalId];

                            if (book.Name == "Ezra")
                            {
                                book.Name += "/Nehemiah";
                            }
                            else if (book.Name == "Psalm")
                            {
                                book.Name += "s";
                            }
                        }
                        else if (isDEU)
                        {
                            book.Name = _bookMap["deu"][internalId];
                        }
                    }

                    if (internalId == "ps")
                    {
                        RestRequest chaptersReq = new($"bibles/{version.ApiBibleId}/books/{book.Id}/chapters");
                        chaptersReq.AddHeader("api-key", System.Environment.GetEnvironmentVariable("APIBIBLE_TOKEN"));

                        ABChaptersResponse chaptersResp = await _restClient.GetAsync<ABChaptersResponse>(chaptersReq);

                        foreach (ABChapter chapter in chaptersResp.Data)
                        {
                            if (chapter.Number == "151")
                            {
                                try
                                {
                                    names[BookCategories.OldTestament]["ps"] = $"{names[BookCategories.OldTestament]["ps"]} <151>";
                                }
                                catch (KeyNotFoundException)
                                {
                                    names[BookCategories.OldTestament].Add(internalId, $"{book.Name} <151>");
                                }
                            }
                        }
                    }

                    if (!names.ContainsKey(category))
                    {
                        names.Add(category, []);
                    }

                    names[category].TryAdd(internalId, book.Name);

                    if (internalId == "ezek" && version.Abbreviation == "ELXX")
                    {
                        names[category].TryAdd("dan", "Daniel");
                    }
                }
            }

            return names;
        }

        private bool IsNuisance(string word) => _nuisances.Contains(word.ToLowerInvariant()) || _nuisances.Contains($"{word.ToLowerInvariant()}.");

        private static Dictionary<string, List<string>> MergeDictionaries(List<Dictionary<string, List<string>>> dicts) => dicts.SelectMany(dict => dict)
                                                                                                                         .ToLookup(pair => pair.Key, pair => pair.Value)
                                                                                                                         .ToDictionary(group => group.Key,
                                                                                                                         group => group.SelectMany(list => list).ToList());
    }
}
