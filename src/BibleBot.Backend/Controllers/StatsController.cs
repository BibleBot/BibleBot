/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Linq;
using System.Threading.Tasks;
using BibleBot.Backend.Services;
using BibleBot.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace BibleBot.Backend.Controllers
{
    [Produces("application/json")]
    [Route("api/stats")]
    [AutoValidateAntiforgeryToken]
    [ApiController]
    public class StatsController(FrontendStatsService frontendStatsService) : ControllerBase
    {

        /// <summary>
        /// Processes a message to update stats from frontend.
        /// </summary>
        /// <param name="req">A <see cref="Request" /> object</param>
        /// <response code="200">Returns BibleBot.Backend.CommandResponse</response>
        /// <response code="400">If <paramref name="req"/> is invalid</response>
        /// <response code="403">If <paramref name="req"/>.Token is invalid</response>
        [Route("process")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<CommandResponse>> ProcessMessage([FromBody] Request req)
        {
            if (req.Token != Environment.GetEnvironmentVariable("ENDPOINT_TOKEN"))
            {
                return new ObjectResult(new CommandResponse
                {
                    OK = false
                })
                {
                    StatusCode = 403
                };
            }

            FrontendStats stats = (await frontendStatsService.Get()).First();
            string[] fields = req.Body.Split("||");

            if (stats != null)
            {
                UpdateDefinition<FrontendStats> update = Builders<FrontendStats>.Update
                             .Set(stats => stats.ShardCount, int.Parse(fields[0]))
                             .Set(stats => stats.ServerCount, int.Parse(fields[1]))
                             .Set(stats => stats.UserCount, int.Parse(fields[2]))
                             .Set(stats => stats.ChannelCount, int.Parse(fields[3]))
                             .Set(stats => stats.FrontendRepoCommitHash, fields[4]);

                await frontendStatsService.Update(stats, update);
            }
            else
            {
                await frontendStatsService.Create(new FrontendStats
                {
                    ShardCount = int.Parse(fields[0]),
                    ServerCount = int.Parse(fields[1]),
                    UserCount = int.Parse(fields[2]),
                    ChannelCount = int.Parse(fields[3]),
                    FrontendRepoCommitHash = fields[4]
                });
            }

            return Ok(new CommandResponse
            {
                OK = true
            });
        }
    }
}
