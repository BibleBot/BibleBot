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
using RestSharp;
using Serilog;

namespace BibleBot.Backend.Services
{
    public class MetadataFetchingService
    {
        private MDABBookMap _apiBibleNames;
        private readonly MDBookNames _abbreviations;
        private MDBookNames _bookNames = [];
        private List<string> _defaultNames;
        private readonly MDBookMap _bookMap;
        private readonly List<string> _bookMapDataNames;
        private readonly List<string> _nuisances;
        private static readonly JsonSerializerOptions _serializerOptions = new() { PropertyNameCaseInsensitive = false };
        private readonly string _filePrefix = ".";

        private readonly HttpClient _httpClient;
        private readonly RestClient _restClient;
        private readonly MongoService _mongoService;

        public MetadataFetchingService(MongoService mongoService, bool isForAutoServ)
        {
            if (isForAutoServ)
            {
                _filePrefix = "../BibleBot.Backend";
            }

            string apibibleNamesText = File.ReadAllText($"{_filePrefix}/Data/NameFetching/apibible_names.json");
            _apiBibleNames = JsonSerializer.Deserialize<MDABBookMap>(apibibleNamesText);

            string abbreviationsText = File.ReadAllText($"{_filePrefix}/Data/NameFetching/abbreviations.json");
            _abbreviations = JsonSerializer.Deserialize<MDBookNames>(abbreviationsText);

            string defaultNamesText = File.ReadAllText($"{_filePrefix}/Data/NameFetching/default_names.json");
            _defaultNames = JsonSerializer.Deserialize<List<string>>(defaultNamesText);

            string nuisancesText = File.ReadAllText($"{_filePrefix}/Data/NameFetching/nuisances.json");
            _nuisances = JsonSerializer.Deserialize<List<string>>(nuisancesText);

            string bookMapText = File.ReadAllText($"{_filePrefix}/Data/book_map.json");
            _bookMap = JsonSerializer.Deserialize<MDBookMap>(bookMapText);

            _bookMapDataNames = [.. _bookMap.Select(b => b.Value).SelectMany(b => b.Keys)];

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.246");
            _restClient = new RestClient("https://api.scripture.api.bible/v1");

            _mongoService = mongoService;
        }

