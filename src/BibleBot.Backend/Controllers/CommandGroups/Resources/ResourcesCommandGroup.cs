using System.Linq;
using System.Globalization;
using System.Collections.Generic;

using System.Reflection;

using BibleBot.Lib;
using BibleBot.Backend.Models;
using BibleBot.Backend.Services;
using BibleBot.Backend.Services.Providers;

namespace BibleBot.Backend.Controllers.CommandGroups.Resources
{
    public class ResourcesCommandGroup : ICommandGroup
    {
        public string Name { get; set; }
        public bool IsOwnerOnly { get; set; }
        public ICommand DefaultCommand { get; set; }
        public List<ICommand> Commands { get; set; }

        private readonly UserService _userService;
        private readonly GuildService _guildService;
        private readonly ResourceService _resourceService;

        private readonly List<IResource> _resources;

        public ResourcesCommandGroup(UserService userService, GuildService guildService, List<IResource> resources)
        {
            _userService = userService;
            _guildService = guildService;

            _resources = resources;

            Name = "resources";
            IsOwnerOnly = false;
            Commands = new List<ICommand>
            {
                new ResourcesUsage(_userService, _guildService, _resources),
                //new InfoBibleBot(_userService, _guildService),
                //new InfoInvite()
            };
            DefaultCommand = Commands.Where(cmd => cmd.Name == "usage").FirstOrDefault();
        }

        public class ResourcesUsage : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }

            private readonly UserService _userService;
            private readonly GuildService _guildService;

            private readonly List<IResource> _resources;

            public ResourcesUsage(UserService userService, GuildService guildService, List<IResource> resources)
            {
                Name = "usage";
                ArgumentsError = null;
                ExpectedArguments = 0;
                PermissionsRequired = null;

                _userService = userService;
                _guildService = guildService;

                _resources = resources;
            }

            public IResponse ProcessCommand(Request req, List<string> args)
            {
                var creeds = _resources.Where(res => res.Type == ResourceType.CREED);
                var creedsList = "";

                foreach (var creed in creeds)
                {
                    creedsList += $"**{creed.CommandReference}** - {creed.Title}\n";
                }

                var catechisms = _resources.Where(res => res.Type == ResourceType.CATECHISM);
                var catechismsList = "";

                foreach (var catechism in catechisms)
                {
                    catechismsList += $"**{catechism.CommandReference}** - {catechism.Title}\n";
                }


                var resp = $"**__Creeds__**\n" + creedsList.Substring(0, creedsList.Length - 1) +
                "\n\n**__Catechisms__**\n" + catechismsList.Substring(0, catechismsList.Length - 1) +
                "\n\nTo use a resource, do `+resources <name>`.\nFor example, `+resources nicene` or `+resources ccc`.";


                return new CommandResponse
                {
                    OK = true,
                    Pages = new List<InternalEmbed>
                    {
                        new Utils().Embedify("+resources", resp, false)
                    },
                    LogStatement = "+resources"
                };
            }
        }
    }
}