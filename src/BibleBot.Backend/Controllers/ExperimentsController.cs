/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using BibleBot.Backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BibleBot.Backend.Controllers
{
    [Produces("application/json")]
    [Route("api/experiments")]
    [ApiController]
    public class ExperimentsController(ExperimentService experimentService) : ControllerBase
    {
        private readonly ExperimentService _experimentService = experimentService;

        [Route("active_user")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetActiveExperimentsForUser([FromQuery(Name = "user_id")] string userId, [FromQuery(Name = "frontend")] bool frontend = false) =>
            frontend
                ? Ok(_experimentService.GetFrontendExperimentVariantsForUser(userId))
                : Ok(_experimentService.GetExperimentVariantsForUser(userId));

        [Route("active_guild")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetActiveExperimentsForGuild([FromQuery(Name = "guild_id")] string guildId, [FromQuery(Name = "frontend")] bool frontend = false) =>
            frontend
                ? Ok(_experimentService.GetFrontendExperimentVariantsForGuild(guildId))
                : Ok(_experimentService.GetExperimentVariantsForGuild(guildId));
    }
}
