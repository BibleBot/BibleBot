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
using BibleBot.Backend.InternalModels;
using BibleBot.Backend.Services;
using BibleBot.Backend.Services.Providers;
using BibleBot.Models;
using Microsoft.Extensions.Localization;
using MongoDB.Driver;
using NodaTime;

namespace BibleBot.Backend.Controllers.CommandGroups
{
    public class DailyVerseCommandGroup(UserService userService, GuildService guildService, VersionService versionService,
                                        SpecialVerseProvider svProvider, List<IBibleProvider> bibleProviders, IStringLocalizerFactory localizerFactory) : CommandGroup
    {
        private readonly IStringLocalizer _localizer = localizerFactory.Create(typeof(DailyVerseCommandGroup));
        private readonly IStringLocalizer _sharedLocalizer = localizerFactory.Create(typeof(SharedResource));

        public override string Name { get => "dailyverse"; set => throw new NotImplementedException(); }
        public override Command DefaultCommand { get => Commands.FirstOrDefault(cmd => cmd.Name == "usage"); set => throw new NotImplementedException(); }
        public override List<Command> Commands
        {
            get => [
                new DailyVerseUsage(userService, guildService, versionService, svProvider, bibleProviders, _sharedLocalizer),
                new DailyVerseSet(guildService, _localizer),
                new DailyVerseRole(guildService, _localizer),
                new DailyVerseStatus(guildService, _localizer),
                new DailyVerseClear(guildService, _localizer)
            ]; set => throw new NotImplementedException();
        }

        public class DailyVerseUsage(UserService userService, GuildService guildService, VersionService versionService,
                               SpecialVerseProvider svProvider, List<IBibleProvider> bibleProviders, IStringLocalizer sharedLocalizer) : Command
        {
            public override string Name { get => "usage"; set => throw new NotImplementedException(); }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                User idealUser = await userService.Get(req.UserId);
                Guild idealGuild = await guildService.Get(req.GuildId);

                bool verseNumbersEnabled = true;
                bool titlesEnabled = true;
                string displayStyle = "embed";

                if (idealUser != null && !req.IsBot)
                {
                    verseNumbersEnabled = idealUser.VerseNumbersEnabled;
                    titlesEnabled = idealUser.TitlesEnabled;
                    displayStyle = idealUser.DisplayStyle;
                }
                else if (idealGuild != null)
                {
                    displayStyle = idealGuild.DisplayStyle ?? displayStyle;
                }

                Models.Version idealVersion = await versionService.GetPreferenceOrDefault(idealUser, idealGuild, false);
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
                    LogStatement = "/dailyverse",
                    Culture = CultureInfo.CurrentUICulture.Name,
                    CultureFooter = string.Format(sharedLocalizer["GlobalFooter"], Utils.Version)
                };
            }
        }

        public class DailyVerseSet(GuildService guildService, IStringLocalizer localizer) : Command
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
                            Utils.GetInstance().Embedify("/setdailyverse", localizer["AutomaticDailyVerseNoDMs"], true)
                        ],
                        LogStatement = "/setdailyverse",
                        Culture = CultureInfo.CurrentUICulture.Name
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
                                    Utils.GetInstance().Embedify("/setdailyverse", localizer["SetDailyVerseSuccess"], false)
                                ],
                                LogStatement = $"/setdailyverse {args[0]} {args[1]}",
                                CreateWebhook = true,
                                RemoveWebhook = true,
                                Culture = CultureInfo.CurrentUICulture.Name
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
                                Utils.GetInstance().Embedify("/setdailyverse", localizer["SetDailyVerseSetupNotice"], true)
                            ],
                            LogStatement = "/setdailyverse",
                            Culture = CultureInfo.CurrentUICulture.Name
                        };
                    }
                }

                return new CommandResponse
                {
                    OK = false,
                    Pages =
                    [
                        Utils.GetInstance().Embedify("/setdailyverse", localizer["SetDailyVerseSetupNotice"], true)
                    ],
                    LogStatement = "/setdailyverse",
                    Culture = CultureInfo.CurrentUICulture.Name
                };
            }
        }

        public class DailyVerseRole(GuildService guildService, IStringLocalizer localizer) : Command
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
                            Utils.GetInstance().Embedify("/dailyverserole", localizer["AutomaticDailyVerseNoDMs"], true)
                        ],
                        LogStatement = "/dailyverserole",
                        Culture = CultureInfo.CurrentUICulture.Name
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
                                Utils.GetInstance().Embedify("/setdailyverserole", localizer["SetDailyVerseRoleSuccess"], false)
                            ],
                            LogStatement = $"/setdailyverserole {args[0]}",
                            Culture = CultureInfo.CurrentUICulture.Name
                        };
                    }
                }

                return new CommandResponse
                {
                    OK = false,
                    Pages =
                    [
                        Utils.GetInstance().Embedify("/setdailyverserole", localizer["SetDailyVerseRoleNotSetup"], true)
                    ],
                    LogStatement = $"/setdailyverserole {args[0]}",
                    Culture = CultureInfo.CurrentUICulture.Name
                };
            }
        }

        public class DailyVerseStatus(GuildService guildService, IStringLocalizer localizer) : Command
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

                        string mentionClause = idealGuild.DailyVerseRoleId != null ? string.Format($" {localizer["DailyVerseStatusRoleAddenda"]} ", $"<@&{idealGuild.DailyVerseRoleId}>") : "<rm>";
                        string resp = $"{localizer["DailyVerseStatusDetail"]}\n\n" +
                                      $"{localizer["DailyVerseStatusHelpSet"]}\n" +
                                      $"{localizer["DailyVerseStatusHelpSetRole"]}\n" +
                                      $"{localizer["DailyVerseStatusHelpClear"]}";

                        resp = string.Format(resp, [$"`{timeFormatted}`", $"**{idealGuild.DailyVerseTimeZone}**", $"<#{idealGuild.DailyVerseChannelId}>", mentionClause]).Replace("<rm>", " ");

                        return new CommandResponse
                        {
                            OK = true,
                            Pages =
                            [
                                Utils.GetInstance().Embedify("/dailyversestatus", resp, false)
                            ],
                            LogStatement = "/dailyversestatus",
                            Culture = CultureInfo.CurrentUICulture.Name
                        };
                    }
                }

                return new CommandResponse
                {
                    OK = false,
                    Pages =
                    [
                        Utils.GetInstance().Embedify("/dailyversestatus", localizer["DailyVerseStatusNotSetup"], true)
                    ],
                    LogStatement = "/dailyversestatus",
                    Culture = CultureInfo.CurrentUICulture.Name
                };
            }
        }

        public class DailyVerseClear(GuildService guildService, IStringLocalizer localizer) : Command
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
                        Utils.GetInstance().Embedify("/cleardailyverse", localizer["ClearDailyVerseSuccess"], false)
                    ],
                    LogStatement = "/cleardailyverse",
                    RemoveWebhook = true,
                    Culture = CultureInfo.CurrentUICulture.Name
                };
            }
        }
    }
}
