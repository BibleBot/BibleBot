using System.Threading.Tasks;
using System.Collections.Generic;

using BibleBot.Lib;
using BibleBot.Backend.Models;
using Microsoft.Extensions.DependencyInjection;

namespace BibleBot.Backend.Services.CommandGroups.Settings
{
    public class Versions : CommandGroup
    {
        private readonly string Name = "versions";
        private readonly bool IsOwnerOnly = false;
        public readonly List<ICommand> Commands;

        private readonly UserService _userService;
        private readonly GuildService _guildService;

        public Versions(UserService userService, GuildService guildService)
        {
            _userService = userService;
            _guildService = guildService;

            Commands = new List<ICommand>
            {
                new VersionSet(_userService, _guildService)
            };
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