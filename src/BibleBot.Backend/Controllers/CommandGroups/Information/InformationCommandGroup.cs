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

namespace BibleBot.Backend.Controllers.CommandGroups.Information
{
    public class InformationCommandGroup : ICommandGroup
    {
        public string Name { get; set; }
        public bool IsStaffOnly { get; set; }
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
            IsStaffOnly = false;
            Commands =
            [
                new InfoStats(_userService, _guildService, _versionService, _frontendStatsService),
                new InfoBibleBot(),
                new InfoInvite()
            ];
            DefaultCommand = Commands.FirstOrDefault(cmd => cmd.Name == "biblebot");
        }

        public class InfoStats(UserService userService, GuildService guildService, VersionService versionService, FrontendStatsService frontendStatsService) : ICommand
        {
            public string Name { get; set; } = "stats";
            public string ArgumentsError { get; set; } = null;
            public int ExpectedArguments { get; set; } = 0;
            public List<Permissions> PermissionsRequired { get; set; } = null;
            public bool BotAllowed { get; set; } = false; // anti-spam measure

            private readonly UserService _userService = userService;
            private readonly GuildService _guildService = guildService;
            private readonly VersionService _versionService = versionService;
            private readonly FrontendStatsService _frontendStatsService = frontendStatsService;

            public async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                long userPrefs = await _userService.GetCount();
                long guildPrefs = await _guildService.GetCount();
                long versions = await _versionService.GetCount();
                FrontendStats frontendStats = (await _frontendStatsService.Get()).First();

                string version = Utils.Version;

                string commitBaseEndpoint = $"https://gitlab.com/kerygmadigital/biblebot/BibleBot/-/commit";

                string frontendShortHash = frontendStats.FrontendRepoCommitHash.Substring(0, 8);
                string frontendLongHash = frontendStats.FrontendRepoCommitHash;
                string frontendCommitURL = $"{commitBaseEndpoint}/{frontendLongHash}";

                string backendShortHash = ThisAssembly.Git.Commit;
                string backendLongHash = ThisAssembly.Git.Sha;
                string backendCommitURL = $"{commitBaseEndpoint}/{backendLongHash}";

                string resp = $"### Frontend Stats\n" +
                $"**Shard Count**: {frontendStats.ShardCount}\n" +
                $"**Server Count**: {frontendStats.ServerCount}\n" +
                $"**User Count**: {frontendStats.UserCount}\n" +
                $"**Channel Count**: {frontendStats.ChannelCount}\n\n" +
                $"### Backend Stats (estimated)\n" +
                $"**User Preference Count**: {userPrefs}\n" +
                $"**Guild Preference Count**: {guildPrefs}\n" +
                $"**Version Count**: {versions}\n\n" +
                $"*For full statistics on our server count and preferences, see <https://biblebot.xyz/stats>.*\n\n" +
                $"### Metadata\n" +
                $"**Frontend**: v{version} ([{frontendShortHash}]({frontendCommitURL}))\n" +
                $"**Backend**: v{version} ([{backendShortHash}]({backendCommitURL}))\n";


                return new CommandResponse
                {
                    OK = true,
                    Pages =
                    [
                        Utils.GetInstance().Embedify("/stats", resp, false)
                    ],
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

            public InfoBibleBot()
            {
                Name = "biblebot";
                ArgumentsError = null;
                ExpectedArguments = 0;
                PermissionsRequired = null;
                BotAllowed = true;
            }

            public Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                InternalEmbed embed = new()
                {
                    Title = $"BibleBot v{Utils.Version}",
                    Description = "Scripture from your Discord client to your heart.",
                    Color = 6709986,
                    Footer = new Footer
                    {
                        Text = "Made with ❤️ by Kerygma Digital",
                        IconURL = "https://i.imgur.com/hr4RXpy.png"
                    },
                    Fields =
                    [
                        new()
                        {
                            Name = ":tools: Commands",
                            Value = "`/search` - search for verses by keyword\n" +
                            "`/version` - version preferences and information\n" +
                            "`/formatting` - preferences for verse styles and bot behavior\n" +
                            "`/dailyverse` - daily verses and automation\n" +
                            "`/random` - get a random Bible verse\n" +
                            "`/resource` - creeds, catechisms, confessions, and historical documents\n" +
                            "`/stats` - view bot statistics\n" +
                            "`/invite` - get the invite link for BibleBot\n\n" +
                            "Look inside these commands for more, or check out our [documentation](https://biblebot.xyz/usage-and-commands/).\n\n" +
                            "─────────────",
                            Inline = false
                        },
                        new()
                        {
                            Name = ":link: Links",
                            Value = "**Website**: https://biblebot.xyz\n" +
                            "**Copyrights**: https://biblebot.xyz/copyright\n" +
                            "**Source Code**: https://gitlab.com/KerygmaDigital/BibleBot/BibleBot\n" +
                            "**Official Discord Server**: https://biblebot.xyz/discord\n" +
                            "**Terms and Conditions**: https://biblebot.xyz/terms\n\n" +
                            "─────────────"
                        },
                        new()
                        {
                            Name = ":newspaper: News",
                            Value = ":new: **26 July 2023** - [The NKJV has been restored for use...](https://discord.com/channels/362503610006765568/440313404427730945/1133504754031546380) :new:\n" +
                            ":new: **21 July 2023** - [Update: v9.2-beta (build 559)](https://biblebot.xyz/2023/07/21/update-v9-2-beta-build-559/) :new:\n" +
                            "**21 June 2023** - [Update: v9.2-beta (build 532)](https://biblebot.xyz/2023/06/21/release-v9-2-beta-build-532/)\n" +
                            "**31 March 2022** - [Release: v9.2-beta](https://biblebot.xyz/2022/03/31/release-v9-2-beta/)",
                            Inline = false,
                        }
                    ]
                };


                return Task.FromResult<IResponse>(new CommandResponse
                {
                    OK = true,
                    Pages =
                    [
                        embed
                    ],
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

            public Task<IResponse> ProcessCommand(Request req, List<string> args) => Task.FromResult<IResponse>(new CommandResponse
            {
                OK = true,
                Pages =
                    [
                        Utils.GetInstance().Embedify("/invite", "To invite the bot to your server, click [here](https://biblebot.xyz/invite).\nTo join the official Discord server, click [here](https://biblebot.xyz/discord).\n\nFor information on the permissions we request, click [here](https://biblebot.xyz/permissions/).", false)
                    ],
                LogStatement = "/invite"
            });
        }
    }
}
