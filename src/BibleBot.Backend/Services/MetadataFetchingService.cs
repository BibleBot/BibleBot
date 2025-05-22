/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
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
// using MDABVersionData = System.Collections.Generic.List<System.Tuple<BibleBot.Models.Version, BibleBot.Models.ABBooksResponse, System.Collections.Generic.List<BibleBot.Models.ABVersesResponse>>>;
using MDABVersionData = System.Collections.Generic.Dictionary<BibleBot.Models.Version, BibleBot.Models.ABBooksResponse>;
using MDBGVersionData = System.Collections.Generic.Dictionary<BibleBot.Models.Version, System.Net.Http.HttpResponseMessage>;
using MDBookMap = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, string>>;
using MDBookNames = System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>>;
using MDVersionBookList = System.Collections.Generic.Dictionary<BibleBot.Models.BookCategories, System.Collections.Generic.Dictionary<string, string>>;
using Version = BibleBot.Models.Version;

namespace BibleBot.Backend.Services
{
    public class MetadataFetchingService
    {
        private Dictionary<string, string> _bibleGatewayNames;
        private readonly MDBookNames _abbreviations;
        private MDBookNames _bookNames = [];
        private List<string> _defaultNames;
        private readonly MDBookMap _bookMap;
        private readonly List<string> _nuisances;
        private static readonly JsonSerializerOptions _serializerOptions = new() { PropertyNameCaseInsensitive = false };
        private readonly string _filePrefix = ".";

        private readonly HttpClient _httpClient;
        private readonly RestClient _restClient;
        private readonly VersionService _versionService;

        public MetadataFetchingService(VersionService versionService, bool isForAutoServ)
        {
            if (isForAutoServ)
            {
                _filePrefix = "../BibleBot.Backend";
            }

            string bibleGatewayNamesText = File.ReadAllText($"{_filePrefix}/Data/NameFetching/biblegateway_names.json");
            _bibleGatewayNames = JsonSerializer.Deserialize<Dictionary<string, string>>(bibleGatewayNamesText);

            string abbreviationsText = File.ReadAllText($"{_filePrefix}/Data/NameFetching/abbreviations.json");
            _abbreviations = JsonSerializer.Deserialize<MDBookNames>(abbreviationsText);

            string defaultNamesText = File.ReadAllText($"{_filePrefix}/Data/NameFetching/default_names.json");
            _defaultNames = JsonSerializer.Deserialize<List<string>>(defaultNamesText);

            string nuisancesText = File.ReadAllText($"{_filePrefix}/Data/NameFetching/nuisances.json");
            _nuisances = JsonSerializer.Deserialize<List<string>>(nuisancesText);

            string bookMapText = File.ReadAllText($"{_filePrefix}/Data/book_map.json");
            _bookMap = JsonSerializer.Deserialize<MDBookMap>(bookMapText);

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.246");
            _restClient = new RestClient("https://api.scripture.api.bible/v1");

            _versionService = versionService;
        }

        public MDBookNames GetBookNames()
        {
            if (_bookNames.Count != 0)
            {
                return _bookNames;
            }

            string bookNamesText = File.ReadAllText($"{_filePrefix}/Data/NameFetching/book_names.json");
            _bookNames = JsonSerializer.Deserialize<MDBookNames>(bookNamesText);

            return _bookNames;
        }

        public List<string> GetDefaultBookNames()
        {
            if (_defaultNames.Count != 0)
            {
                return _defaultNames;
            }

            string defaultNamesText = File.ReadAllText($"{_filePrefix}/Data/NameFetching/default_names.json");
            _defaultNames = JsonSerializer.Deserialize<List<string>>(defaultNamesText);

            return _defaultNames;
        }

