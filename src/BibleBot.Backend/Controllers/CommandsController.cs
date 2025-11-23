/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BibleBot.Backend.Controllers.CommandGroups;
using BibleBot.Backend.Services;
using BibleBot.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sentry;

namespace BibleBot.Backend.Controllers
{
    [Produces("application/json")]
    [Route("api/commands")]
    [ApiController]
    public class CommandsController(UserService userService, GuildService guildService, VersionService versionService, ResourceService resourceService,
                              FrontendStatsService frontendStatsService, LanguageService languageService, MetadataFetchingService metadataFetchingService, SpecialVerseProcessingService specialVerseProcessingService,
                              ExperimentService experimentService, List<IContentProvider> bibleProviders, IStringLocalizerFactory localizerFactory) : ControllerBase
    {
        private readonly List<CommandGroup> _commandGroups = [
                new InformationCommandGroup(userService, guildService, versionService, frontendStatsService, localizerFactory, experimentService),
                new FormattingCommandGroup(userService, guildService, localizerFactory),
                new VersionCommandGroup(userService, guildService, versionService, metadataFetchingService, localizerFactory),
                new LanguageCommandGroup(userService, guildService, languageService, localizerFactory),
                new ResourceCommandGroup(resourceService, localizerFactory),
                new DailyVerseCommandGroup(userService, guildService, versionService, specialVerseProcessingService, localizerFactory),
                new RandomVerseCommandGroup(userService, guildService, versionService, specialVerseProcessingService, localizerFactory),
                new SearchCommandGroup(userService, guildService, versionService, metadataFetchingService, bibleProviders, localizerFactory),
                new KDStaffCommandGroup(versionService, languageService, experimentService)
            ];

        private readonly IStringLocalizer _localizer = localizerFactory.Create(typeof(CommandsController));

        /// <summary>
        /// Processes a message to locate verse references, outputting
        /// the corresponding text.
        /// </summary>
        /// <param name="req">A <see cref="Request" /> object</param>
        /// <response code="200">Returns the corresponding text</response>
        /// <response code="400">If <paramref name="req"/> is invalid</response>
        [Route("process")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IResponse>> ProcessMessage([FromBody] Request req)
        {
            req.ActiveExperiments = await experimentService.GetExperimentVariantsForUser(req.UserId);

            SentrySdk.ConfigureScope(scope =>
            {
                scope.Contexts["request"] = req;
            });

            bool isUserOptOut = (await userService.Get(req.UserId)) is User user && user.IsOptOut;

            if (isUserOptOut || req.Body is null || req.Body.Length == 0)
            {
                return BadRequest(new CommandResponse
                {
                    OK = false,
                    Pages = null,
                    LogStatement = null
                });
            }

            IResponse response;
            string[] tokenizedBody = req.Body.Split(" ");

            if (tokenizedBody.Length <= 0)
            {
                return BadRequest(new CommandResponse
                {
                    OK = false,
                    Pages = null,
                    LogStatement = null
                });
            }
            string potentialCommand = tokenizedBody[0][1..]; // trim the prefix off

            CommandGroup grp = _commandGroups.FirstOrDefault(grp => grp.Name == potentialCommand, _commandGroups.First(grp => grp.Name == "info"));

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
                        Utils.GetInstance().Embedify(_localizer["PermissionsErrorTitle"], _localizer["StaffOnlyCommandError"], true)
                    ],
                    LogStatement = $"Insufficient permissions on +{grp.Name}.",
                    Culture = CultureInfo.CurrentUICulture.Name
                });
            }

            if (tokenizedBody.Length == 1)
            {
                Command idealCommand = grp.Commands.FirstOrDefault(cmd => cmd.Name == potentialCommand);

                if (idealCommand != null)
                {
                    response = await idealCommand.ProcessCommand(req, []);
                    return response.OK ? Ok(response) : BadRequest(response);
                }
            }
            else
            {
                Command idealCommand = grp.Commands.FirstOrDefault(cmd => cmd.Name == tokenizedBody[1]);

                if (idealCommand != null)
                {
                    response = await idealCommand.ProcessCommand(req, [.. tokenizedBody.Skip(2)]);
                    return response.OK ? Ok(response) : BadRequest(response);
                }

                if (grp.Name is "resource" or "search")
                {
                    response = await grp.DefaultCommand.ProcessCommand(req, [.. tokenizedBody.Skip(1)]);
                    return response.OK ? Ok(response) : BadRequest(response);
                }
            }

            response = await grp.DefaultCommand.ProcessCommand(req, []);
            return response.OK ? Ok(response) : BadRequest(response);

        }
    }
}
