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
using BibleBot.Models;

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
            DefaultCommand = Commands.FirstOrDefault(cmd => cmd.Name == "usage");
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

            public Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                if (args.Count > 0)
                {
                    IResource matchingResource = _resources.FirstOrDefault(resource => resource.CommandReference == args[0]);

                    if (matchingResource != null)
                    {
                        string section = args.ElementAtOrDefault(1) ?? "";
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

                        List<InternalEmbed> pages = Utils.GetInstance().EmbedifyResource(matchingResource, section);

                        if (pages != null)
                        {
                            return Task.FromResult<IResponse>(new CommandResponse
                            {
                                OK = true,
                                Pages = Utils.GetInstance().EmbedifyResource(matchingResource, section),
                                LogStatement = $"/resource {matchingResource.CommandReference}{(section.Length > 0 ? $" {section}" : section)}"
                            });
                        }
                    }
                }

                // TODO(SeraphimRP): Use a proper fallback if possible.
                IEnumerable<IResource> creeds = _resources.Where(res => res.Type == ResourceType.CREED);
                string creedsList = "";

                foreach (IResource creed in creeds)
                {
                    creedsList += $"**{creed.CommandReference}** - {creed.Title}\n";
                }

                IEnumerable<IResource> catechisms = _resources.Where(res => res.Type == ResourceType.CATECHISM);
                string catechismsList = "";

                foreach (IResource catechism in catechisms)
                {
                    catechismsList += $"**{catechism.CommandReference}** - {catechism.Title}\n";
                }

                IEnumerable<IResource> canons = _resources.Where(res => res.Type == ResourceType.CANONS);
                string canonsList = "";

                foreach (IResource canon in canons)
                {
                    canonsList += $"**{canon.CommandReference}** - {canon.Title}\n";
                }


                string resp = $"**__Creeds__**\n" + creedsList.Substring(0, creedsList.Length - 1) +
                "\n\n**__Catechisms__**\n" + catechismsList.Substring(0, catechismsList.Length - 1) +
                "\n\n**__Canon Laws__**\n" + canonsList.Substring(0, catechismsList.Length - 1) +
                "\n\nTo use a resource, do `/resource resource:<name>`.\nFor example, `/resource resource:nicene` or `/resource resource:ccc range:1`.";


                return Task.FromResult<IResponse>(new CommandResponse
                {
                    OK = true,
                    Pages = new List<InternalEmbed>
                    {
                        Utils.GetInstance().Embedify("/resource", resp, false)
                    },
                    LogStatement = "/resource"
                });
            }
        }
    }
}