        public Dictionary<string, string> GetBibleGatewayMapping()
        {
            if (_bibleGatewayNames.Count != 0)
            {
                return _bibleGatewayNames;
            }

            string bibleGatewayNamesText = File.ReadAllText($"{_filePrefix}/Data/NameFetching/biblegateway_names.json");
            _bibleGatewayNames = JsonSerializer.Deserialize<Dictionary<string, string>>(bibleGatewayNamesText);

            return _bibleGatewayNames;
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

            Log.Information("MetadataFetchingService: Getting versions from DB...");
            List<Version> versions = await _versionService.Get();

            List<Version> abVersions = versions.Where(version => version.Source == "ab" && version.Books == null).ToList();
            List<Version> bgVersions = versions.Where(version => version.Source == "bg" && version.Books == null).ToList();

            if (abVersions!.Count != 0)
            {
                Log.Information("MetadataFetchingService: Getting API.Bible version metadata...");
                MDABVersionData abVersionData = await GetAPIBibleVersionData(abVersions);

                Log.Information("MetadataFetchingService: Saving API.Bible version metadata into database...");
                await SaveAPIBibleMetadata(abVersionData);
            }

            if (bgVersions!.Count != 0)
            {
                Log.Information("MetadataFetchingService: Getting BibleGateway version metadata...");
                MDBGVersionData bgVersionData = await GetBibleGatewayVersionData(bgVersions);

                Log.Information("MetadataFetchingService: Saving BibleGateway version metadata into database...");
                await SaveBibleGatewayMetadata(bgVersionData);
            }


            Log.Information("MetadataFetchingService: Getting book names from versions in database...");
            MDBookNames names = await GetDBBookNames();

            if (File.Exists($"{_filePrefix}/Data/NameFetching/book_names.json"))
            {
                Log.Information("MetadataFetchingService: Found old names file, removing...");
                File.Delete($"{_filePrefix}/Data/NameFetching/book_names.json");
            }

            MDBookNames completedNames = MergeBookNames([names, _abbreviations]);

            Log.Information("MetadataFetchingService: Serializing and writing book names to file...");
            string serializedNames = JsonSerializer.Serialize(completedNames, _serializerOptions);
            await File.WriteAllTextAsync($"{_filePrefix}/Data/NameFetching/book_names.json", serializedNames);

            Log.Information("MetadataFetchingService: Finished.");
        }

        private async Task<MDBGVersionData> GetBibleGatewayVersionData(IEnumerable<Version> versions)
        {
            MDBGVersionData versionData = [];

            foreach (Version version in versions)
            {
                string url = null;

                if (version.InternalId == null)
                {
                    string versionListResp = await _httpClient.GetStringAsync("https://www.biblegateway.com/versions/");
                    IDocument versionListDocument = await BrowsingContext.New().OpenAsync(req => req.Content(versionListResp));
                    IEnumerable<IElement> translationElements = versionListDocument.All.Where(el => el.ClassList.Contains("translation-name"));

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
                }
                else
                {
                    url = $"https://www.biblegateway.com/versions/{version.InternalId}/#booklist";
                }

                if (url == null)
                {
                    // This shouldn't happen. If it does, something's gone wrong in the process
                    // above *or* something's changed with the translation on BibleGateway's end.
                    Log.Warning($"MetadataFetchingService: Could not obtain BibleGateway booklist URL for '{version.Name}', skipping...");
                    continue;
                }

                HttpResponseMessage booklistResp = await _httpClient.GetAsync(url);

                if (booklistResp.IsSuccessStatusCode)
                {
                    versionData.Add(version, booklistResp);
                }
                else
                {
                    Log.Warning($"MetadataFetchingService: Received {booklistResp.StatusCode} when fetching data for '{version.Name}', skipping...");
                }
            }

            return versionData;
        }

        private async Task<MDABVersionData> GetAPIBibleVersionData(IEnumerable<Version> versions)
        {
            MDABVersionData versionData = [];

            foreach (Version version in versions)
            {
                RestRequest req = new($"bibles/{version.InternalId}/books?include-chapters-and-sections=true");
                req.AddHeader("api-key", Environment.GetEnvironmentVariable("APIBIBLE_TOKEN")!);

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
                else
                {
                    Log.Warning($"MetadataFetchingService: Received null when fetching data for '{version.Name}', skipping...");
                }
            }

            return versionData;
        }

