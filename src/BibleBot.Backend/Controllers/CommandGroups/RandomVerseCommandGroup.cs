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
using BibleBot.Models;
using Microsoft.Extensions.Localization;
using Version = BibleBot.Models.Version;

namespace BibleBot.Backend.Controllers.CommandGroups
{
    public class RandomVerseCommandGroup(UserService userService, GuildService guildService, VersionService versionService,
                                         SpecialVerseProcessingService specialVerseProcessingService, IStringLocalizerFactory localizerFactory) : CommandGroup
    {
        private readonly IStringLocalizer _localizer = localizerFactory.Create(typeof(RandomVerseCommandGroup));
        private readonly IStringLocalizer _sharedLocalizer = localizerFactory.Create(typeof(SharedResource));

        public override string Name { get => "random"; set => throw new NotImplementedException(); }
        public override Command DefaultCommand { get => Commands.FirstOrDefault(cmd => cmd.Name == "usage"); set => throw new NotImplementedException(); }
        public override List<Command> Commands
        {
            get => [
                new RandomVerse(userService, guildService, versionService, specialVerseProcessingService, _localizer, _sharedLocalizer),
                new TrulyRandomVerse(userService, guildService, versionService, specialVerseProcessingService, _localizer, _sharedLocalizer)
            ]; set => throw new NotImplementedException();
        }

        private class RandomVerse(UserService userService, GuildService guildService, VersionService versionService,
                                  SpecialVerseProcessingService specialVerseProcessingService, IStringLocalizer localizer, IStringLocalizer sharedLocalizer) : Command
        {
            public override string Name { get => "usage"; set => throw new NotImplementedException(); }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                if (req.GuildId == "238001909716353025")
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages =
                        [
                            Utils.GetInstance().Embedify("/random", localizer["RandomVerseAntiSpamProvision"], true)
                        ],
                        LogStatement = "/random",
                        Culture = CultureInfo.CurrentUICulture.Name
                    };
                }

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

                Version idealVersion = await versionService.GetPreferenceOrDefault(idealUser, idealGuild, false);
                VerseResult verseResult = await specialVerseProcessingService.GetRandomVerse(idealVersion, titlesEnabled, verseNumbersEnabled);

#pragma warning disable IDE0046 // Convert to conditional expression
                if (verseResult == null)
                {
                    return new VerseResponse
                    {
                        OK = false,
                        Verses = null,
                        LogStatement = "/random",
                        Culture = CultureInfo.CurrentUICulture.Name
                    };
                }
#pragma warning restore IDE0046 // Convert to conditional expression

                return new VerseResponse
                {
                    OK = true,
                    Verses = [verseResult],
                    DisplayStyle = displayStyle,
                    LogStatement = "/random",
                    Culture = CultureInfo.CurrentUICulture.Name,
                    CultureFooter = string.Format(sharedLocalizer["GlobalFooter"], Utils.Version)
                };
            }
        }

        private class TrulyRandomVerse(UserService userService, GuildService guildService, VersionService versionService,
                                       SpecialVerseProcessingService specialVerseProcessingService, IStringLocalizer localizer, IStringLocalizer sharedLocalizer) : Command
        {
            public override string Name { get => "true"; set => throw new NotImplementedException(); }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                if (req.GuildId == "238001909716353025")
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages =
                        [
                            Utils.GetInstance().Embedify("/truerandom", localizer["RandomVerseAntiSpamProvision"], true)
                        ],
                        LogStatement = "/truerandom",
                        Culture = CultureInfo.CurrentUICulture.Name
                    };
                }

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

                Version idealVersion = await versionService.GetPreferenceOrDefault(idealUser, idealGuild, false);
                VerseResult verseResult = await specialVerseProcessingService.GetTrulyRandomVerse(idealVersion, titlesEnabled, verseNumbersEnabled);

#pragma warning disable IDE0046 // Convert to conditional expression
                if (verseResult == null)
                {
                    return new VerseResponse
                    {
                        OK = false,
                        Verses = null,
                        LogStatement = "/truerandom",
                        Culture = CultureInfo.CurrentUICulture.Name
                    };
                }
#pragma warning restore IDE0046 // Convert to conditional expression

                return new VerseResponse
                {
                    OK = true,
                    Verses = [verseResult],
                    DisplayStyle = displayStyle,
                    LogStatement = "/truerandom",
                    Culture = CultureInfo.CurrentUICulture.Name,
                    CultureFooter = string.Format(sharedLocalizer["GlobalFooter"], Utils.Version)
                };

            }
        }
    }
}

