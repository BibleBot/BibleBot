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

        public VersionGroup(UserService userService, GuildService guildService)
        {
            Name = "versions";
            IsOwnerOnly = false;
            Commands = new List<ICommand>
            {
                new VersionSet(_userService, _guildService)
            };
            DefaultCommand = Commands.Where(cmd => cmd.Name == "usage").FirstOrDefault();

            _userService = userService;
            _guildService = guildService;
        }

        public class VersionSet : ICommand
        {
            public string Name { get; set;}
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }

            private readonly UserService _userService;
            private readonly GuildService _guildService;

            public VersionSet(UserService userService, GuildService guildService)
            {
                Name = "set";
                ExpectedArguments = 1;
                PermissionsRequired = null;

                _userService = userService;
                _guildService = guildService;
            }

            public async Task<CommandResponse> ProcessCommand(List<string> args)
            {
                var newVersion = args[0].ToUpperInvariant();

                return new CommandResponse();
            }
        }
    }
}