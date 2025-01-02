/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BibleBot.Backend.Services;
using BibleBot.Models;

namespace BibleBot.Backend.Controllers.CommandGroups.Resources
{
    public class ResourceCommandGroup : ICommandGroup
    {
        public string Name { get; set; }
        public bool IsStaffOnly { get; set; }
        public ICommand DefaultCommand { get; set; }
        public List<ICommand> Commands { get; set; }

        private readonly List<IResource> _resources;

        public ResourceCommandGroup(UserService userService, GuildService guildService, List<IResource> resources)
        {
            _resources = resources;

            Name = "resource";
            IsStaffOnly = false;
            Commands =
            [
                new ResourceUsage(_resources),
            ];
            DefaultCommand = Commands.FirstOrDefault(cmd => cmd.Name == "usage");
        }

        public class ResourceUsage(List<IResource> resources) : ICommand
        {
            public string Name { get; set; } = "usage";
            public string ArgumentsError { get; set; } = null;
            public int ExpectedArguments { get; set; } = 0;
            public List<Permissions> PermissionsRequired { get; set; } = null;
            public bool BotAllowed { get; set; } = true;

            private readonly List<IResource> _resources = resources;

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
                StringBuilder creedsList = new();

                foreach (IResource creed in creeds)
                {
                    creedsList.Append($"**{creed.CommandReference}** - {creed.Title}\n");
                }

                IEnumerable<IResource> catechisms = _resources.Where(res => res.Type == ResourceType.CATECHISM);
                StringBuilder catechismsList = new();

                foreach (IResource catechism in catechisms)
                {
                    catechismsList.Append($"**{catechism.CommandReference}** - {catechism.Title}\n");
                }

                IEnumerable<IResource> canons = _resources.Where(res => res.Type == ResourceType.CANONS);
                StringBuilder canonsList = new();

                foreach (IResource canon in canons)
                {
                    canonsList.Append($"**{canon.CommandReference}** - {canon.Title}\n");
                }


                string resp = $"**__Creeds__**\n" + creedsList.ToString().Substring(0, creedsList.Length - 1) +
                "\n\n**__Catechisms__**\n" + catechismsList.ToString().Substring(0, catechismsList.Length - 1) +
                "\n\n**__Canon Laws__**\n" + canonsList.ToString().Substring(0, catechismsList.Length - 1) +
                "\n\nTo use a resource, do `/resource resource:<name>`.\nFor example, `/resource resource:nicene` or `/resource resource:ccc range:1`.";


                return Task.FromResult<IResponse>(new CommandResponse
                {
                    OK = true,
                    Pages =
                    [
                        Utils.GetInstance().Embedify("/resource", resp, false)
                    ],
                    LogStatement = "/resource"
                });
            }
        }
    }
}
