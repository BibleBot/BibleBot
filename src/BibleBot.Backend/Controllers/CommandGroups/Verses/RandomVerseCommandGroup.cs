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
using BibleBot.Backend.Services.Providers;
using BibleBot.Models;

namespace BibleBot.Backend.Controllers.CommandGroups.Verses
{
    public class RandomVerseCommandGroup : ICommandGroup
    {
        public string Name { get; set; }
        public bool IsStaffOnly { get; set; }
        public ICommand DefaultCommand { get; set; }
        public List<ICommand> Commands { get; set; }

        private readonly UserService _userService;
        private readonly GuildService _guildService;
        private readonly VersionService _versionService;

        private readonly SpecialVerseProvider _spProvider;
        private readonly List<IBibleProvider> _bibleProviders;

        public RandomVerseCommandGroup(UserService userService, GuildService guildService, VersionService versionService,
                                       SpecialVerseProvider spProvider, List<IBibleProvider> bibleProviders)
        {
            _userService = userService;
            _guildService = guildService;
            _versionService = versionService;

            _spProvider = spProvider;
            _bibleProviders = bibleProviders;

            Name = "random";
            IsStaffOnly = false;
            Commands = new List<ICommand>
            {
                new RandomVerse(_userService, _guildService, _versionService, _spProvider, _bibleProviders),
                new TrulyRandomVerse(_userService, _guildService, _versionService, _spProvider, _bibleProviders)
            };
            DefaultCommand = Commands.FirstOrDefault(cmd => cmd.Name == "usage");
        }

        public class RandomVerse : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }
            public bool BotAllowed { get; set; }

            private readonly UserService _userService;
            private readonly GuildService _guildService;
            private readonly VersionService _versionService;

            private readonly SpecialVerseProvider _svProvider;
            private readonly List<IBibleProvider> _bibleProviders;

            public RandomVerse(UserService userService, GuildService guildService, VersionService versionService,
                               SpecialVerseProvider svProvider, List<IBibleProvider> bibleProviders)
            {
                Name = "usage";
                ArgumentsError = null;
                ExpectedArguments = 0;
                PermissionsRequired = null;
                BotAllowed = true;

                _userService = userService;
                _guildService = guildService;
                _versionService = versionService;

                _svProvider = svProvider;
                _bibleProviders = bibleProviders;
            }

            public async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                if (req.GuildId == "238001909716353025")
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages = new List<InternalEmbed>
                        {
                            Utils.GetInstance().Embedify("/random", "This server has personally requested that this command be only used in DMs to avoid spam.", true)
                        },
                        LogStatement = "/random"
                    };
                }

                User idealUser = await _userService.Get(req.UserId);
                Guild idealGuild = await _guildService.Get(req.GuildId);

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

                Version idealVersion = await _versionService.Get(version);
                string randomRef = await _svProvider.GetRandomVerse();
                IBibleProvider provider = _bibleProviders.FirstOrDefault(pv => pv.Name == idealVersion.Source) ?? throw new ProviderNotFoundException($"Couldn't find provider for '{randomRef} {idealVersion.Abbreviation}'");

                return new VerseResponse
                {
                    OK = true,
                    Verses = new List<Verse>
                    {
                        await provider.GetVerse(randomRef, titlesEnabled, verseNumbersEnabled, idealVersion)
                    },
                    DisplayStyle = displayStyle,
                    LogStatement = "/random"
                };
            }
        }

        public class TrulyRandomVerse : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }
            public bool BotAllowed { get; set; }

            private readonly UserService _userService;
            private readonly GuildService _guildService;
            private readonly VersionService _versionService;

            private readonly SpecialVerseProvider _svProvider;
            private readonly List<IBibleProvider> _bibleProviders;

            public TrulyRandomVerse(UserService userService, GuildService guildService, VersionService versionService,
                                    SpecialVerseProvider svProvider, List<IBibleProvider> bibleProviders)
            {
                Name = "true";
                ArgumentsError = null;
                ExpectedArguments = 0;
                PermissionsRequired = null;
                BotAllowed = true;

                _userService = userService;
                _guildService = guildService;
                _versionService = versionService;

                _svProvider = svProvider;
                _bibleProviders = bibleProviders;
            }

            public async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                if (req.GuildId == "238001909716353025")
                {
                    return new CommandResponse
                    {
                        OK = false,
                        Pages = new List<InternalEmbed>
                        {
                            Utils.GetInstance().Embedify("/truerandom", "This server has personally requested that this command be only used in DMs to avoid spam.", true)
                        },
                        LogStatement = "/truerandom"
                    };
                }

                User idealUser = await _userService.Get(req.UserId);
                Guild idealGuild = await _guildService.Get(req.GuildId);

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

                Version idealVersion = await _versionService.Get(version);
                string trulyRandomRef = await _svProvider.GetTrulyRandomVerse();
                IBibleProvider provider = _bibleProviders.FirstOrDefault(pv => pv.Name == idealVersion.Source) ?? throw new ProviderNotFoundException($"Couldn't find provider for '{trulyRandomRef} {idealVersion.Abbreviation}'");

                return new VerseResponse
                {
                    OK = true,
                    Verses = new List<Verse>
                    {
                        await provider.GetVerse(trulyRandomRef, titlesEnabled, verseNumbersEnabled, idealVersion)
                    },
                    DisplayStyle = displayStyle,
                    LogStatement = "/truerandom"
                };
            }
        }
    }
}

