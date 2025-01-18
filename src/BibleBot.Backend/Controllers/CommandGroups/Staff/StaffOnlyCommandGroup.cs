/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BibleBot.Models;

namespace BibleBot.Backend.Controllers.CommandGroups.Staff
{
    public class StaffOnlyCommandGroup : CommandGroup
    {
        public override string Name { get => "staff"; set => throw new NotImplementedException(); }
        public override bool IsStaffOnly { get => true; set => throw new NotImplementedException(); }
        public override Command DefaultCommand { get => Commands.FirstOrDefault(cmd => cmd.Name == "announce"); set => throw new NotImplementedException(); }
        public override List<Command> Commands { get => [new StaffAnnounce(), new StaffPermissionsCheck()]; set => throw new NotImplementedException(); }

        public class StaffAnnounce : Command
        {
            public override string Name { get => "announce"; set => throw new NotImplementedException(); }

            public override Task<IResponse> ProcessCommand(Request req, List<string> args) => Task.FromResult<IResponse>(new CommandResponse
            {
                OK = true,
                Pages =
                [
                    Utils.GetInstance().Embedify("BibleBot Announcement", string.Join(" ", args), false)
                ],
                LogStatement = "/announce",
                SendAnnouncement = true
            });
        }

        public class StaffPermissionsCheck : Command
        {
            public override string Name { get => "permscheck"; set => throw new NotImplementedException(); }

            public override Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                StringBuilder[] results = Utils.GetInstance().PermissionsChecker(long.Parse(args[2]), long.Parse(args[3]), long.Parse(args[4]));

                InternalEmbed embed = Utils.GetInstance().Embedify("Permissions Check", $"This is a command for support use.\n\n**Channel ID**: {args[0]}\n**Server ID**: {args[1]}\n**Integrated Role (IR)**: {args[5]} ({args[6]})", false);
                embed.Fields =
                [
                    new()
                    {
                        Name = "Bot User Channel Permissions",
                        Value = results[0].ToString()
                    },
                    new()
                    {
                        Name = "IR Channel Permissions",
                        Value = results[1].ToString()
                    },
                    new()
                    {
                        Name = "IR Server Permissions",
                        Value = results[2].ToString()
                    }
                ];

                return Task.FromResult<IResponse>(new CommandResponse
                {
                    OK = true,
                    Pages =
                    [
                        embed
                    ],
                    LogStatement = "/permscheck",
                    SendAnnouncement = false
                });
            }
        }
    }
}