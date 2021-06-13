using System.Linq;
using System.Globalization;
using System.Collections.Generic;

using System.Reflection;

using BibleBot.Lib;
using BibleBot.Backend.Models;
using BibleBot.Backend.Services;
using BibleBot.Backend.Services.Providers;

namespace BibleBot.Backend.Controllers.CommandGroups.Information
{
    public class InformationCommandGroup : ICommandGroup
    {
        public string Name { get; set; }
        public bool IsOwnerOnly { get; set; }
        public ICommand DefaultCommand { get; set; }
        public List<ICommand> Commands { get; set; }

        private readonly UserService _userService;
        private readonly GuildService _guildService;
        private readonly VersionService _versionService;
        private readonly FrontendStatsService _frontendStatsService;

        public InformationCommandGroup(UserService userService, GuildService guildService, VersionService versionService, FrontendStatsService frontendStatsService)
        {
            _userService = userService;
            _guildService = guildService;
            _versionService = versionService;
            _frontendStatsService = frontendStatsService;

            Name = "info";
            IsOwnerOnly = false;
            Commands = new List<ICommand>
            {
                new InfoStats(_userService, _guildService, _versionService, _frontendStatsService),
                new InfoBibleBot(_userService, _guildService),
                new InfoInvite()
            };
            DefaultCommand = Commands.Where(cmd => cmd.Name == "biblebot").FirstOrDefault();
        }

        public class InfoStats : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }

            private readonly UserService _userService;
            private readonly GuildService _guildService;
            private readonly VersionService _versionService;
            private readonly FrontendStatsService _frontendStatsService;

            public InfoStats(UserService userService, GuildService guildService, VersionService versionService, FrontendStatsService frontendStatsService)
            {
                Name = "stats";
                ArgumentsError = null;
                ExpectedArguments = 0;
                PermissionsRequired = null;

                _userService = userService;
                _guildService = guildService;
                _versionService = versionService;
                _frontendStatsService = frontendStatsService;
            }

            public IResponse ProcessCommand(Request req, List<string> args)
            {
                var userPrefs = _userService.Get();
                var guildPrefs = _guildService.Get();
                var versions = _versionService.Get();
                var frontendStats = _frontendStatsService.Get();

                var resp = $"**__Frontend Stats__**\n" +
                $"**Shard Count**: {frontendStats.ShardCount}\n" +
                $"**Server Count**: {frontendStats.ServerCount}\n" +
                $"**Channel Count**: {frontendStats.ChannelCount}\n\n" +
                $"**__Backend Stats__**\n" +
                $"**User Preference Count**: {userPrefs.Count}\n" +
                $"**Guild Preference Count**: {guildPrefs.Count}\n" +
                $"**Version Count**: {versions.Count}\n\n" +
                $"**__Metadata__**\n" +
                $"**BibleBot**: v9.1 ([{ThisAssembly.Git.Commit}](https://github.com/BibleBot/BibleBot/commit/{ThisAssembly.Git.Sha}))\n" +
                $"**BibleBot.Lib**: v{typeof(ThisAssembly).Assembly.GetReferencedAssemblies().Where((asm) => asm.Name == "BibleBot.Lib").First().Version.ToString(3)}";


                return new CommandResponse
                {
                    OK = true,
                    Pages = new List<InternalEmbed>
                    {
                        new Utils().Embedify("+stats", resp, false)
                    },
                    LogStatement = "+stats"
                };
            }
        }

        public class InfoBibleBot : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }

            private readonly UserService _userService;
            private readonly GuildService _guildService;

            public InfoBibleBot(UserService userService, GuildService guildService)
            {
                Name = "biblebot";
                ArgumentsError = null;
                ExpectedArguments = 0;
                PermissionsRequired = null;

                _userService = userService;
                _guildService = guildService;
            }

            public IResponse ProcessCommand(Request req, List<string> args)
            {
                var embed = new InternalEmbed
                {
                    Title = "BibleBot v9.1",
                    Description = "The premier Discord bot for Christians.",
                    Color = 6709986,
                    Footer = new Footer
                    {
                        Text = "Made with ❤️ by Kerygma Digital",
                        IconURL = "https://i.imgur.com/hr4RXpy.png"
                    },
                    Fields = new List<EmbedField>
                    {
                        new EmbedField
                        {
                            Name = "📖 Commands",
                            Value = "`+search` - search for verses by keyword\n" +
                            "`+version` - version preferences and information\n" +
                            "`+formatting` - preferences for verse styles and bot behavior\n" +
                            "`+dailyverse` - daily verses and automation\n" +
                            "`+random` - get a random Bible verse\n" +
                            "`+resource` - creeds, catechisms, confessions, and historical documents\n" +
                            "`+stats` - view bot statistics\n" +
                            "`+invite` - get the invite link for BibleBot\n\n─────────────",
                            Inline = false
                        },
                        new EmbedField
                        {
                            Name = "🔗 Links",
                            Value = "**Website**: https://biblebot.xyz\n" +
                            "**Copyrights**: https://biblebot.xyz/copyright\n" +
                            "**Source Code**: https://github.com/BibleBot/BibleBot\n" +
                            "**Official Discord Server**: https://biblebot.xyz/discord\n" +
                            "**Terms and Conditions**: https://biblebot.xyz/terms\n\n─────────────"
                        },
                        new EmbedField
                        {
                            Name = "🔔 News",
                            Value = "**June 4th** - [v9.1-beta has been released! Read more about the changes.](https://biblebot.xyz/2021/06/04/release-v9-1-beta)",
                            Inline = false,
                        }
                    }
                };


                return new CommandResponse
                {
                    OK = true,
                    Pages = new List<InternalEmbed>
                    {
                        embed
                    },
                    LogStatement = "+biblebot"
                };
            }
        }

        public class InfoInvite : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }

            public InfoInvite()
            {
                Name = "invite";
                ArgumentsError = null;
                ExpectedArguments = 0;
                PermissionsRequired = null;
            }

            public IResponse ProcessCommand(Request req, List<string> args)
            {
                return new CommandResponse
                {
                    OK = true,
                    Pages = new List<InternalEmbed>
                    {
                        new Utils().Embedify("+invite", "To invite the bot to your server, click [here](https://biblebot.xyz/invite).\nTo join the official Discord server, click [here](https://biblebot.xyz/discord).\n\nFor information on the permissions we request, click [here](https://biblebot.xyz/permissions/).", false)
                    },
                    LogStatement = "+invite"
                };
            }
        }

        public class InfoStewards : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }

            public InfoStewards()
            {
                Name = "stewards";
                ArgumentsError = null;
                ExpectedArguments = 0;
                PermissionsRequired = null;
            }

            public IResponse ProcessCommand(Request req, List<string> args)
            {
                return new CommandResponse
                {
                    OK = true,
                    Pages = new List<InternalEmbed>
                    {
                        new Utils().Embedify("+stewards", "", false)
                    },
                    LogStatement = "+stewards"
                };
            }
        }
    }
}