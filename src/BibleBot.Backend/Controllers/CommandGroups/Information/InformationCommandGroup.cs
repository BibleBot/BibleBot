/*
* Copyright (C) 2016-2021 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using BibleBot.Backend.Models;
using BibleBot.Backend.Services;
using BibleBot.Backend.Services.Providers;
using BibleBot.Lib;

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
            public bool BotAllowed { get; set; }

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
                BotAllowed = false; // anti-spam measure

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
                $"**User Count**: {frontendStats.UserCount}\n" +
                $"**Channel Count**: {frontendStats.ChannelCount}\n\n" +
                $"**__Backend Stats__**\n" +
                $"**User Preference Count**: {userPrefs.Count}\n" +
                $"**Guild Preference Count**: {guildPrefs.Count}\n" +
                $"**Version Count**: {versions.Count}\n\n" +
                $"**__Metadata__**\n" +
                $"**BibleBot**: v{Utils.Version} ([{ThisAssembly.Git.Commit}](https://github.com/BibleBot/BibleBot/commit/{ThisAssembly.Git.Sha}))\n" +
                $"**BibleBot.Lib**: v{typeof(ThisAssembly).Assembly.GetReferencedAssemblies().Where((asm) => asm.Name == "BibleBot.Lib").First().Version.ToString(3)}";


                return new CommandResponse
                {
                    OK = true,
                    Pages = new List<InternalEmbed>
                    {
                        new Utils().Embedify("/stats", resp, false)
                    },
                    LogStatement = "/stats"
                };
            }
        }

        public class InfoBibleBot : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }
            public bool BotAllowed { get; set; }

            private readonly UserService _userService;
            private readonly GuildService _guildService;

            public InfoBibleBot(UserService userService, GuildService guildService)
            {
                Name = "biblebot";
                ArgumentsError = null;
                ExpectedArguments = 0;
                PermissionsRequired = null;
                BotAllowed = true;

                _userService = userService;
                _guildService = guildService;
            }

            public IResponse ProcessCommand(Request req, List<string> args)
            {
                var embed = new InternalEmbed
                {
                    Title = $"BibleBot v{Utils.Version}",
                    Description = "Scripture from your Discord client to your heart.",
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
                            Value = "`/search` - search for verses by keyword\n" +
                            "`/version` - version preferences and information\n" +
                            "`/formatting` - preferences for verse styles and bot behavior\n" +
                            "`/dailyverse` - daily verses and automation\n" +
                            "`/random` - get a random Bible verse\n" +
                            "`/resource` - creeds, catechisms, confessions, and historical documents\n" +
                            "`/stats` - view bot statistics\n" +
                            "`/invite` - get the invite link for BibleBot\n\n─────────────",
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
                            Value = "**June 26th** - [Update: v9.1-beta (build 74)](https://biblebot.xyz/2021/06/26/update-v9-1-beta-74)\n" +
                            "**June 17th** - [Update: v9.1-beta (build 15)](https://biblebot.xyz/2021/06/17/update-v9-1-beta-15/)\n" +
                            "**June 4th** - [Release: v9.1-beta](https://biblebot.xyz/2021/06/04/release-v9-1-beta)",
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
                    LogStatement = "/biblebot"
                };
            }
        }

        public class InfoInvite : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }
            public bool BotAllowed { get; set; }

            public InfoInvite()
            {
                Name = "invite";
                ArgumentsError = null;
                ExpectedArguments = 0;
                PermissionsRequired = null;
                BotAllowed = true;
            }

            public IResponse ProcessCommand(Request req, List<string> args)
            {
                return new CommandResponse
                {
                    OK = true,
                    Pages = new List<InternalEmbed>
                    {
                        new Utils().Embedify("/invite", "To invite the bot to your server, click [here](https://biblebot.xyz/invite).\nTo join the official Discord server, click [here](https://biblebot.xyz/discord).\n\nFor information on the permissions we request, click [here](https://biblebot.xyz/permissions/).", false)
                    },
                    LogStatement = "/invite"
                };
            }
        }
    }
}
