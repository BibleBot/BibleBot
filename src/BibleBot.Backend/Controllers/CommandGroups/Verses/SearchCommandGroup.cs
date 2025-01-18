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
using System.Threading.Tasks;
using BibleBot.Backend.Services;
using BibleBot.Models;

namespace BibleBot.Backend.Controllers.CommandGroups.Verses
{
    public class SearchCommandGroup(UserService userService, GuildService guildService, VersionService versionService,
                                    List<IBibleProvider> bibleProviders) : CommandGroup
    {
        public override string Name { get => "search"; set => throw new NotImplementedException(); }
        public override Command DefaultCommand { get => Commands.FirstOrDefault(cmd => cmd.Name == "usage"); set => throw new NotImplementedException(); }
        public override List<Command> Commands { get => [new Search(userService, guildService, versionService, bibleProviders)]; set => throw new NotImplementedException(); }

        public class Search(UserService userService, GuildService guildService, VersionService versionService,
                            List<IBibleProvider> bibleProviders) : Command
        {
            public override string Name { get => "usage"; set => throw new NotImplementedException(); }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                User idealUser = await userService.Get(req.UserId);
                Guild idealGuild = await guildService.Get(req.GuildId);

                string version = "RSV";

                if (idealUser != null)
                {
                    version = idealUser.Version;
                }
                else if (idealGuild != null)
                {
                    version = idealGuild.Version;
                }

                Models.Version idealVersion = await versionService.Get(version) ?? await versionService.Get("RSV");
                string query = string.Join(" ", args);

                IBibleProvider provider = bibleProviders.FirstOrDefault(pv => pv.Name == idealVersion.Source) ?? throw new ProviderNotFoundException($"Couldn't find provider for '/search' with {idealVersion.Abbreviation}.");
                List<SearchResult> searchResults = await provider.Search(query, idealVersion);

                if (searchResults.Count > 1)
                {
                    List<InternalEmbed> pages = [];
                    int maxResultsPerPage = 6;
                    List<string> referencesUsed = [];

                    int totalPages = (int)Math.Ceiling((decimal)(searchResults.Count / maxResultsPerPage));

                    if (totalPages > 100)
                    {
                        totalPages = 100;
                    }

                    if (totalPages == 0)
                    {
                        totalPages = 1;
                    }

                    string title = "Search results for \"{0}\"";
                    string pageCounter = "Page {0} of {1}";

                    for (int i = 0; i < totalPages; i++)
                    {
                        InternalEmbed embed = Utils.GetInstance().Embedify(string.Format(title, query), string.Format(pageCounter, i + 1, totalPages), false);
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
                        LogStatement = $"/search {query}"
                    };
                }
                else
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages =
                        [
                            Utils.GetInstance().Embedify("/search", "Your search query produced no results.", true)
                        ],
                        LogStatement = "/search"
                    };
                }
            }
        }
    }
}
