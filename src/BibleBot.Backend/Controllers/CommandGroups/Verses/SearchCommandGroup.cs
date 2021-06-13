using System.Linq;
using System.Globalization;
using System.Collections.Generic;

using NodaTime;

using BibleBot.Lib;
using BibleBot.Backend.Models;
using BibleBot.Backend.Services;
using BibleBot.Backend.Services.Providers;

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

        private readonly BibleGatewayProvider _bgProvider;

        public SearchCommandGroup(UserService userService, GuildService guildService, VersionService versionService,
                                  BibleGatewayProvider bgProvider)
        {
            _userService = userService;
            _guildService = guildService;
            _versionService = versionService;

            _bgProvider = bgProvider;

            Name = "search";
            IsOwnerOnly = false;
            Commands = new List<ICommand>
            {
                new Search(_userService, _guildService, _versionService, _bgProvider)
            };
            DefaultCommand = Commands.Where(cmd => cmd.Name == "usage").FirstOrDefault();
        }

        public class Search : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }

            private readonly UserService _userService;
            private readonly GuildService _guildService;
            private readonly VersionService _versionService;

            private readonly BibleGatewayProvider _bgProvider;

            public Search(UserService userService, GuildService guildService, VersionService versionService,
                          BibleGatewayProvider bgProvider)
            {
                Name = "usage";
                ArgumentsError = null;
                ExpectedArguments = 0;
                PermissionsRequired = null;

                _userService = userService;
                _guildService = guildService;
                _versionService = versionService;

                _bgProvider = bgProvider;
            }

            public IResponse ProcessCommand(Request req, List<string> args)
            {
                var idealUser = _userService.Get(req.UserId);

                var version = "RSV";

                if (idealUser != null)
                {
                    version = idealUser.Version;
                }

                var idealVersion = _versionService.Get(version);

                if (idealVersion == null)
                {
                    idealVersion = _versionService.Get("RSV");
                }

                var query = System.String.Join(" ", args);

                if (query.Length < 4)
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages = new List<InternalEmbed>
                        {
                            new Utils().Embedify("+search", "Your search query needs to be at least 4 characters.", true)
                        },
                        LogStatement = "+search"
                    };
                }


                List<SearchResult> searchResults = _bgProvider.Search(System.String.Join(" ", args), idealVersion).GetAwaiter().GetResult();

                if (searchResults.Count > 1)
                {
                    var pages = new List<InternalEmbed>();
                    var maxResultsPerPage = 6;
                    var referencesUsed = new List<string>();

                    var totalPages = (int) System.Math.Ceiling((decimal) (searchResults.Count / maxResultsPerPage));
                    totalPages++;

                    if (totalPages > 100)
                    {
                        totalPages = 100;
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
                        LogStatement = $"+search {query}"
                    };
                }
                else
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages = new List<InternalEmbed>
                        {
                            new Utils().Embedify("+search", "Your search query produced no results.", true)
                        },
                        LogStatement = "+search"
                    };
                }
            }
        }
    }
}