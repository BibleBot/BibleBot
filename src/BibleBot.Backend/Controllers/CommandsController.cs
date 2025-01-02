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
            _bibleProviders =
            [
                bgProvider,
                abProvider
            ];

            _commandGroups =
            [
                new CommandGroups.Information.InformationCommandGroup(_userService, _guildService, _versionService, _frontendStatsService),
                new CommandGroups.Settings.FormattingCommandGroup(_userService, _guildService),
                new CommandGroups.Settings.VersionCommandGroup(_userService, _guildService, _versionService, _nameFetchingService),
                new CommandGroups.Resources.ResourceCommandGroup(_userService, _guildService, _resourceService.GetAllResources()),
                new CommandGroups.Verses.DailyVerseCommandGroup(_userService, _guildService, _versionService, _spProvider, _bibleProviders),
                new CommandGroups.Verses.RandomVerseCommandGroup(_userService, _guildService, _versionService, _spProvider, _bibleProviders),
                new CommandGroups.Verses.SearchCommandGroup(_userService, _guildService, _versionService, _bibleProviders),
                new CommandGroups.Staff.StaffOnlyCommandGroup()
            ];
        }

        /// <summary>
        /// Processes a message to locate verse references, outputting
        /// the corresponding text.
        /// </summary>
        /// <param name="req">A <see cref="Request" /> object</param>
        /// <response code="200">Returns the corresponding text</response>
        /// <response code="400">If <paramref name="req"/> is invalid</response>
        /// <response code="403">If <paramref name="req"/>.Token is invalid</response>
        [Route("process")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IResponse>> ProcessMessage([FromBody] Request req)
        {
            if (req.Token != Environment.GetEnvironmentVariable("ENDPOINT_TOKEN"))
            {
                return new ObjectResult(new CommandResponse
                {
                    OK = false,
                    Pages = null,
                    LogStatement = null
                })
                {
                    StatusCode = 403
                };
            }

            IResponse response;
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
                        string[] staffIds = [
                            "186046294286925824", "270590533880119297", "304602975446499329", // directors
                            "394261640335327234", "1029302033993433130", "842427954263752724" // support specialists
                        ];

                        if (grp.IsStaffOnly && !staffIds.Contains(req.UserId))
                        {
                            return BadRequest(new CommandResponse
                            {
                                OK = false,
                                Pages =
                                [
                                    Utils.GetInstance().Embedify("Permissions Error", "This command can only be performed by BibleBot staff.", true)
                                ],
                                LogStatement = $"Insufficient permissions on +{grp.Name}."
                            });
                        }

                        // TODO: this logic could be simplified. it wouldn't necessarily optimize anything (nor the opposite),
                        // but it would look visually cleaner
                        if (tokenizedBody.Length > 1)
                        {
                            ICommand idealCommand = grp.Commands.FirstOrDefault(cmd => cmd.Name == tokenizedBody[1]);

                            if (idealCommand != null)
                            {
                                response = await idealCommand.ProcessCommand(req, tokenizedBody.Skip(2).ToList());
                                return response.OK ? Ok(response) : BadRequest(response);
                            }
                            else if (grp.Name is "resource" or "search")
                            {
                                response = await grp.DefaultCommand.ProcessCommand(req, tokenizedBody.Skip(1).ToList());
                                return response.OK ? Ok(response) : BadRequest(response);
                            }
                        }

                        response = await grp.DefaultCommand.ProcessCommand(req, []);
                        return response.OK ? Ok(response) : BadRequest(response);
                    }
                    else
                    {
                        // TODO: this logic could be simplified with above TODO also
                        ICommand cmd = _commandGroups.FirstOrDefault(grp => grp.Name == "info").Commands.FirstOrDefault(cmd => cmd.Name == potentialCommand.Substring(1));

                        if (cmd != null)
                        {
                            response = await cmd.ProcessCommand(req, []);
                            return response.OK ? Ok(response) : BadRequest(response);
                        }
                    }
                }
            }

            return BadRequest(new CommandResponse
            {
                OK = false,
                Pages = null,
                LogStatement = null
            });
        }
    }
}
