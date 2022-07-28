/*
* Copyright (C) 2016-2022 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BibleBot.Backend.Models;
using BibleBot.Backend.Services;
using BibleBot.Backend.Services.Providers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BibleBot.Backend.Controllers
{
    [Produces("application/json")]
    [Route("api/stats")]
    [ApiController]
    public class StatsController : ControllerBase
    {
        private readonly FrontendStatsService _frontendStatsService;

        public StatsController(FrontendStatsService frontendStatsService)
        {
            _frontendStatsService = frontendStatsService;
        }

        /// <summary>
        /// Processes a message to update stats from frontend.
        /// </summary>
        /// <param name="req">A BibleBot.Lib.Request object</param>
        /// <response code="200">Returns BibleBot.Lib.CommandResponse</response>
        /// <response code="400">If req is invalid</response>
        [Route("process")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public CommandResponse ProcessMessage([FromBody] Request req)
        {
            if (req.Token != Environment.GetEnvironmentVariable("ENDPOINT_TOKEN"))
            {
                return new CommandResponse
                {
                    OK = false
                };
            }

            var stats = _frontendStatsService.Get();
            var fields = req.Body.Split("||");

            if (stats != null)
            {
                stats.ShardCount = int.Parse(fields[0]);
                stats.ServerCount = int.Parse(fields[1]);
                stats.UserCount = int.Parse(fields[2]);
                stats.ChannelCount = int.Parse(fields[3]);

                _frontendStatsService.Update(stats);
            }
            else
            {
                _frontendStatsService.Create(new FrontendStats
                {
                    ShardCount = int.Parse(fields[0]),
                    ServerCount = int.Parse(fields[1]),
                    UserCount = int.Parse(fields[2]),
                    ChannelCount = int.Parse(fields[3])
                });
            }

            return new CommandResponse
            {
                OK = true
            };
        }
    }
}
