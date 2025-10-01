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
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using BibleBot.Models;
using MongoDB.Driver;
using Sentry;
using Serilog;
using MDBGVersionData = System.Collections.Generic.Dictionary<BibleBot.Models.Version, System.Net.Http.HttpResponseMessage>;
using MDBookMap = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, string>>;
using Version = BibleBot.Models.Version;

namespace BibleBot.Backend.Services.Providers.Metadata
{
    public class BibleGatewayProvider
    {
        private Dictionary<string, string> _nameMapping = [];
        private readonly MDBookMap _bookMap;
        private readonly List<string> _nuisances;
        private readonly HttpClient _httpClient;

        private readonly List<string> _threeMaccVariants = ["3ma", "3macc", "3m"];
        private readonly List<string> _fourMaccVariants = ["4ma", "4macc", "4m"];
        private readonly List<string> _greekEstherVariants = ["gkest", "gkesth", "gkes"];
        private readonly List<string> _addEstherVariants = ["addesth", "adest"];
        private readonly List<string> _prayerAzariahVariants = ["praz", "prazar"];
        private readonly List<string> _songThreeYouthsVariants = ["sgthr", "sgthree"];

        public BibleGatewayProvider(List<string> nuisances, MDBookMap bookMap)
        {
            _nuisances = nuisances;
            _bookMap = bookMap;

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.246");
        }

        public Dictionary<string, string> GetNameMapping(string _filePrefix = ".")
        {
            if (_nameMapping.Count != 0)
            {
                return _nameMapping;
            }

            string mappingText = File.ReadAllText($"{_filePrefix}/Data/NameFetching/biblegateway_names.json");
            _nameMapping = JsonSerializer.Deserialize<Dictionary<string, string>>(mappingText);

            return _nameMapping;
        }

        public async Task<MDBGVersionData> GetVersionData(IEnumerable<Version> versions)
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
                            if (targets[0].HasAttribute("href") && targets[0].TextContent.Equals(version.Name, StringComparison.InvariantCultureIgnoreCase))
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
                    Log.Warning($"BibleGatewayProvider: Could not obtain booklist URL for '{version.Name}', skipping...");
                    continue;
                }

                HttpResponseMessage booklistResp = await _httpClient.GetAsync(url);

                if (booklistResp.IsSuccessStatusCode)
                {
                    versionData.Add(version, booklistResp);
                }
                else
                {
                    Log.Warning($"BibleGatewayProvider: Received {booklistResp.StatusCode} when fetching data for '{version.Name}', skipping...");
                }
            }

            return versionData;
        }

        public async Task<UpdateDefinition<Version>> GenerateMetadataUpdate(Version version, HttpResponseMessage resp)
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
                    SentrySdk.CaptureException(e);
                    Log.Error($"BibleGatewayProvider: Exception caught - {e}");
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

                string dataName = el.GetAttribute("data-target").Substring(1, el.GetAttribute("data-target").Length - 6);
                string bookName = el.TextContent.Trim();

                bool usesVariant = false;
                string origDataName = "";

                if (_threeMaccVariants.Contains(dataName))
                {
                    usesVariant = true;
                    origDataName = dataName;
                    dataName = "3ma";
                }
                else if (_fourMaccVariants.Contains(dataName))
                {
                    usesVariant = true;
                    origDataName = dataName;
                    dataName = "4ma";
                }
                else if (_greekEstherVariants.Contains(dataName))
                {
                    usesVariant = true;
                    origDataName = dataName;
                    dataName = "gkest";
                }
                else if (_addEstherVariants.Contains(dataName))
                {
                    usesVariant = true;
                    origDataName = dataName;
                    dataName = "addesth";
                }
                else if (_prayerAzariahVariants.Contains(dataName))
                {
                    usesVariant = true;
                    origDataName = dataName;
                    dataName = "praz";
                }
                else if (_songThreeYouthsVariants.Contains(dataName))
                {
                    usesVariant = true;
                    origDataName = dataName;
                    dataName = "sgthr";
                }

                switch (usesVariant)
                {
                    case true when dataName != origDataName:
                        Log.Warning($"BibleGatewayProvider: \"{version.Name}\" uses variant data name \"{origDataName}\", saving as \"{dataName}\".");
                        break;
                    case true:
                        Log.Warning($"BibleGatewayProvider: \"{version.Name}\" uses data name \"{dataName}\".");
                        break;
                    default:
                        break;
                }

                if (!_nameMapping.ContainsKey(dataName))
                {
                    Log.Warning($"BibleGatewayProvider: Data name \"{dataName}\" for \"{version.Name}\" not in biblegateway_names.json.");
                }

                if (IsNuisance(bookName))
                {
                    continue;
                }

                string properName;
                string internalName = dataName;
                string internalId = _nameMapping[dataName];

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
                    Log.Warning($"BibleGatewayProvider: Data name \"{dataName}\" for \"{version.Name}\" not in book_map.json.");
                    continue;
                }

                if (dataName == "ps151")
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
                        Log.Warning($"BibleGatewayProvider: Ignoring chapter '{internalId}.{chapterNumber}' in '{version.Name}' with non-numeric chapter value.");
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
            return Builders<Version>.Update.Set(versionToUpdate => versionToUpdate.Books, [.. versionBookData]).Set(versionToUpdate => versionToUpdate.InternalId, versionInternalId);
        }

        private bool IsNuisance(string word) => _nuisances.Contains(word.ToLowerInvariant()) || _nuisances.Contains($"{word.ToLowerInvariant()}.");
    }
}