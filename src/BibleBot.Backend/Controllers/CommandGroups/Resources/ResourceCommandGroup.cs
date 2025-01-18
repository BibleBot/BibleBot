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
using BibleBot.Models;

namespace BibleBot.Backend.Controllers.CommandGroups.Resources
{
    public class ResourceCommandGroup(List<IResource> resources) : CommandGroup
    {
        public override string Name { get => "resource"; set { } }
        public override Command DefaultCommand { get => Commands.FirstOrDefault(cmd => cmd.Name == "usage"); set { } }
        public override List<Command> Commands { get => [new ResourceUsage(resources)]; set { } }

        public class ResourceUsage(List<IResource> resources) : Command
        {
            public override string Name { get => "usage"; set { } }

            public override Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                if (args.Count > 0)
                {
                    IResource matchingResource = resources.FirstOrDefault(resource => resource.CommandReference == args[0]);

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
                IEnumerable<IResource> creeds = resources.Where(res => res.Type == ResourceType.CREED);
                StringBuilder creedsList = new();

                foreach (IResource creed in creeds)
                {
                    creedsList.Append($"**{creed.CommandReference}** - {creed.Title}\n");
                }

                IEnumerable<IResource> catechisms = resources.Where(res => res.Type == ResourceType.CATECHISM);
                StringBuilder catechismsList = new();

                foreach (IResource catechism in catechisms)
                {
                    catechismsList.Append($"**{catechism.CommandReference}** - {catechism.Title}\n");
                }

                IEnumerable<IResource> canons = resources.Where(res => res.Type == ResourceType.CANONS);
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
