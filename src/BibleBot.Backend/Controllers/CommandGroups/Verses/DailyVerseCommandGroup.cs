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
using BibleBot.Backend.Models;
using BibleBot.Backend.Services;
using BibleBot.Backend.Services.Providers;
using BibleBot.Lib;
using NodaTime;

namespace BibleBot.Backend.Controllers.CommandGroups.Verses
{
    public class DailyVerseCommandGroup : ICommandGroup
    {
        public string Name { get; set; }
        public bool IsOwnerOnly { get; set; }
        public ICommand DefaultCommand { get; set; }
        public List<ICommand> Commands { get; set; }

        private readonly UserService _userService;
        private readonly GuildService _guildService;
        private readonly VersionService _versionService;

        private readonly SpecialVerseProvider _spProvider;
        private readonly List<IBibleProvider> _bibleProviders;

        public DailyVerseCommandGroup(UserService userService, GuildService guildService, VersionService versionService,
                                      SpecialVerseProvider spProvider, List<IBibleProvider> bibleProviders)
        {
            _userService = userService;
            _guildService = guildService;
            _versionService = versionService;

            _spProvider = spProvider;
            _bibleProviders = bibleProviders;

            Name = "dailyverse";
            IsOwnerOnly = false;
            Commands = new List<ICommand>
            {
                new DailyVerseUsage(_userService, _guildService, _versionService, _spProvider, _bibleProviders),
                new DailyVerseSet(_guildService),
                new DailyVerseStatus(_guildService),
                new DailyVerseClear(_guildService)
            };
            DefaultCommand = Commands.Where(cmd => cmd.Name == "usage").FirstOrDefault();
        }

        public class DailyVerseUsage : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }

            private readonly UserService _userService;
            private readonly GuildService _guildService;
            private readonly VersionService _versionService;

            private readonly SpecialVerseProvider _spProvider;
            private readonly List<IBibleProvider> _bibleProviders;

            public DailyVerseUsage(UserService userService, GuildService guildService, VersionService versionService,
                                   SpecialVerseProvider spProvider, List<IBibleProvider> bibleProviders)
            {
                Name = "usage";
                ArgumentsError = null;
                ExpectedArguments = 0;
                PermissionsRequired = null;

                _userService = userService;
                _guildService = guildService;
                _versionService = versionService;

                _spProvider = spProvider;
                _bibleProviders = bibleProviders;
            }

