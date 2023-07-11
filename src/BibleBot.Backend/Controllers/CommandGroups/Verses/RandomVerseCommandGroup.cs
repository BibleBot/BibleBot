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
        public bool IsOwnerOnly { get; set; }
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
            IsOwnerOnly = false;
            Commands = new List<ICommand>
            {
                new RandomVerse(_userService, _guildService, _versionService, _spProvider, _bibleProviders),
                new TrulyRandomVerse(_userService, _guildService, _versionService, _spProvider, _bibleProviders)
            };
            DefaultCommand = Commands.Where(cmd => cmd.Name == "usage").FirstOrDefault();
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
                            new Utils().Embedify("/random", "This server has personally requested that this command be only used in DMs to avoid spam.", true)
                        },
                        LogStatement = "/random"
                    };
                }

                var idealUser = _userService.Get(req.UserId);
                var idealGuild = _guildService.Get(req.GuildId);


                var version = "RSV";
                var verseNumbersEnabled = true;
                var titlesEnabled = true;
                var displayStyle = "embed";

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
                    displayStyle = idealGuild.DisplayStyle == null ? displayStyle : idealGuild.DisplayStyle;
                }

                var idealVersion = _versionService.Get(version);
                string randomRef = await _svProvider.GetRandomVerse();
                IBibleProvider provider = _bibleProviders.Where(pv => pv.Name == idealVersion.Source).FirstOrDefault();

                if (provider != null)
                {
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

                return new VerseResponse
                {
                    OK = false,
                    Verses = null,
                    LogStatement = $"Couldn't find a provider for {randomRef} {idealVersion.Abbreviation}."
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
                            new Utils().Embedify("/truerandom", "This server has personally requested that this command be only used in DMs to avoid spam.", true)
                        },
                        LogStatement = "/truerandom"
                    };
                }

                var idealUser = _userService.Get(req.UserId);
                var idealGuild = _guildService.Get(req.GuildId);

                var version = "RSV";
                var verseNumbersEnabled = true;
                var titlesEnabled = true;
                var displayStyle = "embed";

                if (idealUser != null && !req.IsBot)
                {
                    version = idealUser.Version;
                    verseNumbersEnabled = idealUser.VerseNumbersEnabled;
                    titlesEnabled = idealUser.TitlesEnabled;
                }
                else if (idealGuild != null)
                {
                    version = idealGuild.Version;
                    displayStyle = idealGuild.DisplayStyle == null ? displayStyle : idealGuild.DisplayStyle;
                }

                var idealVersion = _versionService.Get(version);
                string trulyRandomRef = await _svProvider.GetTrulyRandomVerse();
                IBibleProvider provider = _bibleProviders.Where(pv => pv.Name == idealVersion.Source).FirstOrDefault();

                if (provider != null)
                {
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

                return new VerseResponse
                {
                    OK = false,
                    Verses = null,
                    LogStatement = $"Couldn't find a provider for {trulyRandomRef} {idealVersion.Abbreviation}."
                };
            }
        }
    }
}
