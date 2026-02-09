/*
* Copyright (C) 2016-2026 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using BibleBot.Backend.Services.Providers.Metadata;
using BibleBot.Models;
using MongoDB.Driver;
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
        private readonly MDBookNames _abbreviations;
        private MDBookNames _bookNames = [];
        private List<string> _defaultNames;
        private readonly MDBookMap _bookMap;
        private readonly List<string> _nuisances;

        private static readonly JsonSerializerOptions _serializerOptions = new() { PropertyNameCaseInsensitive = false };
        private readonly string _filePrefix = ".";

        private readonly VersionService _versionService;
        private readonly BibleGatewayProvider _bibleGatewayProvider;
        private readonly APIBibleProvider _apiBibleProvider;

        public MetadataFetchingService(VersionService versionService, bool isForAutoServ)
        {
            if (isForAutoServ)
            {
                _filePrefix = "../BibleBot.Backend";
            }

            string abbreviationsText = File.ReadAllText($"{_filePrefix}/Data/NameFetching/abbreviations.json");
            _abbreviations = JsonSerializer.Deserialize<MDBookNames>(abbreviationsText);

            string defaultNamesText = File.ReadAllText($"{_filePrefix}/Data/NameFetching/default_names.json");
            _defaultNames = JsonSerializer.Deserialize<List<string>>(defaultNamesText);

            string nuisancesText = File.ReadAllText($"{_filePrefix}/Data/NameFetching/nuisances.json");
            _nuisances = JsonSerializer.Deserialize<List<string>>(nuisancesText);

            string bookMapText = File.ReadAllText($"{_filePrefix}/Data/book_map.json");
            _bookMap = JsonSerializer.Deserialize<MDBookMap>(bookMapText);

            _versionService = versionService;

            _bibleGatewayProvider = new BibleGatewayProvider(_nuisances, _bookMap);
            _bibleGatewayProvider.GetNameMapping(_filePrefix);

            _apiBibleProvider = new APIBibleProvider(_nuisances, _bookMap, _defaultNames);
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

            List<Version> abVersions = [.. versions.Where(version => version.Source == "ab" && version.Books == null && version.AliasOf == null)];
            List<Version> bgVersions = [.. versions.Where(version => version.Source == "bg" && version.Books == null && version.AliasOf == null)];

            if (abVersions!.Count != 0)
            {
                Log.Information("MetadataFetchingService: Getting API.Bible version metadata...");
                MDABVersionData abVersionData = await _apiBibleProvider.GetVersionData(abVersions);

                Log.Information("MetadataFetchingService: Saving API.Bible version metadata into database...");
                await SaveMetadata(abVersionData);
            }

            if (bgVersions!.Count != 0)
            {
                Log.Information("MetadataFetchingService: Getting BibleGateway version metadata...");
                MDBGVersionData bgVersionData = await _bibleGatewayProvider.GetVersionData(bgVersions);

                Log.Information("MetadataFetchingService: Saving BibleGateway version metadata into database...");
                await SaveMetadata(bgVersionData);
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

        private async Task SaveMetadata<T>(Dictionary<Version, T> versionData)
        {
            foreach ((Version version, T resp) in versionData)
            {
                UpdateDefinition<Version> update = typeof(T).Name switch
                {
                    nameof(HttpResponseMessage) => await _bibleGatewayProvider.GenerateMetadataUpdate(version, resp as HttpResponseMessage),
                    nameof(ABBooksResponse) => _apiBibleProvider.GenerateMetadataUpdate(version, resp as ABBooksResponse),
                    _ => throw new NotSupportedException("Attempted to save metadata with an unknown response type."),
                };

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
                            default:
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
                    defaultNames = [];
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

        private static MDBookNames MergeBookNames(List<MDBookNames> bookNames) => bookNames
                .SelectMany(dict => dict)
                .ToLookup(pair => pair.Key, pair => pair.Value)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .SelectMany(list => list)
                        .OrderByDescending(s => s?.Length ?? 0)
                        .DistinctBy(s => s.ToLowerInvariant())
                        .ToList()
                );
    }
}
