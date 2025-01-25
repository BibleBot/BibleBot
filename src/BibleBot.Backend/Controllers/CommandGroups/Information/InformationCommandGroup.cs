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
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using BibleBot.Backend.Services;
using BibleBot.Models;
using Microsoft.Extensions.Localization;

namespace BibleBot.Backend.Controllers.CommandGroups.Information
{
    public class InformationCommandGroup(UserService userService, GuildService guildService, VersionService versionService,
                                         FrontendStatsService frontendStatsService, IStringLocalizer<InformationCommandGroup> localizer) : CommandGroup
    {
        public override string Name { get => "info"; set => throw new NotImplementedException(); }
        public override Command DefaultCommand { get => Commands.FirstOrDefault(cmd => cmd.Name == "biblebot"); set => throw new NotImplementedException(); }
        public override List<Command> Commands
        {
            get => [
                new InfoStats(userService, guildService, versionService, frontendStatsService, localizer),
                new InfoBibleBot(localizer),
                new InfoInvite(localizer)
            ]; set => throw new NotImplementedException();
        }

        public class InfoStats(UserService userService, GuildService guildService, VersionService versionService, FrontendStatsService frontendStatsService, IStringLocalizer<InformationCommandGroup> localizer) : Command
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

                string resp = $"{localizer["FrontendStats"]}\n" +
                $"{localizer["ShardCount"]}: {frontendStats.ShardCount}\n" +
                $"{localizer["ServerCount"]}: {frontendStats.ServerCount}\n" +
                $"{localizer["UserCount"]}: {frontendStats.UserCount}\n" +
                $"{localizer["ChannelCount"]}: {frontendStats.ChannelCount}\n\n" +
                $"{localizer["BackendStats"]}\n" +
                $"{localizer["UserPreferenceCount"]}: {userPrefs}\n" +
                $"{localizer["GuildPreferenceCount"]}: {guildPrefs}\n" +
                $"{localizer["VersionCount"]}: {versions}\n\n" +
                $"*{localizer["StatisticsSheetAddenda"]}*\n\n" +
                $"{localizer["Metadata"]}\n" +
                $"{localizer["Frontend"]}: v{version} ([{frontendShortHash}]({frontendCommitURL}))\n" +
                $"{localizer["Backend"]}: v{version} ([{backendShortHash}]({backendCommitURL}))\n";


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

        public class InfoBibleBot(IStringLocalizer<InformationCommandGroup> localizer) : Command
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
                    Description = localizer["BibleBotSlogan"],
                    Color = 6709986,
                    Footer = new Footer
                    {
                        Text = localizer["BibleBotCommandFooter"],
                        IconURL = "https://i.imgur.com/hr4RXpy.png"
                    },
                    Fields =
                    [
                        new()
                        {
                            Name = $":tools: {localizer["Commands"]}",
                            Value = $"`/search` - {localizer["SearchCommandDescription"]}\n" +
                            $"`/version` - {localizer["VersionCommandDescription"]}\n" +
                            $"`/formatting` - {localizer["FormattingCommandDescription"]}\n" +
                            $"`/dailyverse` - {localizer["DailyVerseCommandDescription"]}\n" +
                            $"`/random` - {localizer["RandomCommandDescription"]}\n" +
                            $"`/resource` - {localizer["ResourceCommandDescription"]}\n" +
                            $"`/stats` - {localizer["StatsCommandDescription"]}\n" +
                            $"`/invite` - {localizer["InviteCommandDescription"]}\n\n" +
                            $"{localizer["CommandsAddenda"]}\n\n" +
                            "─────────────",
                            Inline = false
                        },
                        new()
                        {
                            Name = $":link: {localizer["Links"]}",
                            Value = $"{localizer["Website"]}: https://biblebot.xyz\n" +
                            $"{localizer["Copyrights"]}: https://biblebot.xyz/copyright\n" +
                            $"{localizer["SourceCode"]}: https://gitlab.com/KerygmaDigital/BibleBot/BibleBot\n" +
                            $"{localizer["OfficialDiscordServer"]}: https://biblebot.xyz/discord\n" +
                            $"{localizer["TermsAndConditions"]}: https://biblebot.xyz/terms\n\n" +
                            "─────────────"
                        },
                        new()
                        {
                            Name = $":newspaper: {localizer["NewsFromBlog"]}",
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

        public class InfoInvite(IStringLocalizer<InformationCommandGroup> localizer) : Command
        {
            public override string Name { get => "invite"; set => throw new NotImplementedException(); }

            public override Task<IResponse> ProcessCommand(Request req, List<string> args) => Task.FromResult<IResponse>(new CommandResponse
            {
                OK = true,
                Pages =
                    [
                        Utils.GetInstance().Embedify("/invite", localizer["InviteResp"], false)
                    ],
                LogStatement = "/invite"
            });
        }
    }
}
