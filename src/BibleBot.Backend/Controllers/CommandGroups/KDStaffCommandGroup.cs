/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BibleBot.Backend.Services;
using BibleBot.Models;

namespace BibleBot.Backend.Controllers.CommandGroups
{
    public class KDStaffCommandGroup(VersionService versionService, LanguageService languageService, ExperimentService experimentService) : CommandGroup
    {
        public override string Name { get => "staff"; set => throw new NotImplementedException(); }
        public override bool IsStaffOnly { get => true; set => throw new NotImplementedException(); }
        public override Command DefaultCommand { get => Commands.FirstOrDefault(cmd => cmd.Name == "announce"); set => throw new NotImplementedException(); }
        public override List<Command> Commands { get => [new StaffAnnounce(), new StaffPermissionsCheck(), new StaffReloadVersions(versionService), new StaffReloadLanguages(languageService), new StaffReloadExperiments(experimentService)]; set => throw new NotImplementedException(); }

        private class StaffAnnounce : Command
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
                SendAnnouncement = true,
                Culture = CultureInfo.CurrentUICulture.Name
            });
        }

        private class StaffPermissionsCheck : Command
        {
            public override string Name { get => "permscheck"; set => throw new NotImplementedException(); }

            public override Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                StringBuilder[] results = Utils.GetInstance().PermissionsChecker(long.Parse(args[2]), long.Parse(args[3]), long.Parse(args[4]));

                InternalEmbed embed = Utils.GetInstance().Embedify("Permissions Check", $"This is a command for support use.\n\n**Channel ID**: {args[0]}\n**Server ID**: {args[1]}\n**Integrated Role (IR)**: {args[5]} ({args[6]})", false);
                embed.Fields =
                [
                    new EmbedField
                    {
                        Name = "Bot User Channel Permissions",
                        Value = results[0].ToString()
                    },
                    new EmbedField
                    {
                        Name = "IR Channel Permissions",
                        Value = results[1].ToString()
                    },
                    new EmbedField
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
                    SendAnnouncement = false,
                    Culture = CultureInfo.CurrentUICulture.Name
                });
            }
        }

        private class StaffReloadVersions(VersionService versionService) : Command
        {
            public override string Name { get => "reload_versions"; set => throw new NotImplementedException(); }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                await versionService.GetVersions(forcePull: true);

                return new CommandResponse
                {
                    OK = true,
                    LogStatement = "/reload_versions",
                    Pages =
                    [
                        Utils.GetInstance().Embedify("/reload_versions", "Versions have been reloaded.", false)
                    ],
                    SendAnnouncement = false,
                    Culture = CultureInfo.CurrentUICulture.Name
                };
            }
        }

        private class StaffReloadLanguages(LanguageService languageService) : Command
        {
            public override string Name { get => "reload_languages"; set => throw new NotImplementedException(); }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                await languageService.GetLanguages(forcePull: true);

                return new CommandResponse
                {
                    OK = true,
                    LogStatement = "/reload_languages",
                    Pages =
                    [
                        Utils.GetInstance().Embedify("/reload_languages", "Languages have been reloaded.", false)
                    ],
                    SendAnnouncement = false,
                    Culture = CultureInfo.CurrentUICulture.Name
                };
            }
        }

        private class StaffReloadExperiments(ExperimentService experimentService) : Command
        {
            public override string Name { get => "reload_experiments"; set => throw new NotImplementedException(); }

            public override async Task<IResponse> ProcessCommand(Request req, List<string> args)
            {
                await experimentService.GetExperiments(forcePull: true);

                return new CommandResponse
                {
                    OK = true,
                    LogStatement = "/reload_experiments",
                    Pages =
                    [
                        Utils.GetInstance().Embedify("/reload_experiments", "Experiments have been reloaded.", false)
                    ],
                    SendAnnouncement = false,
                    Culture = CultureInfo.CurrentUICulture.Name
                };
            }
        }
    }
}
