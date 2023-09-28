/*
* Copyright (C) 2016-2023 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BibleBot.Backend.Services;
using BibleBot.Backend.Services.Providers;
using BibleBot.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BibleBot.Backend.Controllers
{
    [Produces("application/json")]
    [Route("api/commands")]
    [ApiController]
    public class CommandsController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly GuildService _guildService;
        private readonly VersionService _versionService;
        private readonly ResourceService _resourceService;
        private readonly FrontendStatsService _frontendStatsService;
        private readonly NameFetchingService _nameFetchingService;

        private readonly List<IBibleProvider> _bibleProviders;
        private readonly SpecialVerseProvider _spProvider;

        private readonly List<ICommandGroup> _commandGroups;

        public CommandsController(UserService userService, GuildService guildService, VersionService versionService, ResourceService resourceService,
                                  FrontendStatsService frontendStatsService, NameFetchingService nameFetchingService, SpecialVerseProvider spProvider, BibleGatewayProvider bgProvider, APIBibleProvider abProvider)
        {
            _userService = userService;
            _guildService = guildService;
            _versionService = versionService;
            _resourceService = resourceService;
            _frontendStatsService = frontendStatsService;
            _nameFetchingService = nameFetchingService;

            _spProvider = spProvider;
            _bibleProviders = new List<IBibleProvider>
            {
                bgProvider,
                abProvider
            };

            _commandGroups = new List<ICommandGroup>
            {
                new CommandGroups.Information.InformationCommandGroup(_userService, _guildService, _versionService, _frontendStatsService),
                new CommandGroups.Settings.FormattingCommandGroup(_userService, _guildService),
                new CommandGroups.Settings.VersionCommandGroup(_userService, _guildService, _versionService, _nameFetchingService),
                new CommandGroups.Resources.ResourceCommandGroup(_userService, _guildService, _resourceService.GetAllResources()),
                new CommandGroups.Verses.DailyVerseCommandGroup(_userService, _guildService, _versionService, _spProvider, _bibleProviders),
                new CommandGroups.Verses.RandomVerseCommandGroup(_userService, _guildService, _versionService, _spProvider, _bibleProviders),
                new CommandGroups.Verses.SearchCommandGroup(_userService, _guildService, _versionService, _bibleProviders),
                new CommandGroups.Staff.StaffOnlyCommandGroup()
            };
        }

        /// <summary>
        /// Processes a message to locate verse references, outputting
        /// the corresponding text.
        /// </summary>
        /// <param name="req">A BibleBot.Backend.Request object</param>
        /// <response code="200">Returns the corresponding text</response>
        /// <response code="400">If req is invalid</response>
        [Route("process")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IResponse> ProcessMessage([FromBody] Request req)
        {
            if (req.Token != Environment.GetEnvironmentVariable("ENDPOINT_TOKEN"))
            {
                return new CommandResponse
                {
                    OK = false,
                    Pages = null,
                    LogStatement = null
                };
            }

            string[] tokenizedBody = req.Body.Split(" ");

            if (tokenizedBody.Length > 0)
            {
                string potentialCommand = tokenizedBody[0];
                string prefix = "+";

                if (potentialCommand.StartsWith(prefix))
                {
                    ICommandGroup grp = _commandGroups.FirstOrDefault(grp => grp.Name == potentialCommand.Substring(1));

                    if (grp != null)
                    {
                        if (grp.IsStaffOnly && req.UserId != "186046294286925824")
                        {
                            return new CommandResponse
                            {
                                OK = false,
                                Pages = new List<InternalEmbed>
                                {
                                    Utils.GetInstance().Embedify("Permissions Error", "This command can only be performed by BibleBot staff", true)
                                }
                            };
                        }

                        if (tokenizedBody.Length > 1)
                        {
                            ICommand idealCommand = grp.Commands.FirstOrDefault(cmd => cmd.Name == tokenizedBody[1]);

                            if (idealCommand != null)
                            {
                                // if (idealCommand.PermissionsRequired != null)
                                // {
                                //     foreach (Permissions permission in idealCommand.PermissionsRequired)
                                //     {
                                //         if ((req.UserPermissions & (long)permission) != (long)permission)
                                //         {
                                //             return new CommandResponse
                                //             {
                                //                 OK = false,
                                //                 Pages = new List<InternalEmbed>
                                //                 {
                                //                     Utils.GetInstance().Embedify("Insufficient Permissions", "You do not have the required permissions to use this command.", true)
                                //                 },
                                //                 LogStatement = $"Insufficient permissions on +{grp.Name} {idealCommand.Name}."
                                //             };
                                //         }
                                //     }
                                // }

                                if (req.IsBot && !idealCommand.BotAllowed)
                                {
                                    return new CommandResponse
                                    {
                                        OK = false,
                                        Pages = new List<InternalEmbed>
                                            {
                                                Utils.GetInstance().Embedify("Insufficient Permissions", "Bots are not permitted to use this command, please inform your nearest human.", true)
                                            },
                                        LogStatement = $"Bot can't use +{grp.Name} {idealCommand.Name}."
                                    };
                                }

                                var commandArgs = tokenizedBody.Skip(2).ToList();
#pragma warning disable IDE0045
                                if (commandArgs.Count() < idealCommand.ExpectedArguments)
                                {
                                    return new CommandResponse
                                    {
                                        OK = false,
                                        Pages = new List<InternalEmbed>
                                        {
                                            Utils.GetInstance().Embedify("Insufficient Parameters", idealCommand.ArgumentsError, true)
                                        },
                                        LogStatement = $"Insufficient parameters on +{grp.Name} {idealCommand.Name}."
                                    };
                                }
#pragma warning restore IDE0045
                                return await idealCommand.ProcessCommand(req, tokenizedBody.Skip(2).ToList());
                            }
                            else if (grp.Name is "resource" or "search")
                            {
                                return await grp.DefaultCommand.ProcessCommand(req, tokenizedBody.Skip(1).ToList());
                            }
                        }

                        return await grp.DefaultCommand.ProcessCommand(req, new List<string>());
                    }
                    else
                    {
                        ICommand cmd = _commandGroups.FirstOrDefault(grp => grp.Name == "info").Commands.FirstOrDefault(cmd => cmd.Name == potentialCommand.Substring(1));

                        if (cmd != null)
                        {
                            return await cmd.ProcessCommand(req, new List<string>());
                        }
                    }
                }
            }

            return new CommandResponse
            {
                OK = false,
                Pages = null,
                LogStatement = null
            };
        }
    }
}
