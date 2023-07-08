/*
* Copyright (C) 2016-2023 Kerygma Digital Co.
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

            public Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                var userPrefs = _userService.GetCount();
                var guildPrefs = _guildService.GetCount();
                var versions = _versionService.GetCount();
                var frontendStats = _frontendStatsService.Get();

                var version = Utils.Version;

                var commitBaseEndpoint = $"https://gitlab.com/kerygmadigital/biblebot/BibleBot/-/commit";

                var frontendShortHash = frontendStats.FrontendRepoCommitHash.Substring(0, 8);
                var frontendLongHash = frontendStats.FrontendRepoCommitHash;
                var frontendCommitURL = $"{commitBaseEndpoint}/{frontendLongHash}";

                var backendShortHash = ThisAssembly.Git.Commit;
                var backendLongHash = ThisAssembly.Git.Sha;
                var backendCommitURL = $"{commitBaseEndpoint}/{backendLongHash}";

                var resp = $"**__Frontend Stats__**\n" +
                $"**Shard Count**: {frontendStats.ShardCount}\n" +
                $"**Server Count**: {frontendStats.ServerCount}\n" +
                $"**User Count**: {frontendStats.UserCount}\n" +
                $"**Channel Count**: {frontendStats.ChannelCount}\n\n" +
                $"**__Backend Stats (estimated)__**\n" +
                $"**User Preference Count**: {userPrefs}\n" +
                $"**Guild Preference Count**: {guildPrefs}\n" +
                $"**Version Count**: {versions}\n\n" +
                $"**__Metadata__**\n" +
                $"**Frontend**: v{version} ([{frontendShortHash}]({frontendCommitURL}))\n" +
                $"**Backend**: v{version} ([{backendShortHash}]({backendCommitURL}))\n";


                return Task.FromResult<IResponse>(new CommandResponse
                {
                    OK = true,
                    Pages = new List<InternalEmbed>
                    {
                        new Utils().Embedify("/stats", resp, false)
                    },
                    LogStatement = "/stats"
                });
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

            public Task<IResponse> ProcessCommand(Request req, List<string> args)
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
                            "**Source Code**: https://gitlab.com/KerygmaDigital/BibleBot/BibleBot\n" +
                            "**Official Discord Server**: https://biblebot.xyz/discord\n" +
                            "**Terms and Conditions**: https://biblebot.xyz/terms\n\n─────────────"
                        },
                        new EmbedField
                        {
                            Name = "🔔 News",
                            Value = "**21 June 2023** - [Update: v9.2-beta (build 532)](https://biblebot.xyz/2023/06/21/release-v9-2-beta-build-532/)\n" +
                            "**31 March 2022** - [Release: v9.2-beta](https://biblebot.xyz/2022/03/31/release-v9-2-beta/)\n" +
                            "**14 March 2022** - [Update: v9.1-beta (build 138)](https://biblebot.xyz/2022/03/14/update-v9-1-beta-build-138/)\n" +
                            "**15 October 2021** - [Update: v9.1-beta (build 119)](https://biblebot.xyz/2021/10/15/update-v9-1-beta-build-119/)",
                            Inline = false,
                        }
                    }
                };


                return Task.FromResult<IResponse>(new CommandResponse
                {
                    OK = true,
                    Pages = new List<InternalEmbed>
                    {
                        embed
                    },
                    LogStatement = "/biblebot"
                });
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

            public Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                return Task.FromResult<IResponse>(new CommandResponse
                {
                    OK = true,
                    Pages = new List<InternalEmbed>
                    {
                        new Utils().Embedify("/invite", "To invite the bot to your server, click [here](https://biblebot.xyz/invite).\nTo join the official Discord server, click [here](https://biblebot.xyz/discord).\n\nFor information on the permissions we request, click [here](https://biblebot.xyz/permissions/).", false)
                    },
                    LogStatement = "/invite"
                });
            }
        }
    }
}
