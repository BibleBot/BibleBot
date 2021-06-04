using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using BibleBot.Lib;

using BibleBot.Backend.Models;
using BibleBot.Backend.Services;
using BibleBot.Backend.Services.Providers;

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