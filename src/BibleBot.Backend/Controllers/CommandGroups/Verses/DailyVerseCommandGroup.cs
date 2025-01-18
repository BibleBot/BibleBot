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
using System.Threading.Tasks;
using BibleBot.Backend.Services;
using BibleBot.Backend.Services.Providers;
using BibleBot.Models;
using MongoDB.Driver;
using NodaTime;

namespace BibleBot.Backend.Controllers.CommandGroups.Verses
{
    public class DailyVerseCommandGroup(UserService userService, GuildService guildService, VersionService versionService,
                                        SpecialVerseProvider svProvider, List<IBibleProvider> bibleProviders) : CommandGroup
    {
        public override string Name { get => "dailyverse"; set => throw new NotImplementedException(); }
        public override Command DefaultCommand { get => Commands.FirstOrDefault(cmd => cmd.Name == "usage"); set => throw new NotImplementedException(); }
        public override List<Command> Commands
        {
            get => [
                new DailyVerseUsage(userService, guildService, versionService, svProvider, bibleProviders),
                new DailyVerseSet(guildService),
                new DailyVerseRole(guildService),
                new DailyVerseStatus(guildService),
                new DailyVerseClear(guildService)
            ]; set => throw new NotImplementedException();
        }

        public class DailyVerseUsage(UserService userService, GuildService guildService, VersionService versionService,
                               SpecialVerseProvider svProvider, List<IBibleProvider> bibleProviders) : Command
        {
            public override string Name { get => "usage"; set => throw new NotImplementedException(); }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                User idealUser = await userService.Get(req.UserId);
                Guild idealGuild = await guildService.Get(req.GuildId);

                string version = "RSV";
                bool verseNumbersEnabled = true;
                bool titlesEnabled = true;
                string displayStyle = "embed";

                if (idealUser != null && !req.IsBot)
                {
                    version = idealUser.Version;
                    verseNumbersEnabled = idealUser.VerseNumbersEnabled;
                    titlesEnabled = idealUser.TitlesEnabled;
                    displayStyle = idealUser.DisplayStyle;
                }
                else if (idealGuild != null)
                {
                    version = idealGuild.Version;
                    displayStyle = idealGuild.DisplayStyle ?? displayStyle;
                }

                Models.Version idealVersion = await versionService.Get(version) ?? await versionService.Get("RSV");
                string votdRef = await svProvider.GetDailyVerse();
                IBibleProvider provider = bibleProviders.FirstOrDefault(pv => pv.Name == idealVersion.Source) ?? throw new ProviderNotFoundException($"Couldn't find provider for '{votdRef} {idealVersion.Abbreviation}'");

                return new VerseResponse
                {
                    OK = true,
                    Verses =
                    [
                        await provider.GetVerse(votdRef, titlesEnabled, verseNumbersEnabled, idealVersion)
                    ],
                    DisplayStyle = displayStyle,
                    LogStatement = "/dailyverse"
                };
            }
        }