        private async Task<MDBookNames> GetDBBookNames()
        {
            MDBookNames names = [];

            List<Version> versions = await _versionService.Get();

            foreach (Book book in versions.Where(version => version.Books != null).Select(version => version.Books).SelectMany(bookData => bookData))
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

            return names;
        }

        private async Task SaveAPIBibleMetadata(MDABVersionData versionData)
        {
            List<string> latterKings = ["3 Kings", "4 Kings"];

            foreach ((Version version, ABBooksResponse resp) in versionData)
            {
                List<Book> versionBookData = [];

                foreach (ABBook book in resp.Data)
                {
                    bool usesVariant = false;
                    string properDataName = "";

                    switch (book.Id)
                    {
                        case "DAG":
                            usesVariant = true;
                            properDataName = "DAN";
                            break;
                        case "PS2":
                            usesVariant = true;
                            properDataName = "PSA";
                            break;
                    }

                    if (usesVariant)
                    {
                        Log.Warning($"MetadataFetchingService: \"{version.Name}\" uses variant data name \"{book.Id}\", skipping in favor of \"{properDataName}\".");
                        continue;
                    }

                    if (!_defaultNames.Contains(book.Id))
                    {
                        Log.Warning($"MetadataFetchingService: Id \"{book.Id}\" for '{book.Name}' in {version.Name} ({version.InternalId}) does not exist in default_names.json, skipping...");
                        continue;
                    }

                    string properName;
                    string internalId = book.Id;

                    if (_bookMap["ot"].TryGetValue(internalId, out string otName))
                    {
                        properName = otName;
                    }
                    else if (_bookMap["nt"].TryGetValue(internalId, out string ntName))
                    {
                        properName = ntName;
                    }
                    else if (_bookMap["deu"].TryGetValue(internalId, out string deuName))
                    {
                        properName = deuName;
                    }
                    else
                    {
                        if ((version.Abbreviation is "ELXX" or "LXX") && internalId == "DAG")
                        {
                            // TODO: remove these branches, there's no scenario where this will happen.
                            properName = _bookMap["deu"]["DAN"];
                        }
                        else
                        {
                            Log.Warning($"MetadataFetchingService: API.Bible translation \"{book.Id}\" for \"{version.Name}\" not in apibible_names.json.");
                            continue;
                        }
                    }

                    book.Name = (IsNuisance(book.Name) || book.Name.EndsWith('.')) ? properName : book.Name.Trim();

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
                            List<Tuple<int, int, string>> titles = [];
                            titles.AddRange(from section in chapter.Sections
                                            let firstVerseIdSplit = section.FirstVerseOrgId.Split('.')
                                            let lastVerseIdSplit = section.FirstVerseOrgId.Split('.')
                                            let firstVerseNumber = int.Parse(firstVerseIdSplit.Last())
                                            let lastVerseNumber = int.Parse(lastVerseIdSplit.Last())
                                            select new Tuple<int, int, string>(firstVerseNumber, lastVerseNumber, section.Title)
                            );

                            chapters.Add(new Chapter
                            {
                                Number = parsedNumber,
                                Titles = titles.Count == 0 ? null : titles
                            });
                        }
                        else
                        {
                            Log.Warning($"MetadataFetchingService: Ignoring chapter '{chapter.Id}' in '{version.Name}' with non-numeric chapter value.");
                        }
                    }

                    versionBookData.Add(new Book
                    {
                        Name = internalId,
                        ProperName = properName,
                        InternalName = book.Id,
                        PreferredName = book.Name,
                        Chapters = [.. chapters]
                    });
                }

