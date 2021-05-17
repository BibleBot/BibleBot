using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using BibleBot.Lib;
using BibleBot.Backend.Models;
using BibleBot.Backend.Services;

namespace BibleBot.Backend.Controllers.CommandGroups.Settings
{
    public class VersionGroup : ICommandGroup
    {
        public string Name { get; set; }
        public bool IsOwnerOnly { get; set; }
        public ICommand DefaultCommand { get; set; }
        public List<ICommand> Commands { get; set; }

        private readonly UserService _userService;
        private readonly GuildService _guildService;
        private readonly VersionService _versionService;

        public VersionGroup(UserService userService, GuildService guildService, VersionService versionService)
        {
            Name = "version";
            IsOwnerOnly = false;
            Commands = new List<ICommand>
            {
                new VersionUsage(_userService, _guildService, _versionService),
                new VersionSet(_userService, _guildService, _versionService)
            };
            DefaultCommand = Commands.Where(cmd => cmd.Name == "usage").FirstOrDefault();

            _userService = userService;
            _guildService = guildService;
            _versionService = versionService;
        }

        public class VersionUsage : ICommand
        {
            public string Name { get; set;}
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }

            private readonly UserService _userService;
            private readonly GuildService _guildService;
            private readonly VersionService _versionService;

            public VersionUsage(UserService userService, GuildService guildService, VersionService versionService)
            {
                Name = "usage";
                ExpectedArguments = 0;
                PermissionsRequired = null;

                _userService = userService;
                _guildService = guildService;
                _versionService = versionService;
            }

            public CommandResponse ProcessCommand(Request req, List<string> args)
            {
                var idealUser = _userService.Get(req.UserId);

                if (idealUser != null)
                {
                    var idealVersion = _versionService.Get(idealUser.Version);

                    if (idealVersion != null)
                    {

                        return new CommandResponse
                        {
                            OK = true,
                            Pages = new List<DiscordEmbed>
                            {
                                new Utils().Embedify("+version", $"You are currently using **{idealVersion.Name}**.", false)
                            }
                        };
                    }
                }

                return new CommandResponse
                {
                    OK = true,
                    Pages = new List<DiscordEmbed>
                    {
                        new Utils().Embedify("+version", "No version settings found.", true)
                    }
                };
            }
        }

        public class VersionSet : ICommand
        {
            public string Name { get; set;}
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }

            private readonly UserService _userService;
            private readonly GuildService _guildService;
            private readonly VersionService _versionService;

            public VersionSet(UserService userService, GuildService guildService, VersionService versionService)
            {
                Name = "set";
                ExpectedArguments = 1;
                PermissionsRequired = null;

                _userService = userService;
                _guildService = guildService;
                _versionService = versionService;
            }

            public CommandResponse ProcessCommand(Request req, List<string> args)
            {
                var newVersion = args[0].ToUpperInvariant();
                var idealVersion = _versionService.Get(newVersion);

                if (idealVersion != null)
                {
                    var idealUser = _userService.Get(req.UserId);

                    if (idealUser != null)
                    {
                        idealUser.Version = idealVersion.Abbreviation;
                        _userService.Update(req.UserId, idealUser);

                        return new CommandResponse
                        {
                            OK = true,
                            Pages = new List<DiscordEmbed>
                            {
                                new Utils().Embedify("+version set", "Set version successfully.", false)
                            }
                        };
                    }
                }

                return new CommandResponse
                {
                    OK = true,
                    Pages = new List<DiscordEmbed>
                    {
                        new Utils().Embedify("+version set", "Failed to set version. See `+version list`.", true)
                    }
                };
            }
        }
    }
}