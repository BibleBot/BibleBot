/*
* Copyright (C) 2016-2026 Kerygma Digital Co.
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
    [Route("api/stats")]
    [ApiController]
    public class StatsController(FrontendStatsService frontendStatsService) : ControllerBase
    {

        /// <summary>
        /// Processes a message to update stats from frontend.
        /// </summary>
        /// <param name="req">A <see cref="Request" /> object</param>
        /// <response code="200">Returns BibleBot.Backend.CommandResponse</response>
        /// <response code="400">If <paramref name="req"/> is invalid</response>
        [Route("process")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CommandResponse>> ProcessMessage([FromBody] Request req)
        {
            FrontendStats stats = (await frontendStatsService.Get()).FirstOrDefault();
            string[] fields = req.Body.Split("||");

            if (stats != null)
            {
                List<UpdateDef<FrontendStats>> update =
                [
                    UpdateDef<FrontendStats>.Set(statsToUpdate => statsToUpdate.ShardCount, int.Parse(fields[0])),
                    UpdateDef<FrontendStats>.Set(statsToUpdate => statsToUpdate.ServerCount, int.Parse(fields[1])),
                    UpdateDef<FrontendStats>.Set(statsToUpdate => statsToUpdate.UserCount, int.Parse(fields[2])),
                    UpdateDef<FrontendStats>.Set(statsToUpdate => statsToUpdate.ChannelCount, int.Parse(fields[3])),
                    UpdateDef<FrontendStats>.Set(statsToUpdate => statsToUpdate.UserInstallCount, int.Parse(fields[4])),
                    UpdateDef<FrontendStats>.Set(statsToUpdate => statsToUpdate.FrontendRepoCommitHash, fields[5])
                ];

                await frontendStatsService.Update(stats, update.Combine());
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
