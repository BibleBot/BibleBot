/*
* Copyright (C) 2016-2023 Kerygma Digital Co.
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

namespace BibleBot.Backend.Controllers.CommandGroups.Staff
{
    public class StaffOnlyCommandGroup : ICommandGroup
    {
        public string Name { get; set; }
        public bool IsStaffOnly { get; set; }
        public ICommand DefaultCommand { get; set; }
        public List<ICommand> Commands { get; set; }

        public StaffOnlyCommandGroup()
        {
            Name = "staff";
            IsStaffOnly = true;
            Commands = new List<ICommand>
            {
                new StaffAnnounce(),
                new StaffPermissionsCheck()
            };
            DefaultCommand = Commands.FirstOrDefault(cmd => cmd.Name == "announce");
        }

        public class StaffAnnounce : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }
            public bool BotAllowed { get; set; }

            public StaffAnnounce()
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

        public class StaffPermissionsCheck : ICommand
        {
            public string Name { get; set; }
            public string ArgumentsError { get; set; }
            public int ExpectedArguments { get; set; }
            public List<Permissions> PermissionsRequired { get; set; }
            public bool BotAllowed { get; set; }

            public StaffPermissionsCheck()
            {
                Name = "permscheck";
                ArgumentsError = null;
                ExpectedArguments = 2;
                PermissionsRequired = null;
                BotAllowed = false; // anti-spam measure
            }

            public Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                StringBuilder[] results = Utils.GetInstance().PermissionsChecker(long.Parse(args[0]), long.Parse(args[1]));

                InternalEmbed embed = Utils.GetInstance().Embedify("Permissions Check", $"Integrated Role: {args[2]} ({args[3]})", false);
                embed.Fields = new List<EmbedField>
                {
                    new()
                    {
                        Name = "Channel Permissions",
                        Value = results[0].ToString()
                    },
                    new()
                    {
                        Name = "Guild Permissions",
                        Value = results[1].ToString()
                    }
                };

                return Task.FromResult<IResponse>(new CommandResponse
                {
                    OK = true,
                    Pages = new List<InternalEmbed>
                    {
                        embed
                    },
                    LogStatement = "/permscheck",
                    SendAnnouncement = true
                });
            }
        }
    }
}