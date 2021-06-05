using System.Linq;
using System.Globalization;
using System.Collections.Generic;

using NodaTime;

using BibleBot.Lib;
using BibleBot.Backend.Models;
using BibleBot.Backend.Services;
using BibleBot.Backend.Services.Providers;

namespace BibleBot.Backend.Controllers.CommandGroups.Resources
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
        private readonly BibleGatewayProvider _bgProvider;

        public RandomVerseCommandGroup(UserService userService, GuildService guildService, VersionService versionService,
                                       SpecialVerseProvider spProvider, BibleGatewayProvider bgProvider)
        {
            _userService = userService;
            _guildService = guildService;
            _versionService = versionService;

            _spProvider = spProvider;
            _bgProvider = bgProvider;

            Name = "random";
            IsOwnerOnly = false;
            Commands = new List<ICommand>
            {
                new RandomVerse(_userService, _guildService, _versionService, _spProvider, _bgProvider),
                new TrulyRandomVerse(_userService, _guildService, _versionService, _spProvider, _bgProvider)
            };
            DefaultCommand = Commands.Where(cmd => cmd.Name == "usage").FirstOrDefault();
        }

        public class RandomVerse : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }

            private readonly UserService _userService;
            private readonly GuildService _guildService;
            private readonly VersionService _versionService;

            private readonly SpecialVerseProvider _spProvider;
            private readonly BibleGatewayProvider _bgProvider;

            public RandomVerse(UserService userService, GuildService guildService, VersionService versionService,
                               SpecialVerseProvider spProvider, BibleGatewayProvider bgProvider)
            {
                Name = "usage";
                ArgumentsError = null;
                ExpectedArguments = 0;
                PermissionsRequired = null;

                _userService = userService;
                _guildService = guildService;
                _versionService = versionService;

                _spProvider = spProvider;
                _bgProvider = bgProvider;
            }

            public IResponse ProcessCommand(Request req, List<string> args)
            {
                var idealUser = _userService.Get(req.UserId);

                var version = "RSV";
                var verseNumbersEnabled = true;
                var titlesEnabled = true;

                if (idealUser != null)
                {
                    version = idealUser.Version;
                    verseNumbersEnabled = idealUser.VerseNumbersEnabled;
                    titlesEnabled = idealUser.TitlesEnabled;
                }

                var idealVersion = _versionService.Get(version);

                if (idealVersion == null)
                {
                    idealVersion = _versionService.Get("RSV");
                }

                string randomRef = _spProvider.GetRandomVerse().GetAwaiter().GetResult();
                Verse verse = _bgProvider.GetVerse(randomRef, titlesEnabled, verseNumbersEnabled, idealVersion).GetAwaiter().GetResult();

                return new CommandResponse
                {
                    OK = true,
                    Pages = new List<InternalEmbed>
                    {
                        new Utils().Embedify($"{verse.Reference.AsString} - {verse.Reference.Version.Name}", verse.Title, verse.Text, false, null)
                    },
                    LogStatement = "+random"
                };
            }
        }

        public class TrulyRandomVerse : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }

            private readonly UserService _userService;
            private readonly GuildService _guildService;
            private readonly VersionService _versionService;

            private readonly SpecialVerseProvider _spProvider;
            private readonly BibleGatewayProvider _bgProvider;

            public TrulyRandomVerse(UserService userService, GuildService guildService, VersionService versionService,
                                    SpecialVerseProvider spProvider, BibleGatewayProvider bgProvider)
            {
                Name = "true";
                ArgumentsError = null;
                ExpectedArguments = 0;
                PermissionsRequired = null;

                _userService = userService;
                _guildService = guildService;
                _versionService = versionService;

                _spProvider = spProvider;
                _bgProvider = bgProvider;
            }

            public IResponse ProcessCommand(Request req, List<string> args)
            {
                var idealUser = _userService.Get(req.UserId);

                var version = "RSV";
                var verseNumbersEnabled = true;
                var titlesEnabled = true;

                if (idealUser != null)
                {
                    version = idealUser.Version;
                    verseNumbersEnabled = idealUser.VerseNumbersEnabled;
                    titlesEnabled = idealUser.TitlesEnabled;
                }

                var idealVersion = _versionService.Get(version);

                if (idealVersion == null)
                {
                    idealVersion = _versionService.Get("RSV");
                }

                string trulyRandomRef = _spProvider.GetTrulyRandomVerse().GetAwaiter().GetResult();
                Verse verse = _bgProvider.GetVerse(trulyRandomRef, titlesEnabled, verseNumbersEnabled, idealVersion).GetAwaiter().GetResult();

                return new CommandResponse
                {
                    OK = true,
                    Pages = new List<InternalEmbed>
                    {
                        new Utils().Embedify($"{verse.Reference.AsString} - {verse.Reference.Version.Name}", verse.Title, verse.Text, false, null)
                    },
                    LogStatement = "+random true"
                };
            }
        }
    }
}