            public IResponse ProcessCommand(Request req, List<string> args)
            {
                var idealUser = _userService.Get(req.UserId);
                var idealGuild = _guildService.Get(req.GuildId);

                var version = "RSV";
                var verseNumbersEnabled = true;
                var titlesEnabled = true;
                var displayStyle = "embed";

                if (idealUser != null)
                {
                    version = idealUser.Version;
                    verseNumbersEnabled = idealUser.VerseNumbersEnabled;
                    titlesEnabled = idealUser.TitlesEnabled;
                    displayStyle = idealUser.DisplayStyle;
                }
                else if (idealGuild != null)
                {
                    version = idealGuild.Version;
                }

                var idealVersion = _versionService.Get(version);
                string votdRef = _spProvider.GetDailyVerse().GetAwaiter().GetResult();
                IBibleProvider provider = _bibleProviders.Where(pv => pv.Name == idealVersion.Source).FirstOrDefault();

                if (provider != null)
                {
                    return new VerseResponse
                    {
                        OK = true,
                        Verses = new List<Verse>
                        {
                            provider.GetVerse(votdRef, titlesEnabled, verseNumbersEnabled, idealVersion).GetAwaiter().GetResult()
                        },
                        DisplayStyle = displayStyle,
                        LogStatement = "+dailyverse"
                    };
                }

                throw new ProviderNotFoundException();
            }
        }

        public class DailyVerseSet : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }

            private readonly GuildService _guildService;

            public DailyVerseSet(GuildService guildService)
            {
                Name = "set";
                ArgumentsError = null;
                ExpectedArguments = 0;
                PermissionsRequired = new List<Permissions>
                {
                    Permissions.MANAGE_GUILD
                };

                _guildService = guildService;
            }

            public IResponse ProcessCommand(Request req, List<string> args)
            {
                if (req.IsDM)
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages = new List<InternalEmbed>
                        {
                            new Utils().Embedify("+dailyverse set", "The automatic daily verse cannot be used in DMs, as DMs do not allow for webhooks.", true)
                        },
                        LogStatement = "+dailyverse set"
                    };
                }

                if (args.Count == 2)
                {
                    var timeSplit = args[0].Split(":");

                    try
                    {
                        var hour = int.Parse(timeSplit[0]);
                        var minute = int.Parse(timeSplit[1]);

                        if (((hour > -1 && hour < 24) || (minute > -1 && minute < 60)) && DateTimeZoneProviders.Tzdb.GetZoneOrNull(args[1]) != null)
                        {
                            var idealGuild = _guildService.Get(req.GuildId);

                            if (idealGuild != null)
                            {
                                idealGuild.DailyVerseTime = args[0];
                                idealGuild.DailyVerseTimeZone = args[1];
                                _guildService.Update(req.GuildId, idealGuild);
                            }
                            else
                            {
                                _guildService.Create(new Guild
                                {
                                    GuildId = req.GuildId,
                                    Version = "RSV",
                                    Language = "english",
                                    Prefix = "+",
                                    IgnoringBrackets = "<>",
                                    IsDM = req.IsDM,
                                    DailyVerseTime = args[0],
                                    DailyVerseTimeZone = args[1]
                                });
                            }

                            return new CommandResponse
                            {
                                OK = true,
                                Pages = new List<InternalEmbed>
                                {
                                    new Utils().Embedify("+dailyverse set", "Set automatic daily verse successfully.", false)
                                },
                                LogStatement = $"+dailyverse set {args[0]} {args[0]}",
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
                            Pages = new List<InternalEmbed>
                            {
                                new Utils().Embedify("+dailyverse set", "Go to https://biblebot.xyz/daily-verse-setup/ to continue the setup process.", true)
                            },
                            LogStatement = "+dailyverse set"
                        };
                    }
                }

                return new CommandResponse
                {
                    OK = false,
                    Pages = new List<InternalEmbed>
                    {
                        new Utils().Embedify("+dailyverse set", "Go to https://biblebot.xyz/daily-verse-setup/ to continue the setup process.", true)
                    },
                    LogStatement = "+dailyverse set"
                };
            }
        }

        public class DailyVerseStatus : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }

            private readonly GuildService _guildService;

            public DailyVerseStatus(GuildService guildService)
            {
                Name = "status";
                ArgumentsError = null;
                ExpectedArguments = 0;
                PermissionsRequired = null;

                _guildService = guildService;
            }

            public IResponse ProcessCommand(Request req, List<string> args)
            {
                var idealGuild = _guildService.Get(req.GuildId);

                if (idealGuild != null)
                {
                    if (idealGuild.DailyVerseChannelId != null && idealGuild.DailyVerseTime != null &&
                        idealGuild.DailyVerseTimeZone != null && idealGuild.DailyVerseWebhook != null)
                    {
                        var preferredTimeZone = DateTimeZoneProviders.Tzdb[idealGuild.DailyVerseTimeZone];
                        var currentTime = SystemClock.Instance.GetCurrentInstant().InZone(preferredTimeZone);

                        var preferredHour = int.Parse(idealGuild.DailyVerseTime.Split(":")[0]);
                        var preferredMinute = int.Parse(idealGuild.DailyVerseTime.Split(":")[1]);

                        var todaysHourPassed = false;
                        var todaysMinutePassed = false;

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

                        var timeFormatted = currentTime.ToString("h:mm tt", new CultureInfo("en-US"));

                        var resp = $"The daily verse will be sent at `{timeFormatted}`, in the **{preferredTimeZone.ToString()}** time zone, and will be published in <#{idealGuild.DailyVerseChannelId}>. It will use this server's preferred version, which you can find by using **`+version`**.\n\nUse **`+dailyverse set`** to set a new time or channel.\nUse **`+dailyverse clear`** to clear automatic daily verse settings.";

                        return new CommandResponse
                        {
                            OK = true,
                            Pages = new List<InternalEmbed>
                            {
                                new Utils().Embedify("+dailyverse status", resp, false)
                            },
                            LogStatement = "+dailyverse status"
                        };
                    }
                }

                return new CommandResponse
                {
                    OK = false,
                    Pages = new List<InternalEmbed>
                    {
                        new Utils().Embedify("+dailyverse status", "The automatic daily verse has not been setup for this server or has been configured incorrectly. Use `+dailyverse set` to setup the automatic daily verse.", true)
                    },
                    LogStatement = "+dailyverse status"
                };
            }
        }

        public class DailyVerseClear : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }

            private readonly GuildService _guildService;

            public DailyVerseClear(GuildService guildService)
            {
                Name = "clear";
                ArgumentsError = null;
                ExpectedArguments = 0;
                PermissionsRequired = new List<Permissions>
                {
                    Permissions.MANAGE_GUILD
                };

                _guildService = guildService;
            }

            public IResponse ProcessCommand(Request req, List<string> args)
            {
                var idealGuild = _guildService.Get(req.GuildId);

                if (idealGuild != null)
                {
                    idealGuild.DailyVerseTime = null;
                    idealGuild.DailyVerseTimeZone = null;
                    idealGuild.DailyVerseWebhook = null;
                    idealGuild.DailyVerseChannelId = null;

                    _guildService.Update(req.GuildId, idealGuild);
                }

                return new CommandResponse
                {
                    OK = true,
                    Pages = new List<InternalEmbed>
                    {
                        new Utils().Embedify("+dailyverse clear", "Cleared all daily verse webhooks successfully.", false)
                    },
                    LogStatement = "+dailyverse clear",
                    RemoveWebhook = true
                };
            }
        }
    }
}
