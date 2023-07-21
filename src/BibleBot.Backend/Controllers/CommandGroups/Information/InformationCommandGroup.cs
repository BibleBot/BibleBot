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

            public async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                var userPrefs = await _userService.GetCount();
                var guildPrefs = await _guildService.GetCount();
                var versions = await _versionService.GetCount();
                var frontendStats = await _frontendStatsService.Get();

                var version = Utils.Version;

                var commitBaseEndpoint = $"https://gitlab.com/kerygmadigital/biblebot/BibleBot/-/commit";

                var frontendShortHash = frontendStats.FrontendRepoCommitHash.Substring(0, 8);
                var frontendLongHash = frontendStats.FrontendRepoCommitHash;
                var frontendCommitURL = $"{commitBaseEndpoint}/{frontendLongHash}";

                var backendShortHash = ThisAssembly.Git.Commit;
                var backendLongHash = ThisAssembly.Git.Sha;
                var backendCommitURL = $"{commitBaseEndpoint}/{backendLongHash}";

                var resp = $"### Frontend Stats\n" +
                $"**Shard Count**: {frontendStats.ShardCount}\n" +
                $"**Server Count**: {frontendStats.ServerCount}\n" +
                $"**User Count**: {frontendStats.UserCount}\n" +
                $"**Channel Count**: {frontendStats.ChannelCount}\n\n" +
                $"### Backend Stats (estimated)\n" +
                $"**User Preference Count**: {userPrefs}\n" +
                $"**Guild Preference Count**: {guildPrefs}\n" +
                $"**Version Count**: {versions}\n\n" +
                $"### Metadata\n" +
                $"**Frontend**: v{version} ([{frontendShortHash}]({frontendCommitURL}))\n" +
                $"**Backend**: v{version} ([{backendShortHash}]({backendCommitURL}))\n";


                return new CommandResponse
                {
                    OK = true,
                    Pages = new List<InternalEmbed>
                    {
                        Utils.GetInstance().Embedify("/stats", resp, false)
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

            public Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                var embed = new InternalEmbed
                {
                    Title = $"<:biblebot:800682801925980260> BibleBot v{Utils.Version}",
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
                            Name = "<:slashcommand:1132081589313081364> Commands",
                            Value = "`/search` - search for verses by keyword\n" +
                            "`/version` - version preferences and information\n" +
                            "`/formatting` - preferences for verse styles and bot behavior\n" +
                            "`/dailyverse` - daily verses and automation\n" +
                            "`/random` - get a random Bible verse\n" +
                            "`/resource` - creeds, catechisms, confessions, and historical documents\n" +
                            "`/stats` - view bot statistics\n" +
                            "`/invite` - get the invite link for BibleBot\n\n" +
                            "Look inside these commands for more, or check out our [documentation](https://biblebot.xyz/usage-and-commands/)\n\n" +
                            "─────────────",
                            Inline = false
                        },
                        new EmbedField
                        {
                            Name = ":link: Links",
                            Value = "**Website**: https://biblebot.xyz\n" +
                            "**Copyrights**: https://biblebot.xyz/copyright\n" +
                            "**Source Code**: https://gitlab.com/KerygmaDigital/BibleBot/BibleBot\n" +
                            "**Official Discord Server**: https://biblebot.xyz/discord\n" +
                            "**Terms and Conditions**: https://biblebot.xyz/terms\n\n" +
                            "─────────────"
                        },
                        new EmbedField
                        {
                            Name = "<:news:1132081053251686552> News",
                            Value = "<:newbadge:1132080714343526521> **21 July 2023** - [Update: v9.2-beta (build 559)](https://biblebot.xyz/2023/07/21/update-v9-2-beta-build-559/) <:newbadge:1132080714343526521>\n" +
                            "**21 June 2023** - [Update: v9.2-beta (build 532)](https://biblebot.xyz/2023/06/21/release-v9-2-beta-build-532/)\n" +
                            "**31 March 2022** - [Release: v9.2-beta](https://biblebot.xyz/2022/03/31/release-v9-2-beta/)\n" +
                            "**14 March 2022** - [Update: v9.1-beta (build 138)](https://biblebot.xyz/2022/03/14/update-v9-1-beta-build-138/)",
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
                        Utils.GetInstance().Embedify("/invite", "To invite the bot to your server, click [here](https://biblebot.xyz/invite).\nTo join the official Discord server, click [here](https://biblebot.xyz/discord).\n\nFor information on the permissions we request, click [here](https://biblebot.xyz/permissions/).", false)
                    },
                    LogStatement = "/invite"
                });
            }
        }
    }
}
