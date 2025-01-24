/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using BibleBot.Backend.Services;
using BibleBot.Models;

namespace BibleBot.Backend.Controllers.CommandGroups.Information
{
    public class InformationCommandGroup(UserService userService, GuildService guildService, VersionService versionService, FrontendStatsService frontendStatsService) : CommandGroup
    {
        public override string Name { get => "info"; set => throw new NotImplementedException(); }
        public override Command DefaultCommand { get => Commands.FirstOrDefault(cmd => cmd.Name == "biblebot"); set => throw new NotImplementedException(); }
        public override List<Command> Commands
        {
            get => [
                new InfoStats(userService, guildService, versionService, frontendStatsService),
                new InfoBibleBot(),
                new InfoInvite()
            ]; set => throw new NotImplementedException();
        }

        public class InfoStats(UserService userService, GuildService guildService, VersionService versionService, FrontendStatsService frontendStatsService) : Command
        {
            public override string Name { get => "stats"; set => throw new NotImplementedException(); }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                long userPrefs = await userService.GetCount();
                long guildPrefs = await guildService.GetCount();
                long versions = await versionService.GetCount();
                FrontendStats frontendStats = (await frontendStatsService.Get()).First();

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

        public class InfoBibleBot : Command
        {
            public override string Name { get => "biblebot"; set => throw new NotImplementedException(); }

            public override Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                XmlReader reader = XmlReader.Create("https://biblebot.xyz/feed/");
                SyndicationFeed blogRssFeed = SyndicationFeed.Load(reader);
                SyndicationItem[] firstFourEntries = [.. blogRssFeed.Items.Take(4)];

                StringBuilder newsSb = new();

                for (int i = 0; i < firstFourEntries.Length; i++)
                {
                    SyndicationItem entry = firstFourEntries[i];
                    if (i < 2)
                    {
                        newsSb.AppendLine($":new: **{entry.PublishDate.ToString("d MMMM yyy")}** - [{entry.Title.Text}]({entry.Links.FirstOrDefault().GetAbsoluteUri().ToString()}) :new:");
                    }
                    else
                    {
                        newsSb.AppendLine($"**{entry.PublishDate.ToString("d MMMM yyy")}** - [{entry.Title.Text}]({entry.Links.FirstOrDefault().GetAbsoluteUri().ToString()})");
                    }
                }

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
                            Name = ":newspaper: News from BibleBot's Blog",
                            Value = newsSb.ToString().Trim(),
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

        public class InfoInvite : Command
        {
            public override string Name { get => "invite"; set => throw new NotImplementedException(); }

            public override Task<IResponse> ProcessCommand(Request req, List<string> args) => Task.FromResult<IResponse>(new CommandResponse
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
