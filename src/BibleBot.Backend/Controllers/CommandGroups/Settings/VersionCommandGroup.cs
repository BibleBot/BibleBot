using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using BibleBot.Lib;
using BibleBot.Backend.Models;
using BibleBot.Backend.Services;

namespace BibleBot.Backend.Controllers.CommandGroups.Settings
{
    public class VersionCommandGroup : ICommandGroup
    {
        public string Name { get; set; }
        public bool IsOwnerOnly { get; set; }
        public ICommand DefaultCommand { get; set; }
        public List<ICommand> Commands { get; set; }

        private readonly UserService _userService;
        private readonly GuildService _guildService;
        private readonly VersionService _versionService;

        public VersionCommandGroup(UserService userService, GuildService guildService, VersionService versionService)
        {
            _userService = userService;
            _guildService = guildService;
            _versionService = versionService;

            Name = "version";
            IsOwnerOnly = false;
            Commands = new List<ICommand>
            {
                new VersionUsage(_userService, _guildService, _versionService),
                new VersionSet(_userService, _guildService, _versionService),
                new VersionSetServer(_userService, _guildService, _versionService),
                new VersionInfo(_userService, _guildService, _versionService),
                new VersionList(_userService, _guildService, _versionService)
            };
            DefaultCommand = Commands.Where(cmd => cmd.Name == "usage").FirstOrDefault();
        }

        public class VersionUsage : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }

            private readonly UserService _userService;
            private readonly GuildService _guildService;
            private readonly VersionService _versionService;

            public VersionUsage(UserService userService, GuildService guildService, VersionService versionService)
            {
                Name = "usage";
                ArgumentsError = null;
                ExpectedArguments = 0;
                PermissionsRequired = null;

                _userService = userService;
                _guildService = guildService;
                _versionService = versionService;
            }

