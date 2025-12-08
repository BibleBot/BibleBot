/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BibleBot.Backend.Services;
using BibleBot.Models;
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
        public async Task<IActionResult> GetActiveExperimentsForUser([FromQuery(Name = "user_id")] string userId, [FromQuery(Name = "frontend")] bool frontend = false)
        {
            Dictionary<Experiment, string> experiments = frontend
                ? await _experimentService.GetFrontendExperimentVariantsForUser(userId)
                : await _experimentService.GetExperimentVariantsForUser(userId);

            return Ok(experiments.ToDictionary(x => x.Key.Name, x => x.Value));
        }

        [Route("active_guild")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActiveExperimentsForGuild([FromQuery(Name = "guild_id")] string guildId, [FromQuery(Name = "frontend")] bool frontend = false)
        {
            Dictionary<Experiment, string> experiments = frontend
                ? await _experimentService.GetFrontendExperimentVariantsForGuild(guildId)
                : await _experimentService.GetExperimentVariantsForGuild(guildId);

            return Ok(experiments.ToDictionary(x => x.Key.Name, x => x.Value));
        }

        [Route("helped")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Helped([FromQuery(Name = "experiment_name")] string experimentName, [FromQuery(Name = "user_id")] string userId)
        {
            await _experimentService.Helped(experimentName, userId);
            return Ok();
        }

        [Route("did_not_help")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DidNotHelp([FromQuery(Name = "experiment_name")] string experimentName, [FromQuery(Name = "user_id")] string userId)
        {
            await _experimentService.DidNotHelp(experimentName, userId);
            return Ok();
        }

        [Route("feedback_exists")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> FeedbackExists([FromQuery(Name = "experiment_name")] string experimentName, [FromQuery(Name = "user_id")] string userId)
        {
            Experiment experiment = await _experimentService.Get(experimentName);

            return Ok(experiment.Helped.Contains(userId) || experiment.DidNotHelp.Contains(userId));
        }
    }
}
