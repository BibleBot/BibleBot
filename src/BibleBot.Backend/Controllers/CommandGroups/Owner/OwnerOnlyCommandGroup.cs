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
using BibleBot.Models;

namespace BibleBot.Backend.Controllers.CommandGroups.Owner
{
    public class OwnerOnlyCommandGroup : ICommandGroup
    {
        public string Name { get; set; }
        public bool IsOwnerOnly { get; set; }
        public ICommand DefaultCommand { get; set; }
        public List<ICommand> Commands { get; set; }

        public OwnerOnlyCommandGroup()
        {
            Name = "owner";
            IsOwnerOnly = true;
            Commands = new List<ICommand>
            {
                new OwnerAnnounce()
            };
            DefaultCommand = Commands.FirstOrDefault(cmd => cmd.Name == "announce");
        }

        public class OwnerAnnounce : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }
            public bool BotAllowed { get; set; }

            public OwnerAnnounce()
            {
                Name = "announce";
                ArgumentsError = null;
                ExpectedArguments = 0;
                PermissionsRequired = null;
                BotAllowed = false; // anti-spam measure
            }

            public Task<IResponse> ProcessCommand(Request req, List<string> args) => Task.FromResult<IResponse>(new CommandResponse
            {
                OK = true,
                Pages = new List<InternalEmbed>
                    {
                        Utils.GetInstance().Embedify("BibleBot Announcement", string.Join(" ", args), false)
                    },
                LogStatement = "/announce",
                SendAnnouncement = true
            });
        }
    }
}