        public MDBookNames GetBookNames()
        {
            if (_bookNames.Count == 0)
            {
                string bookNamesText = File.ReadAllText($"{_filePrefix}/Data/NameFetching/book_names.json");
                _bookNames = JsonSerializer.Deserialize<MDBookNames>(bookNamesText);
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

        public MDABBookMap GetAPIBibleMapping()
        {
            if (_apiBibleNames.Count == 0)
            {
                string apiBibleNamesText = File.ReadAllText($"{_filePrefix}/Data/NameFetching/apibible_names.json");
                _apiBibleNames = JsonSerializer.Deserialize<MDABBookMap>(apiBibleNamesText);
            }

            return _apiBibleNames;
        }

        public async Task FetchMetadata(bool isDryRun)
        {
            if (isDryRun)
            {
                Log.Information("MetadataFetchingService: Dry run enabled, we will not fetch metadata for this session.");

                if (!File.Exists($"{_filePrefix}/Data/NameFetching/book_names.json"))
                {
                    Log.Warning("MetadataFetchingService: Book names file does NOT exist, some references may not process.");
                }
                return;
            }

            Log.Information("MetadataFetchingService: Getting BibleGateway versions...");
            Dictionary<string, string> bgVersions = await GetBibleGatewayVersions();

            Log.Information("MetadataFetchingService: Getting BibleGateway book names...");
            MDBookNames bgNames = await GetBibleGatewayNames(bgVersions);

            Log.Information("MetadataFetchingService: Getting API.Bible versions without book data...");
            List<Version> abVersions = [.. (await _mongoService.Get<Version>()).Where(version => version.Source == "ab" && version.Books == null)];

            if (abVersions.Count > 0)
            {
                Log.Information("MetadataFetchingService: Getting API.Bible version metadata...");
                MDABVersionData abVersionData = await GetAPIBibleVersionData(abVersions);

                Log.Information("MetadataFetchingService: Saving API.Bible version metadata into database...");
                await SaveAPIBibleMetadata(abVersionData);
            }

            Log.Information("MetadataFetchingService: Getting book names from API.Bible versions...");
            MDBookNames abNames = await GetAllAPIBibleNames();

            if (File.Exists($"{_filePrefix}/Data/NameFetching/book_names.json"))
            {
                Log.Information("MetadataFetchingService: Found old names file, removing...");
                File.Delete($"{_filePrefix}/Data/NameFetching/book_names.json");
            }

            MDBookNames completedNames = MergeBookNames([bgNames, abNames, _abbreviations]);

            Log.Information("MetadataFetchingService: Serializing and writing book names to file...");
            string serializedNames = JsonSerializer.Serialize(completedNames, _serializerOptions);
            File.WriteAllText($"{_filePrefix}/Data/NameFetching/book_names.json", serializedNames);

            Log.Information("MetadataFetchingService: Finished.");
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

        private async Task<MDBookNames> GetBibleGatewayNames(Dictionary<string, string> versions)
        {
            MDBookNames names = [];

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
                            Log.Warning($"MetadataFetchingService: \"{version.Key}\" uses variant data name \"{origDataName}\", replaced with \"{dataName}\".");
                        }
                        else if (usesVariant)
                        {
                            Log.Warning($"MetadataFetchingService: \"{version.Key}\" uses data name \"{dataName}\".");
                        }

                        if (!_bookMapDataNames.Contains(dataName))
                        {
                            Log.Warning($"MetadataFetchingService: Data name \"{dataName}\" for \"{version.Key}\" not in book_map.json.");
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

        public async Task<MDVersionBookList> GetBibleGatewayVersionBookList(Version version)
        {
            MDVersionBookList names = [];

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
                            Log.Warning($"MetadataFetchingService: Data name \"{dataName}\" for \"{version.Name}\" not in book_map.json.");
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

        private async Task<MDABVersionData> GetAPIBibleVersionData(List<Version> versions)
        {
            MDABVersionData versionData = [];

            foreach (Version version in versions)
            {
                RestRequest req = new($"bibles/{version.ApiBibleId}/books?include-chapters=true");
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
                            Log.Warning("MetadataFetchingService: Received unauthorized from API.Bible, WE MIGHT BE RATE LIMITED");
                        }
                        return [];
                    }
                }

                if (resp != null)
                {
                    versionData.Add(version, resp);
                }
            }

            return versionData;
        }

        private async Task<MDBookNames> GetAllAPIBibleNames()
        {
            MDBookNames names = [];

            List<Version> abVersions = [.. (await _mongoService.Get<Version>()).Where(version => version.Source == "ab")];

            foreach (Version version in abVersions)
            {
                Book[] bookData = version.Books;

                foreach (Book book in bookData)
                {
                    if (names.ContainsKey(book.Name))
                    {
                        if (!names[book.Name].Contains(book.PreferredName))
                        {
                            names[book.Name].Add(book.PreferredName);
                        }
                    }
                    else
                    {
                        names.Add(book.Name, [book.PreferredName]);
                    }
                }
            }

            return names;
        }

        private async Task SaveAPIBibleMetadata(MDABVersionData versionData)
        {
            List<string> latterKings = ["3 Kings", "4 Kings"];
            List<string> workaroundIds = ["DAG", "PS2"];

            foreach (KeyValuePair<Version, ABBooksResponse> kvp in versionData)
            {
                Version version = kvp.Key;
                ABBooksResponse resp = kvp.Value;

                List<Book> versionBookData = [];

                foreach (ABBookData book in resp.Data)
                {
                    if (!_apiBibleNames.ContainsKey(book.Id) && workaroundIds.Contains(book.Id))
                    {
                        Log.Warning($"MetadataFetchingService: Id \"{book.Id}\" for '{book.Name}' in {version.Name} ({version.ApiBibleId}) does not exist in apibible_names.json, skipping...");
                        continue;
                    }

                    string properName;
                    string internalId = _apiBibleNames[book.Id];

                    if (_bookMap["ot"].ContainsKey(internalId))
                    {
                        properName = _bookMap["ot"][internalId];
                    }
                    else if (_bookMap["nt"].ContainsKey(internalId))
                    {
                        properName = _bookMap["nt"][internalId];
                    }
                    else if (_bookMap["deu"].ContainsKey(internalId))
                    {
                        properName = _bookMap["deu"][internalId];
                    }
                    else
                    {
                        if ((version.Abbreviation is "ELXX" or "LXX") && internalId == "DAG")
                        {
                            properName = _bookMap["deu"]["DAN"];
                        }
                        else
                        {
                            Log.Warning($"MetadataFetchingService: API.Bible translation \"{book.Id}\" for \"{version.Name}\" not in apibible_names.json.");
                            continue;
                        }
                    }

                    book.Name = IsNuisance(book.Name) ? properName : book.Name.Trim();

                    if ((internalId == "1sam" && book.Name == "1 Kings") || (internalId == "2sam" && book.Name == "2 Kings") || latterKings.Contains(book.Abbreviation))
                    {
                        // TODO(srp): So, the first two conditions ultimately avoid parsing
                        // a default name, but I don't know why the third one exists or
                        // what it achieves.
                        continue;
                    }

                    List<Chapter> chapters = [];

                    foreach (ABChapter chapter in book.Chapters)
                    {
                        if (int.TryParse(chapter.Number, out int parsedNumber))
                        {
                            chapters.Add(new()
                            {
                                Number = parsedNumber
                            });
                        }
                        else
                        {
                            Log.Warning($"MetadataFetchingService: Ignoring chapter '{chapter.Id}' in '{version.Name}' with non-numeric chapter value.");
                        }
                    }

                    versionBookData.Add(new()
                    {
                        Name = internalId,
                        ProperName = properName,
                        InternalName = book.Id,
                        PreferredName = book.Name
                    });
                }

                UpdateDefinition<Version> update = Builders<Version>.Update.Set(version => version.Books, [.. versionBookData]);
                await _mongoService.Update(version.Abbreviation, update);
            }
        }

        public async Task<MDVersionBookList> GetAPIBibleVersionBookList(Version version)
        {
            MDVersionBookList names = [];

            foreach (Book book in version.Books)
            {
                // We use these for renaming the ELXX books.
                bool isOT = false;
                bool isDEU = false;

                BookCategories category;

                if (_bookMap["ot"].ContainsKey(book.Name))
                {
                    isOT = true;
                    category = BookCategories.OldTestament;
                }
                else if (_bookMap["nt"].ContainsKey(book.Name))
                {
                    category = BookCategories.NewTestament;
                }
                else if (_bookMap["deu"].ContainsKey(book.Name))
                {
                    isDEU = true;
                    category = BookCategories.Deuterocanon;
                }
                else
                {
                    Log.Information($"MetadataFetchingService: Book '{book.Name}' in '{version.Name}' does not match any known categories, this should never happen...");
                    continue;
                }

                if (version.Abbreviation is "ELXX" or "LXX")
                {
                    if (isOT)
                    {
                        book.PreferredName = _bookMap["ot"][book.Name];

                        if (book.PreferredName == "Ezra")
                        {
                            book.PreferredName += "/Nehemiah";
                        }
                        else if (book.PreferredName == "Psalm")
                        {
                            book.PreferredName += "s";
                        }
                    }
                    else if (isDEU)
                    {
                        book.PreferredName = _bookMap["deu"][book.Name];
                    }
                }

                if (book.Name == "ps")
                {
                    RestRequest chaptersReq = new($"bibles/{version.ApiBibleId}/books/{book.InternalName}/chapters");
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
                                names[BookCategories.OldTestament].Add(book.Name, $"{book.PreferredName} <151>");
                            }
                        }
                    }
                }

                if (!names.ContainsKey(category))
                {
                    names.Add(category, []);
                }

                names[category].TryAdd(book.Name, book.PreferredName);

                if (book.Name == "ezek" && version.Abbreviation == "ELXX")
                {
                    names[category].TryAdd("dan", "Daniel");
                }
            }

            return names;
        }

        private bool IsNuisance(string word) => _nuisances.Contains(word.ToLowerInvariant()) || _nuisances.Contains($"{word.ToLowerInvariant()}.");

        private static MDBookNames MergeBookNames(List<MDBookNames> bookNames) => bookNames.SelectMany(dict => dict).ToLookup(pair => pair.Key, pair => pair.Value).ToDictionary(group => group.Key, group => group.SelectMany(list => list).ToList()) as MDBookNames;
    }
}
