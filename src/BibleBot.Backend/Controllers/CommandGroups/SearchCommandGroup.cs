/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BibleBot.Backend.InternalModels;
using BibleBot.Backend.Services;
using BibleBot.Models;
using Microsoft.Extensions.Localization;
using Serilog;

namespace BibleBot.Backend.Controllers.CommandGroups
{
    public class SearchCommandGroup(UserService userService, GuildService guildService, VersionService versionService,
                                    MetadataFetchingService metadataFetchingService, List<IBibleProvider> bibleProviders,
                                    IStringLocalizerFactory localizerFactory) : CommandGroup
    {
        private readonly IStringLocalizer _localizer = localizerFactory.Create(typeof(SearchCommandGroup));
        private readonly IStringLocalizer _sharedLocalizer = localizerFactory.Create(typeof(SharedResource));

        public override string Name { get => "search"; set => throw new NotImplementedException(); }
        public override Command DefaultCommand { get => Commands.FirstOrDefault(cmd => cmd.Name == "usage"); set => throw new NotImplementedException(); }
        public override List<Command> Commands { get => [new Search(userService, guildService, versionService, metadataFetchingService, bibleProviders, _localizer, _sharedLocalizer)]; set => throw new NotImplementedException(); }

        public enum SubsetFlag
        {
            INVALID = 0,
            OT_ONLY = 1,
            NT_ONLY = 2,
            DEU_ONLY = 3
        }

        public class Search(UserService userService, GuildService guildService, VersionService versionService,
                            MetadataFetchingService metadataFetchingService, List<IBibleProvider> bibleProviders, IStringLocalizer localizer, IStringLocalizer sharedLocalizer) : Command
        {
            public override string Name { get => "usage"; set => throw new NotImplementedException(); }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                User idealUser = await userService.Get(req.UserId);
                Guild idealGuild = await guildService.Get(req.GuildId);
                SubsetFlag potentialSubset = SubsetFlag.INVALID;

                string versionParam = null;

                for (int i = 0; i < args.Count; i++)
                {
                    if (args[i].StartsWith("subset:"))
                    {
                        string[] subsetSplit = args[i].Split(":");

                        try
                        {
                            potentialSubset = (SubsetFlag)int.Parse(subsetSplit[1]);
                        }
                        catch
                        {
                            Log.Warning("Received an invalid subset, ignoring...");
                        }

                        args.Remove(args[i]);
                    }

                    if (args[i].StartsWith("version:"))
                    {
                        string[] versionSplit = args[i].Split(":");

                        try
                        {
                            versionParam = versionSplit[1] != "null" ? versionSplit[1] : null;
                        }
                        catch
                        {
                            Log.Warning("Received an invalid version, ignoring...");
                        }

                        args.Remove(args[i]);
                    }
                }

                Models.Version idealVersion = versionParam != null ? await versionService.Get(versionParam) : await versionService.GetPreferenceOrDefault(idealUser, idealGuild, false);

                if (idealVersion.Source != "bg" && potentialSubset != SubsetFlag.INVALID)
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages =
                        [
                            Utils.GetInstance().Embedify("/search", localizer["SearchSubsetVersionIneligible"], true)
                        ],
                        LogStatement = "/search",
                        Culture = CultureInfo.CurrentUICulture.Name
                    };
                }

                string query = string.Join(" ", args);

                IBibleProvider provider = bibleProviders.FirstOrDefault(pv => pv.Name == idealVersion.Source) ?? throw new ProviderNotFoundException($"Couldn't find provider for '/search' with {idealVersion.Abbreviation}.");
                List<SearchResult> searchResults = await provider.Search(query, idealVersion);