            public IResponse ProcessCommand(Request req, List<string> args)
            {
                var idealUser = _userService.Get(req.UserId);
                var idealGuild = _guildService.Get(req.GuildId);

                var defaultVersion = _versionService.Get("RSV");

                var response = "Your preferred version is set to **<version>**.\n" +
                               "The server's preferred version is set to **<gversion>**.\n\n" +
                               "__**Subcommands**__\n" +
                               "**set** - set your preferred version\n" +
                               "**setserver** - set the server's default version (staff only)\n" +
                               "**info** - get information on a version\n" +
                               "**list** - list all available versions";

                if (idealUser != null)
                {
                    var idealUserVersion = _versionService.Get(idealUser.Version);
                    
                    if (idealUserVersion != null)
                    {
                        response = response.Replace("<version>", idealUserVersion.Name);   
                    }
                }

                if (idealGuild != null)
                {
                    var idealGuildVersion = _versionService.Get(idealGuild.Version);
                    
                    if (idealGuildVersion != null)
                    {
                        response = response.Replace("<gversion>", idealGuildVersion.Name);   
                    }
                }

                response = response.Replace("<version>", defaultVersion.Name);
                response = response.Replace("<gversion>", defaultVersion.Name);

                return new CommandResponse
                {
                    OK = true,
                    Pages = new List<InternalEmbed>
                    {
                        new Utils().Embedify("+version", response, false)
                    },
                    LogStatement = "+version"
                };
            }
        }

        public class VersionSet : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }

            private readonly UserService _userService;
            private readonly GuildService _guildService;
            private readonly VersionService _versionService;

            public VersionSet(UserService userService, GuildService guildService, VersionService versionService)
            {
                Name = "set";
                ArgumentsError = "Expected a version abbreviation parameter, like `RSV` or `KJV`.";
                ExpectedArguments = 1;
                PermissionsRequired = null;

                _userService = userService;
                _guildService = guildService;
                _versionService = versionService;
            }

            public IResponse ProcessCommand(Request req, List<string> args)
            {
                var newVersion = args[0].ToUpperInvariant();
                var idealVersion = _versionService.Get(newVersion);

                if (idealVersion != null)
                {
                    var idealUser = _userService.Get(req.UserId);

                    if (idealUser != null)
                    {
                        idealUser.Version = idealVersion.Abbreviation;
                        _userService.Update(req.UserId, idealUser);
                    }
                    else
                    {
                        _userService.Create(new User
                        {
                            UserId = req.UserId,
                            Version = idealVersion.Abbreviation,
                            InputMethod = "default",
                            Language = "english",
                            TitlesEnabled = true,
                            VerseNumbersEnabled = true,
                            DisplayStyle = "embed"
                        });
                    }

                    return new CommandResponse
                    {
                        OK = true,
                        Pages = new List<InternalEmbed>
                        {
                            new Utils().Embedify("+version set", "Set version successfully.", false)
                        },
                        LogStatement = $"+version set {args[0]}"
                    };
                }

                return new CommandResponse
                {
                    OK = false,
                    Pages = new List<InternalEmbed>
                    {
                        new Utils().Embedify("+version set", "Failed to set version, see `+version list`.", true)
                    },
                    LogStatement = "+version set"
                };
            }
        }

        public class VersionSetServer : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }

            private readonly UserService _userService;
            private readonly GuildService _guildService;
            private readonly VersionService _versionService;

            public VersionSetServer(UserService userService, GuildService guildService, VersionService versionService)
            {
                Name = "setserver";
                ArgumentsError = "Expected a version abbreviation parameter, like `RSV` or `KJV`.";
                ExpectedArguments = 1;
                PermissionsRequired = new List<Permissions>
                {
                    Permissions.MANAGE_GUILD
                };

                _userService = userService;
                _guildService = guildService;
                _versionService = versionService;
            }

            public IResponse ProcessCommand(Request req, List<string> args)
            {
                var newVersion = args[0].ToUpperInvariant();
                var idealVersion = _versionService.Get(newVersion);

                if (idealVersion != null)
                {
                    var idealGuild = _guildService.Get(req.GuildId);

                    if (idealGuild != null)
                    {
                        idealGuild.Version = idealVersion.Abbreviation;
                        _guildService.Update(req.GuildId, idealGuild);
                    }
                    else
                    {
                        _guildService.Create(new Guild
                        {
                            GuildId = req.GuildId,
                            Version = idealVersion.Abbreviation,
                            Language = "english",
                            Prefix = "+",
                            IgnoringBrackets = "<>",
                            IsDM = req.IsDM
                        });
                    }

                    return new CommandResponse
                    {
                        OK = true,
                        Pages = new List<InternalEmbed>
                        {
                            new Utils().Embedify("+version setserver", "Set server version successfully.", false)
                        },
                        LogStatement = $"+version setserver {args[0]}"
                    };
                }

                return new CommandResponse
                {
                    OK = false,
                    Pages = new List<InternalEmbed>
                    {
                        new Utils().Embedify("+version setserver", "Failed to set server version, see `+version list`.", true)
                    },
                    LogStatement = "+version setserver"
                };
            }
        }

        public class VersionInfo : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }

            private readonly UserService _userService;
            private readonly GuildService _guildService;
            private readonly VersionService _versionService;

            public VersionInfo(UserService userService, GuildService guildService, VersionService versionService)
            {
                Name = "info";
                ArgumentsError = "Expected a version abbreviation parameter, like `RSV` or `KJV`.";
                ExpectedArguments = 1;
                PermissionsRequired = null;

                _userService = userService;
                _guildService = guildService;
                _versionService = versionService;
            }

            public IResponse ProcessCommand(Request req, List<string> args)
            {
                if (args.Count > 0)
                {
                    var idealVersion = _versionService.Get(args[0].ToUpperInvariant());

                    if (idealVersion != null)
                    {
                        return new CommandResponse
                        {
                            OK = true,
                            Pages = new List<InternalEmbed>
                            {
                                new Utils().Embedify("+version info",
                                $"**{idealVersion.Name}**\n\n" +
                                $"Contains Old Testament: {(idealVersion.SupportsOldTestament ? "Yes" : "No")}\n" +
                                $"Contains New Testament: {(idealVersion.SupportsNewTestament ? "Yes" : "No")}\n" +
                                $"Contains Apocrypha/Deuterocanon: {(idealVersion.SupportsDeuterocanon ? "Yes" : "No")}",
                                false)
                            },
                            LogStatement = $"+version info {args[0]}"
                        };
                    }
                }

                return new CommandResponse
                {
                    OK = false,
                    Pages = new List<InternalEmbed>
                    {
                        new Utils().Embedify("+version info", "I couldn't find that version, are you sure you used the right acronym?", true)
                    },
                    LogStatement = "+version info"
                };
            }
        }

        public class VersionList : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }

            private readonly UserService _userService;
            private readonly GuildService _guildService;
            private readonly VersionService _versionService;

            public VersionList(UserService userService, GuildService guildService, VersionService versionService)
            {
                Name = "list";
                ArgumentsError = null;
                ExpectedArguments = 0;
                PermissionsRequired = null;

                _userService = userService;
                _guildService = guildService;
                _versionService = versionService;
            }

            public IResponse ProcessCommand(Request req, List<string> args)
            {
                var versions = _versionService.Get();
                versions.Sort((x, y) => x.Name.CompareTo(y.Name));

                var versionsUsed = new List<string>();

                var pages = new List<InternalEmbed>();
                var maxResultsPerPage = 25;
                var totalPages = (int) System.Math.Ceiling((decimal) (versions.Count / maxResultsPerPage));
                totalPages++;

                for (int i = 0; i < totalPages; i++)
                {
                    var count = 0;
                    var versionList = "";

                    foreach (var version in versions)
                    {
                        if (count < maxResultsPerPage)
                        {
                            if (!versionsUsed.Contains(version.Name))
                            {
                                versionList += $"{version.Name}\n";

                                versionsUsed.Add(version.Name);
                                count++;
                            }
                        }
                    }

                    var embed = new Utils().Embedify($"+version list - Page {i + 1} of {totalPages}", versionList, false);
                    pages.Add(embed);
                }


                return new CommandResponse
                {
                    OK = true,
                    Pages = pages,
                    LogStatement = "+version list"
                };
            }
        }
    }
}