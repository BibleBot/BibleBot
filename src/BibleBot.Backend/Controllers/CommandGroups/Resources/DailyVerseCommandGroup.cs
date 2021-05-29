using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using NodaTime;

using BibleBot.Lib;
using BibleBot.Backend.Models;
using BibleBot.Backend.Services;
using BibleBot.Backend.Services.Providers;

namespace BibleBot.Backend.Controllers.CommandGroups.Resources
{
    public class DailyVerseCommandGroup : ICommandGroup
    {
        public string Name { get; set; }
        public bool IsOwnerOnly { get; set; }
        public ICommand DefaultCommand { get; set; }
        public List<ICommand> Commands { get; set; }

        private readonly UserService _userService;
        private readonly GuildService _guildService;
        private readonly VersionService _versionService;

        private readonly BibleGatewayProvider _bgProvider;

        public DailyVerseCommandGroup(UserService userService, GuildService guildService, VersionService versionService,
                                      BibleGatewayProvider bgProvider)
        {
            _userService = userService;
            _guildService = guildService;
            _versionService = versionService;

            _bgProvider = bgProvider;

            Name = "dailyverse";
            IsOwnerOnly = false;
            Commands = new List<ICommand>
            {
                new DailyVerseUsage(_userService, _guildService, _versionService, _bgProvider),
                new DailyVerseSet(_userService, _guildService, _versionService)
            };
            DefaultCommand = Commands.Where(cmd => cmd.Name == "usage").FirstOrDefault();
        }

        public class DailyVerseUsage : ICommand
        {
            public string Name { get; set;}
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }

            private readonly UserService _userService;
            private readonly GuildService _guildService;
            private readonly VersionService _versionService;

            private readonly BibleGatewayProvider _bgProvider;

            public DailyVerseUsage(UserService userService, GuildService guildService, VersionService versionService,
                                   BibleGatewayProvider bgProvider)
            {
                Name = "usage";
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
                var idealGuild = _guildService.Get(req.GuildId);

                var version = "RSV";
                var verseNumbersEnabled = true;
                var titlesEnabled = true;
                var ignoringBrackets = "<>";
                var displayStyle = "embed";

                if (idealUser != null)
                {
                    version = idealUser.Version;
                    verseNumbersEnabled = idealUser.VerseNumbersEnabled;
                    titlesEnabled = idealUser.TitlesEnabled;
                    displayStyle = idealUser.DisplayStyle;
                }
                
                if (idealGuild != null)
                {
                    ignoringBrackets = idealGuild.IgnoringBrackets;
                }

                var idealVersion = _versionService.Get(version);

                if (idealVersion == null)
                {
                    idealVersion = _versionService.Get("RSV");
                }

                string votdRef = _bgProvider.GetDailyVerse().GetAwaiter().GetResult();
                Verse verse = _bgProvider.GetVerse(votdRef, titlesEnabled, verseNumbersEnabled, idealVersion).GetAwaiter().GetResult();

                return new CommandResponse
                {
                    OK = true,
                    Pages = new List<InternalEmbed>
                    {
                        new Utils().Embedify($"{verse.Reference.AsString} - {verse.Reference.Version.Name}", verse.Title, verse.Text, false, null)
                    }
                };
            }
        }

        public class DailyVerseSet : ICommand
        {
            public string Name { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }

            private readonly UserService _userService;
            private readonly GuildService _guildService;
            private readonly VersionService _versionService;

            public DailyVerseSet(UserService userService, GuildService guildService, VersionService versionService)
            {
                Name = "set";
                ExpectedArguments = 0;
                PermissionsRequired = null;

                _userService = userService;
                _guildService = guildService;
                _versionService = versionService;
            }

            public IResponse ProcessCommand(Request req, List<string> args)
            {
                if (args.Count == 2)
                {
                    var timeSplit = args[0].Split(":");

                    try
                    {
                        var hour = int.Parse(timeSplit[0]);
                        var minute = int.Parse(timeSplit[1]);

                        if ((hour > -1 && hour < 24) || (minute > -1 && minute < 60))
                        {
                            var idealGuild = _guildService.Get(req.GuildId);

                            if (idealGuild != null)
                            {
                                idealGuild.DailyVerseTime = args[0];
                                idealGuild.DailyVerseTimeZone = args[1];
                                _guildService.Update(req.GuildId, idealGuild);
                            }
                            else
                            {
                                _guildService.Create(new Guild
                                {
                                    GuildId = req.GuildId,
                                    Version = "RSV",
                                    Language = "english",
                                    Prefix = "+",
                                    IgnoringBrackets = "<>",
                                    IsDM = req.IsDM,
                                    DailyVerseTime = args[0],
                                    DailyVerseTimeZone = args[1]
                                });
                            }

                            return new CommandResponse
                            {
                                OK = true,
                                Pages = new List<InternalEmbed>
                                {
                                    new Utils().Embedify("+dailyverse set", "Set automatic daily verse successfully.", false)
                                },
                                WebhookCallback = true
                            };
                        }
                    }
                    catch
                    {
                        return new CommandResponse
                        {
                            OK = true,
                            Pages = new List<InternalEmbed>
                            {
                                new Utils().Embedify("+dailyverse set", "Go to https://biblebot.github.io/dailyversesetup/ to continue the setup process.", false)
                            }
                        };
                    }
                }

                return new CommandResponse
                {
                    OK = true,
                    Pages = new List<InternalEmbed>
                    {
                        new Utils().Embedify("+dailyverse set", "Go to https://biblebot.github.io/dailyversesetup/ to continue the setup process.", false)
                    }
                };
            }
        }
    }
}