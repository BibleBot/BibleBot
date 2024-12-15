/*
* Copyright (C) 2016-2024 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BibleBot.Backend.Services;
using BibleBot.Models;

namespace BibleBot.Backend.Controllers.CommandGroups.Verses
{
    public class SearchCommandGroup : ICommandGroup
    {
        public string Name { get; set; }
        public bool IsStaffOnly { get; set; }
        public ICommand DefaultCommand { get; set; }
        public List<ICommand> Commands { get; set; }

        private readonly UserService _userService;
        private readonly GuildService _guildService;
        private readonly VersionService _versionService;

        private readonly List<IBibleProvider> _bibleProviders;

        public SearchCommandGroup(UserService userService, GuildService guildService, VersionService versionService,
                                  List<IBibleProvider> bibleProviders)
        {
            _userService = userService;
            _guildService = guildService;
            _versionService = versionService;

            _bibleProviders = bibleProviders;

            Name = "search";
            IsStaffOnly = false;
            Commands =
            [
                new Search(_userService, _guildService, _versionService, _bibleProviders)
            ];
            DefaultCommand = Commands.FirstOrDefault(cmd => cmd.Name == "usage");
        }

        public class Search(UserService userService, GuildService guildService, VersionService versionService,
                      List<IBibleProvider> bibleProviders) : ICommand
        {
            public string Name { get; set; } = "usage";
            public string ArgumentsError { get; set; } = null;
            public int ExpectedArguments { get; set; } = 0;
            public List<Permissions> PermissionsRequired { get; set; } = null;
            public bool BotAllowed { get; set; } = false;

            private readonly UserService _userService = userService;
            private readonly GuildService _guildService = guildService;
            private readonly VersionService _versionService = versionService;

            private readonly List<IBibleProvider> _bibleProviders = bibleProviders;

            public async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                User idealUser = await _userService.Get(req.UserId);
                Guild idealGuild = await _guildService.Get(req.GuildId);

                string version = "RSV";

                if (idealUser != null)
                {
                    version = idealUser.Version;
                }
                else if (idealGuild != null)
                {
                    version = idealGuild.Version;
                }

                Version idealVersion = await _versionService.Get(version) ?? await _versionService.Get("RSV");
                string query = string.Join(" ", args);

                IBibleProvider provider = _bibleProviders.FirstOrDefault(pv => pv.Name == idealVersion.Source) ?? throw new ProviderNotFoundException($"Couldn't find provider for '/search' with {idealVersion.Abbreviation}.");
                List<SearchResult> searchResults = await provider.Search(query, idealVersion);

                if (searchResults.Count > 1)
                {
                    List<InternalEmbed> pages = [];
                    int maxResultsPerPage = 6;
                    List<string> referencesUsed = [];

                    int totalPages = (int)System.Math.Ceiling((decimal)(searchResults.Count / maxResultsPerPage));

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
