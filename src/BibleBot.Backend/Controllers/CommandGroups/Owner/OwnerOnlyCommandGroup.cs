/*
* Copyright (C) 2016-2021 Kerygma Digital Co.
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
using BibleBot.Lib;

namespace BibleBot.Backend.Controllers.CommandGroups.Owner
{
    public class OwnerOnlyCommandGroup : ICommandGroup
    {
        public string Name { get; set; }
        public bool IsOwnerOnly { get; set; }
        public ICommand DefaultCommand { get; set; }
        public List<ICommand> Commands { get; set; }

        private readonly UserService _userService;
        private readonly GuildService _guildService;
        private readonly VersionService _versionService;
        private readonly FrontendStatsService _frontendStatsService;

        public OwnerOnlyCommandGroup(UserService userService, GuildService guildService, VersionService versionService, FrontendStatsService frontendStatsService)
        {
            _userService = userService;
            _guildService = guildService;
            _versionService = versionService;
            _frontendStatsService = frontendStatsService;

            Name = "owner";
            IsOwnerOnly = true;
            Commands = new List<ICommand>
            {
                new OwnerAnnounce()
            };
            DefaultCommand = Commands.Where(cmd => cmd.Name == "announce").FirstOrDefault();
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

            public IResponse ProcessCommand(Request req, List<string> args)
            {
                return new CommandResponse
                {
                    OK = true,
                    Pages = new List<InternalEmbed>
                    {
                        new Utils().Embedify("BibleBot Announcement", string.Join(" ", args), false)
                    },
                    LogStatement = "/announce",
                    SendAnnouncement = true
                };
            }
        }
    }
}
