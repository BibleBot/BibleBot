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
using System.Threading.Tasks;
using BibleBot.Backend.Services;
using BibleBot.Backend.Services.Providers;
using BibleBot.Models;
using Microsoft.Extensions.Localization;

namespace BibleBot.Backend.Controllers.CommandGroups.Verses
{
    public class RandomVerseCommandGroup(UserService userService, GuildService guildService, VersionService versionService,
                                         SpecialVerseProvider svProvider, List<IBibleProvider> bibleProviders, IStringLocalizerFactory localizerFactory) : CommandGroup
    {
        private readonly IStringLocalizer _localizer = localizerFactory.Create(typeof(RandomVerseCommandGroup));

        public override string Name { get => "random"; set => throw new NotImplementedException(); }
        public override Command DefaultCommand { get => Commands.FirstOrDefault(cmd => cmd.Name == "usage"); set => throw new NotImplementedException(); }
        public override List<Command> Commands
        {
            get => [
                new RandomVerse(userService, guildService, versionService, svProvider, bibleProviders, _localizer),
                new TrulyRandomVerse(userService, guildService, versionService, svProvider, bibleProviders, _localizer)
            ]; set => throw new NotImplementedException();
        }

        public class RandomVerse(UserService userService, GuildService guildService, VersionService versionService,
                           SpecialVerseProvider svProvider, List<IBibleProvider> bibleProviders, IStringLocalizer localizer) : Command
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
                            Utils.GetInstance().Embedify("/random", "This server has personally requested that this command be only used in DMs to avoid spam.", true)
                        ],
                        LogStatement = "/random"
                    };
                }

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
                string randomRef = await svProvider.GetRandomVerse();
                IBibleProvider provider = bibleProviders.FirstOrDefault(pv => pv.Name == idealVersion.Source) ?? throw new ProviderNotFoundException($"Couldn't find provider for '{randomRef} {idealVersion.Abbreviation}'");

                return new VerseResponse
                {
                    OK = true,
                    Verses =
                    [
                        await provider.GetVerse(randomRef, titlesEnabled, verseNumbersEnabled, idealVersion)
                    ],
                    DisplayStyle = displayStyle,
                    LogStatement = "/random"
                };
            }
        }

        public class TrulyRandomVerse(UserService userService, GuildService guildService, VersionService versionService,
                                SpecialVerseProvider svProvider, List<IBibleProvider> bibleProviders, IStringLocalizer localizer) : Command
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
                            Utils.GetInstance().Embedify("/truerandom", "This server has personally requested that this command be only used in DMs to avoid spam.", true)
                        ],
                        LogStatement = "/truerandom"
                    };
                }

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
                }
                else if (idealGuild != null)
                {
                    version = idealGuild.Version;
                    displayStyle = idealGuild.DisplayStyle ?? displayStyle;
                }

                Models.Version idealVersion = await versionService.Get(version) ?? await versionService.Get("RSV");
                string trulyRandomRef = await svProvider.GetTrulyRandomVerse();
                IBibleProvider provider = bibleProviders.FirstOrDefault(pv => pv.Name == idealVersion.Source) ?? throw new ProviderNotFoundException($"Couldn't find provider for '{trulyRandomRef} {idealVersion.Abbreviation}'");

                return new VerseResponse
                {
                    OK = true,
                    Verses =
                    [
                        await provider.GetVerse(trulyRandomRef, titlesEnabled, verseNumbersEnabled, idealVersion)
                    ],
                    DisplayStyle = displayStyle,
                    LogStatement = "/truerandom"
                };
            }
        }
    }
}

