/*
* Copyright (C) 2016-2023 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using BibleBot.Backend.Models;
using BibleBot.Backend.Services;
using BibleBot.Backend.Services.Providers;

namespace BibleBot.Backend.Controllers.CommandGroups.Resources
{
    public class ResourceCommandGroup : ICommandGroup
    {
        public string Name { get; set; }
        public bool IsOwnerOnly { get; set; }
        public ICommand DefaultCommand { get; set; }
        public List<ICommand> Commands { get; set; }

        private readonly UserService _userService;
        private readonly GuildService _guildService;

        private readonly List<IResource> _resources;

        public ResourceCommandGroup(UserService userService, GuildService guildService, List<IResource> resources)
        {
            _userService = userService;
            _guildService = guildService;

            _resources = resources;

            Name = "resource";
            IsOwnerOnly = false;
            Commands = new List<ICommand>
            {
                new ResourceUsage(_userService, _guildService, _resources),
            };
            DefaultCommand = Commands.Where(cmd => cmd.Name == "usage").FirstOrDefault();
        }

        public class ResourceUsage : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }
            public bool BotAllowed { get; set; }

            private readonly UserService _userService;
            private readonly GuildService _guildService;

            private readonly List<IResource> _resources;

            public ResourceUsage(UserService userService, GuildService guildService, List<IResource> resources)
            {
                Name = "usage";
                ArgumentsError = null;
                ExpectedArguments = 0;
                PermissionsRequired = null;
                BotAllowed = true;

                _userService = userService;
                _guildService = guildService;

                _resources = resources;
            }

            public IResponse ProcessCommand(Request req, List<string> args)
            {
                if (args.Count > 0)
                {
                    var matchingResource = _resources.Where(resource => resource.CommandReference == args[0]).FirstOrDefault();

                    if (matchingResource != null)
                    {
                        string section = args.ElementAtOrDefault(1) == null ? "" : args.ElementAtOrDefault(1);
                        int page = 0;

                        if (args.Count > 2)
                        {
                            try
                            {
                                page = int.Parse(args[2]);
                            }
                            catch
                            {
                                page = 0;
                            }
                        }

                        var pages = new Utils().EmbedifyResource(matchingResource, section);

                        if (pages != null)
                        {
                            return new CommandResponse
                            {
                                OK = true,
                                Pages = new Utils().EmbedifyResource(matchingResource, section),
                                LogStatement = $"/resource {matchingResource.CommandReference}{(section.Length > 0 ? $" {section}" : section)}"
                            };
                        }
                    }
                }

                // TODO(SeraphimRP): Use a proper fallback if possible.
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

                var canons = _resources.Where(res => res.Type == ResourceType.CANONS);
                var canonsList = "";

                foreach (var canon in canons)
                {
                    canonsList += $"**{canon.CommandReference}** - {canon.Title}\n";
                }


                var resp = $"**__Creeds__**\n" + creedsList.Substring(0, creedsList.Length - 1) +
                "\n\n**__Catechisms__**\n" + catechismsList.Substring(0, catechismsList.Length - 1) +
                "\n\n**__Canon Laws__**\n" + canonsList.Substring(0, catechismsList.Length - 1) +
                "\n\nTo use a resource, do `/resource resource: <name>`.\nFor example, `/resource resource: nicene` or `/resource resource: ccc range: 1`.";


                return new CommandResponse
                {
                    OK = true,
                    Pages = new List<InternalEmbed>
                    {
                        new Utils().Embedify("/resource", resp, false)
                    },
                    LogStatement = "/resource"
                };
            }
        }
    }
}
