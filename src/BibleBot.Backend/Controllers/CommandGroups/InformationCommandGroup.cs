/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using BibleBot.Backend.Services;
using BibleBot.Models;
using Microsoft.Extensions.Localization;
using Sentry;
using Serilog;

namespace BibleBot.Backend.Controllers.CommandGroups
{
    public class InformationCommandGroup(UserService userService, GuildService guildService, VersionService versionService,
                                         FrontendStatsService frontendStatsService, IStringLocalizerFactory localizerFactory) : CommandGroup
    {
        private readonly IStringLocalizer _localizer = localizerFactory.Create(typeof(InformationCommandGroup));

        public override string Name { get => "info"; set => throw new NotImplementedException(); }
        public override Command DefaultCommand { get => Commands.FirstOrDefault(cmd => cmd.Name == "biblebot"); set => throw new NotImplementedException(); }
        public override List<Command> Commands
        {
            get => [
                new InfoStats(userService, guildService, versionService, frontendStatsService, _localizer),
                new InfoBibleBot(_localizer),
                new InfoInvite(_localizer)
            ]; set => throw new NotImplementedException();
        }

        private class InfoStats(UserService userService, GuildService guildService, VersionService versionService, FrontendStatsService frontendStatsService, IStringLocalizer localizer) : Command
        {
            public override string Name { get => "stats"; set => throw new NotImplementedException(); }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                long userPrefs = await userService.GetCount();
                long guildPrefs = await guildService.GetCount();
                long versions = await versionService.GetCount();
                FrontendStats frontendStats = (await frontendStatsService.Get()).First();

                string version = Utils.Version;

                const string commitBaseEndpoint = $"https://gitlab.com/kerygmadigital/biblebot/BibleBot/-/commit";

                string frontendShortHash = frontendStats.FrontendRepoCommitHash.Substring(0, 8);
                string frontendLongHash = frontendStats.FrontendRepoCommitHash;
                string frontendCommitURL = $"{commitBaseEndpoint}/{frontendLongHash}";

#pragma warning disable CS0618 // Type or member is obsolete
                // The pragma exists because ThisAssembly devs are assholes who force
                // a sponsorship in exchange for the library being navigable in an IDE.
                // The library still works.
                string backendShortHash = ThisAssembly.Git.Commit;
                string backendLongHash = ThisAssembly.Git.Sha;
#pragma warning restore CS0618 // Type or member is obsolete

                string backendCommitURL = $"{commitBaseEndpoint}/{backendLongHash}";

                string resp = $"### {localizer["FrontendStats"]}\n" +
                $"**{localizer["ShardCount"]}**: {frontendStats.ShardCount}\n" +
                $"**{localizer["ServerCount"]}**: {frontendStats.ServerCount}\n" +
                $"**{localizer["UserCount"]}**: {frontendStats.UserCount}\n" +
                $"**{localizer["ChannelCount"]}**: {frontendStats.ChannelCount}\n\n" +
                $"### {localizer["BackendStats"]}\n" +
                $"**{localizer["UserPreferenceCount"]}**: {userPrefs}\n" +
                $"**{localizer["GuildPreferenceCount"]}**: {guildPrefs}\n" +
                $"**{localizer["VersionCount"]}**: {versions}\n\n" +
                $"*{localizer["StatisticsSheetAddenda"]}*\n\n" +
                $"### {localizer["Metadata"]}\n" +
                $"**{localizer["Frontend"]}**: {version} ([{frontendShortHash}]({frontendCommitURL}))\n" +
                $"**{localizer["Backend"]}**: {version} ([{backendShortHash}]({backendCommitURL}))\n";


                return new CommandResponse
                {
                    OK = true,
                    Pages =
                    [
                        Utils.GetInstance().Embedify("/stats", resp, false)
                    ],
                    LogStatement = "/stats",
                    Culture = CultureInfo.CurrentUICulture.Name
                };
            }
        }

        private class InfoBibleBot(IStringLocalizer localizer) : Command
        {
            public override string Name { get => "biblebot"; set => throw new NotImplementedException(); }

            public override Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                StringBuilder newsSb = new();

                try
                {
                    XmlReader reader = XmlReader.Create("https://biblebot.xyz/feed/");
                    SyndicationFeed blogRssFeed = SyndicationFeed.Load(reader);
                    SyndicationItem[] firstFourEntries = [.. blogRssFeed.Items.Take(4)];

                    for (int i = 0; i < firstFourEntries.Length; i++)
                    {
                        SyndicationItem entry = firstFourEntries[i];
                        if (i < 2)
                        {
                            newsSb.AppendLine($":new: **{entry.PublishDate.ToString("d MMMM yyy")}** - [{entry.Title.Text}]({entry.Links.FirstOrDefault()!.GetAbsoluteUri().ToString()}) :new:");
                        }
                        else
                        {
                            newsSb.AppendLine($"**{entry.PublishDate.ToString("d MMMM yyy")}** - [{entry.Title.Text}]({entry.Links.FirstOrDefault()!.GetAbsoluteUri().ToString()})");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"InformationCommandGroup: Encountered {ex.GetType()} in parsing blog RSS.");
                    SentrySdk.CaptureException(ex);

                    newsSb = new StringBuilder().AppendLine("Unable to parse RSS feed, visit https://biblebot.xyz/blog.");
                }

                InternalEmbed embed = new()
                {
                    Title = $"BibleBot {Utils.Version}",
                    Description = localizer["BibleBotSlogan"],
                    Color = 6709986,
                    Footer = new Footer
                    {
                        Text = localizer["BibleBotCommandFooter"],
                        IconURL = "https://i.imgur.com/hr4RXpy.png"
                    },
                    Fields =
                    [
                        new EmbedField
                        {
                            Name = $":tools: {localizer["Commands"]}",
                            Value = $"`/search` - {localizer["SearchCommandDescription"]}\n" +
                            $"`/version` - {localizer["VersionCommandDescription"]}\n" +
                            $"`/language` - {localizer["LanguageCommandDescription"]}\n" +
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
                        new EmbedField
                        {
                            Name = $":link: {localizer["Links"]}",
                            Value = $"**{localizer["Website"]}**: https://biblebot.xyz\n" +
                            $"**{localizer["Copyrights"]}**: https://biblebot.xyz/copyright\n" +
                            $"**{localizer["SourceCode"]}**: https://gitlab.com/KerygmaDigital/BibleBot/BibleBot\n" +
                            $"**{localizer["OfficialDiscordServer"]}**: https://biblebot.xyz/discord\n" +
                            $"**{localizer["TermsAndConditions"]}**: https://biblebot.xyz/terms\n\n" +
                            "─────────────"
                        },
                        new EmbedField
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
                    LogStatement = "/biblebot",
                    Culture = CultureInfo.CurrentUICulture.Name
                });
            }
        }

        private class InfoInvite(IStringLocalizer localizer) : Command
        {
            public override string Name { get => "invite"; set => throw new NotImplementedException(); }

            public override Task<IResponse> ProcessCommand(Request req, List<string> args) => Task.FromResult<IResponse>(new CommandResponse
            {
                OK = true,
                Pages =
                    [
                        Utils.GetInstance().Embedify("/invite", localizer["InviteResp"], false)
                    ],
                LogStatement = "/invite",
                Culture = CultureInfo.CurrentUICulture.Name
            });
        }
    }
}