                if (searchResults.Count > 1)
                {
                    List<InternalEmbed> pages = [];
                    int maxResultsPerPage = 6;
                    List<string> referencesUsed = [];

                    Dictionary<BookCategories, Dictionary<string, string>> categoryMapping = potentialSubset != SubsetFlag.INVALID ? await metadataFetchingService.GetBibleGatewayVersionBookList(idealVersion) : null;

                    searchResults.RemoveAll(searchResult =>
                    {
                        if (potentialSubset != SubsetFlag.INVALID)
                        {
                            if ((!categoryMapping.ContainsKey(BookCategories.OldTestament) && potentialSubset == SubsetFlag.OT_ONLY) ||
                                (!categoryMapping.ContainsKey(BookCategories.NewTestament) && potentialSubset == SubsetFlag.NT_ONLY) ||
                                (!categoryMapping.ContainsKey(BookCategories.Deuterocanon) && potentialSubset == SubsetFlag.DEU_ONLY))
                            {
                                return true;
                            }

                            string bookName = searchResult.Reference.Split(" ")[0];

                            bool notOT = categoryMapping.ContainsKey(BookCategories.OldTestament) && potentialSubset == SubsetFlag.OT_ONLY && !categoryMapping[BookCategories.OldTestament].ContainsValue(bookName);
                            bool notNT = categoryMapping.ContainsKey(BookCategories.NewTestament) && potentialSubset == SubsetFlag.NT_ONLY && !categoryMapping[BookCategories.NewTestament].ContainsValue(bookName);
                            bool notDEU = categoryMapping.ContainsKey(BookCategories.Deuterocanon) && potentialSubset == SubsetFlag.DEU_ONLY && !categoryMapping[BookCategories.Deuterocanon].ContainsValue(bookName);

                            return notOT || notNT || notDEU;
                        }

                        return false;
                    });

                    if (searchResults.Count == 0 && potentialSubset != SubsetFlag.INVALID)
                    {
                        return new CommandResponse
                        {
                            OK = false,
                            Pages =
                            [
                                Utils.GetInstance().Embedify("/search", localizer["SearchNoResultsVersionMissingSubset"], true)
                            ],
                            LogStatement = "/search",
                            Culture = CultureInfo.CurrentUICulture.Name
                        };
                    }

                    int totalPages = (int)Math.Ceiling((decimal)(searchResults.Count / maxResultsPerPage));

                    if (totalPages > 100)
                    {
                        totalPages = 100;
                    }

                    if (totalPages == 0)
                    {
                        totalPages = 1;
                    }

                    string subsetString = "";
                    if (potentialSubset == SubsetFlag.OT_ONLY)
                    {
                        subsetString = $"{localizer["SearchSubsetOldTestament"]} ";
                    }
                    else if (potentialSubset == SubsetFlag.NT_ONLY)
                    {
                        subsetString = $"{localizer["SearchSubsetNewTestament"]} ";
                    }
                    else if (potentialSubset == SubsetFlag.DEU_ONLY)
                    {
                        subsetString = $"{localizer["SearchSubsetDeuterocanon"]} ";
                    }

                    string title = $"{localizer["SearchResultsTitle"]} \"{query}\" {subsetString}({idealVersion.Abbreviation})";
                    string pageCounter = sharedLocalizer["PageCounter"];

                    for (int i = 0; i < totalPages; i++)
                    {
                        InternalEmbed embed = Utils.GetInstance().Embedify(title, string.Format(pageCounter, i + 1, totalPages), false);
                        embed.Fields = [];

                        int count = 0;

                        foreach (SearchResult searchResult in searchResults)
                        {
                            if (searchResult.Text.Length < 700)
                            {
                                if (count < maxResultsPerPage && !referencesUsed.Contains(searchResult.Reference))
                                {
                                    embed.Fields.Add(new EmbedField
                                    {
                                        Name = searchResult.Reference,
                                        Value = searchResult.Text,
                                        Inline = false
                                    });

                                    referencesUsed.Add(searchResult.Reference);
                                    count++;
                                }
                            }
                        }

                        pages.Add(embed);
                    }

                    return new CommandResponse
                    {
                        OK = true,
                        Pages = pages,
                        LogStatement = $"/search {(potentialSubset != SubsetFlag.INVALID ? $"subset:{potentialSubset} " : "")}{query}",
                        Culture = CultureInfo.CurrentUICulture.Name
                    };
                }
                else
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages =
                        [
                            Utils.GetInstance().Embedify("/search", localizer["SearchNoResults"], true)
                        ],
                        LogStatement = "/search",
                        Culture = CultureInfo.CurrentUICulture.Name
                    };
                }
            }
        }
    }
}
