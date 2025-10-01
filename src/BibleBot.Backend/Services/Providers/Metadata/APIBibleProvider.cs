/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BibleBot.Models;
using MongoDB.Driver;
using RestSharp;
using Sentry;
using Serilog;
using MDABVersionData = System.Collections.Generic.Dictionary<BibleBot.Models.Version, BibleBot.Models.ABBooksResponse>;
using MDBookMap = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, string>>;
using Version = BibleBot.Models.Version;

namespace BibleBot.Backend.Services.Providers.Metadata
{
    public class APIBibleProvider(List<string> nuisances, MDBookMap bookMap, List<string> defaultNames)
    {
        private readonly List<string> _nuisances = nuisances;
        private readonly MDBookMap _bookMap = bookMap;
        private readonly List<string> _defaultNames = defaultNames;

        private readonly RestClient _restClient = new("https://api.scripture.api.bible/v1");

        private readonly List<string> _latterKings = ["3 Kings", "4 Kings"];

        public async Task<MDABVersionData> GetVersionData(IEnumerable<Version> versions)
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
                            Log.Warning("APIBibleProvider: Received unauthorized from API.Bible, WE MIGHT BE RATE LIMITED");
                        }
                        return [];
                    }
                    SentrySdk.CaptureException(ex);
                }

                if (resp != null)
                {
                    versionData.Add(version, resp);
                }
                else
                {
                    Log.Warning($"APIBibleProvider: Received null when fetching data for '{version.Name}', skipping...");
                }
            }

            return versionData;
        }

        public UpdateDefinition<Version> GenerateMetadataUpdate(Version version, ABBooksResponse resp)
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
                    default:
                        break;
                }

                if (usesVariant)
                {
                    Log.Warning($"APIBibleProvider: \"{version.Name}\" uses variant data name \"{book.Id}\", skipping in favor of \"{properDataName}\".");
                    continue;
                }

                if (!_defaultNames.Contains(book.Id))
                {
                    Log.Warning($"APIBibleProvider: Id \"{book.Id}\" for '{book.Name}' in {version.Name} ({version.InternalId}) does not exist in default_names.json, skipping...");
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
                        Log.Warning($"APIBibleProvider: \"{book.Id}\" for \"{version.Name}\" not in apibible_names.json.");
                        continue;
                    }
                }

                book.Name = (IsNuisance(book.Name) || book.Name.EndsWith('.')) ? properName : book.Name.Trim();

                if ((internalId == "1sam" && book.Name == "1 Kings") || (internalId == "2sam" && book.Name == "2 Kings") || _latterKings.Contains(book.Abbreviation))
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
                        Log.Warning($"APIBibleProvider: Ignoring chapter '{chapter.Id}' in '{version.Name}' with non-numeric chapter value.");
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

            return Builders<Version>.Update.Set(versionToUpdate => versionToUpdate.Books, [.. versionBookData]);
        }

        private bool IsNuisance(string word) => _nuisances.Contains(word.ToLowerInvariant()) || _nuisances.Contains($"{word.ToLowerInvariant()}.");
    }
}