                UpdateDefinition<Version> update = Builders<Version>.Update.Set(versionToUpdate => versionToUpdate.Books, [.. versionBookData]);
                await _versionService.Update(version.Abbreviation, update);
            }
        }

        private async Task SaveBibleGatewayMetadata(MDBGVersionData versionData)
        {
            List<string> threeMaccVariants = ["3ma", "3macc", "3m"];
            List<string> fourMaccVariants = ["4ma", "4macc", "4m"];
            List<string> greekEstherVariants = ["gkest", "gkesth", "gkes"];
            List<string> addEstherVariants = ["addesth", "adest"];
            List<string> prayerAzariahVariants = ["praz", "prazar"];
            List<string> songThreeYouthsVariants = ["sgthr", "sgthree"];

            foreach ((Version version, HttpResponseMessage resp) in versionData)
            {
                List<Book> versionBookData = [];

                IDocument bookListDocument = await BrowsingContext.New().OpenAsync(async void (req) =>
                {
                    try
                    {
                        req.Content(await resp.Content.ReadAsStringAsync());
                    }
                    catch (Exception e)
                    {
                        Log.Error($"MetadataFetchingService: Exception caught - {e}");
                    }
                });

                IEnumerable<IElement> bookNames = bookListDocument.All.Where(el => el.ClassList.Contains("book-name"));
                foreach (IElement el in bookNames)
                {
                    foreach (IElement span in el.GetElementsByTagName("span"))
                    {
                        span.Remove();
                    }

                    if (!el.HasAttribute("data-target"))
                    {
                        continue;
                    }

                    string bgDataName = el.GetAttribute("data-target").Substring(1, el.GetAttribute("data-target").Length - 6);
                    string bookName = el.TextContent.Trim();

                    bool usesVariant = false;
                    string origDataName = "";

                    if (threeMaccVariants.Contains(bgDataName))
                    {
                        usesVariant = true;
                        origDataName = bgDataName;
                        bgDataName = "3ma";
                    }
                    else if (fourMaccVariants.Contains(bgDataName))
                    {
                        usesVariant = true;
                        origDataName = bgDataName;
                        bgDataName = "4ma";
                    }
                    else if (greekEstherVariants.Contains(bgDataName))
                    {
                        usesVariant = true;
                        origDataName = bgDataName;
                        bgDataName = "gkest";
                    }
                    else if (addEstherVariants.Contains(bgDataName))
                    {
                        usesVariant = true;
                        origDataName = bgDataName;
                        bgDataName = "addesth";
                    }
                    else if (prayerAzariahVariants.Contains(bgDataName))
                    {
                        usesVariant = true;
                        origDataName = bgDataName;
                        bgDataName = "praz";
                    }
                    else if (songThreeYouthsVariants.Contains(bgDataName))
                    {
                        usesVariant = true;
                        origDataName = bgDataName;
                        bgDataName = "sgthr";
                    }

                    switch (usesVariant)
                    {
                        case true when bgDataName != origDataName:
                            Log.Warning($"MetadataFetchingService: \"{version.Name}\" uses BG variant data name \"{origDataName}\", operating as \"{bgDataName}\".");
                            break;
                        case true:
                            Log.Warning($"MetadataFetchingService: \"{version.Name}\" uses BG data name \"{bgDataName}\".");
                            break;
                    }

                    if (!_bibleGatewayNames.ContainsKey(bgDataName))
                    {
                        Log.Warning($"MetadataFetchingService: BG data name \"{bgDataName}\" for \"{version.Name}\" not in biblegateway_names.json.");
                    }

                    if (IsNuisance(bookName))
                    {
                        continue;
                    }

                    string properName;
                    string internalName = bgDataName;
                    string internalId = _bibleGatewayNames[bgDataName];

                    if (_bookMap["ot"].TryGetValue(internalId, out string otName))
                    {
                        properName = otName;
                    }
                    else if (_bookMap["nt"].TryGetValue(internalId, out string ntName))
                    {
                        properName = ntName;
                    }
                    else if (_bookMap["deu"].TryGetValue(internalId, out string deuName))
                    {
                        properName = deuName;
                    }
                    else
                    {
                        Log.Warning($"MetadataFetchingService: Data name \"{bgDataName}\" for \"{version.Name}\" not in book_map.json.");
                        continue;
                    }

                    if (bgDataName == "ps151")
                    {
                        Book psalms = versionBookData.First(book => book.Name == "PSA");
                        int indexOfPsalms = versionBookData.IndexOf(psalms);

                        psalms.Chapters = [
                            .. psalms.Chapters,
                            new Chapter
                            {
                                Number = 151,
                            }
                        ];

                        versionBookData[indexOfPsalms] = psalms;
                        continue;
                    }

                    List<Chapter> chapters = [];

                    foreach (IElement chapter in el.ParentElement.GetElementsByClassName("chapters")[0].GetElementsByTagName("a"))
                    {
                        string chapterNumber = chapter.TextContent;
                        if (int.TryParse(chapterNumber, out int parsedNumber))
                        {
                            chapters.Add(new Chapter
                            {
                                Number = parsedNumber
                            });
                        }
                        else
                        {
                            Log.Warning($"MetadataFetchingService: Ignoring chapter '{internalId}.{chapterNumber}' in '{version.Name}' with non-numeric chapter value.");
                        }
                    }

                    versionBookData.Add(new Book
                    {
                        Name = internalId,
                        ProperName = properName,
                        PreferredName = bookName,
                        InternalName = internalName,
                        Chapters = [.. chapters]
                    });
                }

                string versionInternalId = version.InternalId ?? resp.RequestMessage.RequestUri.AbsoluteUri.Replace("https://www.biblegateway.com/versions/", "").Replace("/#booklist", "");
                UpdateDefinition<Version> update = Builders<Version>.Update.Set(versionToUpdate => versionToUpdate.Books, [.. versionBookData]).Set(versionToUpdate => versionToUpdate.InternalId, versionInternalId);
                await _versionService.Update(version.Abbreviation, update);
            }
        }

        public MDVersionBookList GetVersionBookList(Version version)
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

                        switch (book.PreferredName)
                        {
                            case "Ezra":
                                book.PreferredName += "/Nehemiah";
                                break;
                            case "Psalm":
                                book.PreferredName += "s";
                                break;
                        }
                    }
                    else if (isDEU)
                    {
                        book.PreferredName = _bookMap["deu"][book.Name];
                    }
                }

                if (book.Name == "ps")
                {
                    if (book.Chapters.Any(chapter => chapter.Number == 151))
                    {
                        try
                        {
                            names[BookCategories.OldTestament]["PSA"] = $"{names[BookCategories.OldTestament]["PSA"]} <151>";
                        }
                        catch (KeyNotFoundException)
                        {
                            names[BookCategories.OldTestament].Add(book.Name, $"{book.PreferredName} <151>");
                        }
                    }
                }

                if (!names.TryGetValue(category, out Dictionary<string, string> defaultNames))
                {
                    defaultNames = ([]);
                    names.Add(category, defaultNames);
                }

                defaultNames.TryAdd(book.Name, book.PreferredName);

                if (book.Name == "EZK" && version.Abbreviation == "ELXX")
                {
                    defaultNames.TryAdd("DAN", "Daniel");
                }
            }

            return names;
        }

        private bool IsNuisance(string word) => _nuisances.Contains(word.ToLowerInvariant()) || _nuisances.Contains($"{word.ToLowerInvariant()}.");

        private static MDBookNames MergeBookNames(List<MDBookNames> bookNames) => bookNames.SelectMany(dict => dict).ToLookup(pair => pair.Key, pair => pair.Value).ToDictionary(group => group.Key, group => group.SelectMany(list => list).ToList());
    }
}