        public class DailyVerseSet(GuildService guildService) : Command
        {
            public override string Name { get => "set"; set => throw new NotImplementedException(); }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                if (req.IsDM)
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages =
                        [
                            Utils.GetInstance().Embedify("/dailyverseset", "The automatic daily verse cannot be used in DMs, as DMs do not allow for webhooks.", true)
                        ],
                        LogStatement = "/dailyverseset"
                    };
                }

                if (args.Count == 2)
                {
                    string[] timeSplit = args[0].Split(":");

                    try
                    {
                        int hour = int.Parse(timeSplit[0]);
                        int minute = int.Parse(timeSplit[1]);

                        if (((hour > -1 && hour < 24) || (minute > -1 && minute < 60)) && DateTimeZoneProviders.Tzdb.GetZoneOrNull(args[1]) != null)
                        {
                            Guild idealGuild = await guildService.Get(req.GuildId);

                            if (idealGuild != null)
                            {
                                UpdateDefinition<Guild> update = Builders<Guild>.Update
                                             .Set(guild => guild.DailyVerseTime, args[0])
                                             .Set(guild => guild.DailyVerseTimeZone, args[1])
                                             .Set(guild => guild.DailyVerseChannelId, req.IsThread ? req.ThreadId : req.ChannelId)
                                             .Set(guild => guild.DailyVerseIsThread, req.IsThread)
                                             .Set(guild => guild.DailyVerseLastSentDate, null);

                                await guildService.Update(req.GuildId, update);
                            }
                            else
                            {
                                // You may be inclined to think that this is where we should set
                                // the channel ID the verses will be sent to, but this is actually
                                // handled in the webhook creation process which results in these
                                // variables being set in the preference by WebhooksController.
                                Guild newGuild = new()
                                {
                                    GuildId = req.GuildId,
                                    DailyVerseTime = args[0],
                                    DailyVerseTimeZone = args[1],
                                    DailyVerseChannelId = req.IsThread ? req.ThreadId : req.ChannelId,
                                    DailyVerseIsThread = req.IsThread,
                                    IsDM = req.IsDM
                                };

                                await guildService.Create(newGuild);
                            }

                            // For information on why CreateWebhook and RemoveWebhook can
                            // both be true, see the documentation comment on RemoveWebhook.
                            return new CommandResponse
                            {
                                OK = true,
                                Pages =
                                [
                                    Utils.GetInstance().Embedify("/dailyverseset", "Set automatic daily verse successfully.", false)
                                ],
                                LogStatement = $"/dailyverseset {args[0]} {args[1]}",
                                CreateWebhook = true,
                                RemoveWebhook = true
                            };
                        }
                    }
                    catch
                    {
                        return new CommandResponse
                        {
                            OK = false,
                            Pages =
                            [
                                Utils.GetInstance().Embedify("/dailyverseset", "Go to https://biblebot.xyz/daily-verse-setup/ to continue the setup process.", true)
                            ],
                            LogStatement = "/dailyverseset"
                        };
                    }
                }

                return new CommandResponse
                {
                    OK = false,
                    Pages =
                    [
                        Utils.GetInstance().Embedify("/dailyverseset", "Go to https://biblebot.xyz/daily-verse-setup/ to continue the setup process.", true)
                    ],
                    LogStatement = "/dailyverseset"
                };
            }
        }

        public class DailyVerseRole(GuildService guildService) : Command
        {
            public override string Name { get => "role"; set => throw new NotImplementedException(); }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                if (req.IsDM)
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages =
                        [
                            Utils.GetInstance().Embedify("/dailyverserole", "The automatic daily verse cannot be used in DMs, as DMs do not allow for webhooks.", true)
                        ],
                        LogStatement = "/dailyverserole"
                    };
                }


                Guild idealGuild = await guildService.Get(req.GuildId);

                if (idealGuild != null)
                {
                    if (idealGuild.DailyVerseWebhook != null)
                    {
                        UpdateDefinition<Guild> update = Builders<Guild>.Update
                                     .Set(guild => guild.DailyVerseRoleId, args[0]);

                        await guildService.Update(req.GuildId, update);

                        return new CommandResponse
                        {
                            OK = true,
                            Pages =
                            [
                                Utils.GetInstance().Embedify("/dailyverserole", "Set automatic daily verse role successfully.", false)
                            ],
                            LogStatement = $"/dailyverserole {args[0]}"
                        };
                    }
                }

                return new CommandResponse
                {
                    OK = false,
                    Pages =
                    [
                        Utils.GetInstance().Embedify("/dailyverserole", "This server does not have automatic daily verse setup. Please do so with `/dailyverseset` before running this command.", true)
                    ],
                    LogStatement = $"/dailyverserole {args[0]}"
                };
            }
        }

        public class DailyVerseStatus(GuildService guildService) : Command
        {
            public override string Name { get => "status"; set => throw new NotImplementedException(); }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                Guild idealGuild = await guildService.Get(req.GuildId);

                if (idealGuild != null)
                {
                    if (idealGuild.DailyVerseChannelId != null && idealGuild.DailyVerseTime != null &&
                        idealGuild.DailyVerseTimeZone != null && idealGuild.DailyVerseWebhook != null)
                    {
                        DateTimeZone preferredTimeZone = DateTimeZoneProviders.Tzdb[idealGuild.DailyVerseTimeZone];
                        ZonedDateTime currentTime = SystemClock.Instance.GetCurrentInstant().InZone(preferredTimeZone);

                        int preferredHour = int.Parse(idealGuild.DailyVerseTime.Split(":")[0]);
                        int preferredMinute = int.Parse(idealGuild.DailyVerseTime.Split(":")[1]);

                        bool todaysHourPassed = false;
                        bool todaysMinutePassed = false;

                        if (currentTime.Hour != preferredHour)
                        {
                            if (currentTime.Hour < preferredHour)
                            {
                                currentTime = currentTime.PlusHours(preferredHour - currentTime.Hour);
                            }
                            else if (currentTime.Hour > preferredHour)
                            {
                                todaysHourPassed = true;
                                currentTime = currentTime.Minus(Duration.FromHours(currentTime.Hour - preferredHour));
                            }
                        }

                        if (currentTime.Minute != preferredMinute)
                        {
                            if (currentTime.Minute < preferredMinute)
                            {
                                currentTime = currentTime.PlusMinutes(preferredMinute - currentTime.Minute);
                            }
                            else if (currentTime.Minute > preferredMinute)
                            {
                                todaysMinutePassed = true;
                                currentTime = currentTime.Minus(Duration.FromMinutes(currentTime.Minute - preferredMinute));
                            }
                        }

                        if (todaysHourPassed && todaysMinutePassed)
                        {
                            currentTime = currentTime.Plus(Duration.FromDays(1));
                        }

                        string timeFormatted = currentTime.ToString("h:mm tt", new CultureInfo("en-US"));

                        string mentionClause = idealGuild.DailyVerseRoleId != null ? $" The <@&{idealGuild.DailyVerseRoleId}> role will be notified when daily verses are sent. " : " ";
                        string resp = $"The daily verse will be sent at `{timeFormatted}`, in the **{preferredTimeZone}** time zone, and will be published in <#{idealGuild.DailyVerseChannelId}>.{mentionClause}It will use this server's preferred version, which you can find by using **`/version`**.\n\nUse **`/dailyverseset`** to set a new time or channel.\nUse **`/dailyverserole`** to set a role to be @mention'd with every automatic daily verse.\nUse **`/dailyverseclear`** to clear automatic daily verse settings.";

                        return new CommandResponse
                        {
                            OK = true,
                            Pages =
                            [
                                Utils.GetInstance().Embedify("/dailyversestatus", resp, false)
                            ],
                            LogStatement = "/dailyversestatus"
                        };
                    }
                }

                return new CommandResponse
                {
                    OK = false,
                    Pages =
                    [
                        Utils.GetInstance().Embedify("/dailyversestatus", "The automatic daily verse has not been setup for this server or has been configured incorrectly. Use `/dailyverseset` to setup the automatic daily verse.", true)
                    ],
                    LogStatement = "/dailyversestatus"
                };
            }
        }

        public class DailyVerseClear(GuildService guildService) : Command
        {
            public override string Name { get => "clear"; set => throw new NotImplementedException(); }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                Guild idealGuild = await guildService.Get(req.GuildId);

                if (idealGuild != null)
                {
                    UpdateDefinition<Guild> update = Builders<Guild>.Update
                                 .Set(guild => guild.DailyVerseTime, null)
                                 .Set(guild => guild.DailyVerseTimeZone, null)
                                 .Set(guild => guild.DailyVerseWebhook, null)
                                 .Set(guild => guild.DailyVerseChannelId, null)
                                 .Set(guild => guild.DailyVerseIsThread, false)
                                 .Set(guild => guild.DailyVerseLastSentDate, null)
                                 .Set(guild => guild.DailyVerseRoleId, null);

                    await guildService.Update(req.GuildId, update);
                }

                return new CommandResponse
                {
                    OK = true,
                    Pages =
                    [
                        Utils.GetInstance().Embedify("/dailyverseclear", "Cleared all daily verse preferences successfully.", false)
                    ],
                    LogStatement = "/dailyverseclear",
                    RemoveWebhook = true
                };
            }
        }
    }
}
