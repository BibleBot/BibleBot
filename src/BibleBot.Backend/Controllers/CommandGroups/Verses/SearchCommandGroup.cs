/*
* Copyright (C) 2016-2023 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BibleBot.Backend.Models;
using BibleBot.Backend.Services;
using BibleBot.Backend.Services.Providers;
using NodaTime;

namespace BibleBot.Backend.Controllers.CommandGroups.Verses
{
    public class SearchCommandGroup : ICommandGroup
    {
        public string Name { get; set; }
        public bool IsOwnerOnly { get; set; }
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
            IsOwnerOnly = false;
            Commands = new List<ICommand>
            {
                new Search(_userService, _guildService, _versionService, _bibleProviders)
            };
            DefaultCommand = Commands.Where(cmd => cmd.Name == "usage").FirstOrDefault();
        }

        public class Search : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }
            public bool BotAllowed { get; set; }

            private readonly UserService _userService;
            private readonly GuildService _guildService;
            private readonly VersionService _versionService;

            private readonly List<IBibleProvider> _bibleProviders;

            public Search(UserService userService, GuildService guildService, VersionService versionService,
                          List<IBibleProvider> bibleProviders)
            {
                Name = "usage";
                ArgumentsError = null;
                ExpectedArguments = 0;
                PermissionsRequired = null;
                BotAllowed = false;

                _userService = userService;
                _guildService = guildService;
                _versionService = versionService;

                _bibleProviders = bibleProviders;
            }

            public IResponse ProcessCommand(Request req, List<string> args)
            {
                var idealUser = _userService.Get(req.UserId);
                var idealGuild = _guildService.Get(req.GuildId);

                var version = "RSV";

                if (idealUser != null)
                {
                    version = idealUser.Version;
                }
                else if (idealGuild != null)
                {
                    version = idealGuild.Version;
                }

                var idealVersion = _versionService.Get(version);
                var query = System.String.Join(" ", args);

                if (query.Length < 4)
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages = new List<InternalEmbed>
                        {
                            new Utils().Embedify("/search", "Your search query needs to be at least 4 characters.", true)
                        },
                        LogStatement = "/search"
                    };
                }

                IBibleProvider provider = _bibleProviders.Where(pv => pv.Name == idealVersion.Source).FirstOrDefault();

                if (provider == null)
                {
                    throw new ProviderNotFoundException();
                }

                List<SearchResult> searchResults = provider.Search(System.String.Join(" ", args), idealVersion).GetAwaiter().GetResult();

                if (searchResults.Count > 1)
                {
                    var pages = new List<InternalEmbed>();
                    var maxResultsPerPage = 6;
                    var referencesUsed = new List<string>();

                    var totalPages = (int)System.Math.Ceiling((decimal)(searchResults.Count / maxResultsPerPage));

                    if (totalPages > 100)
                    {
                        totalPages = 100;
                    }

                    if (totalPages == 0)
                    {
                        totalPages = 1;
                    }

                    var title = "Search results for \"{0}\"";
                    var pageCounter = "Page {0} of {1}";

                    for (int i = 0; i < totalPages; i++)
                    {
                        var embed = new Utils().Embedify(System.String.Format(title, query), System.String.Format(pageCounter, i + 1, totalPages), false);
                        embed.Fields = new List<EmbedField>();

                        var count = 0;

                        foreach (var searchResult in searchResults)
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
                        Pages = new List<InternalEmbed>
                        {
                            new Utils().Embedify("/search", "Your search query produced no results.", true)
                        },
                        LogStatement = "/search"
                    };
                }
            }
        }
    }
}
