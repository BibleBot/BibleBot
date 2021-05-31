using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

using BibleBot.Lib;

using BibleBot.Backend.Models;
using BibleBot.Backend.Services;
using BibleBot.Backend.Services.Providers;

namespace BibleBot.Backend.Controllers
{
    [Produces("application/json")]
    [Route("api/webhooks")]
    [ApiController]
    public class WebhooksController : ControllerBase
    {
        private readonly GuildService _guildService;

        public WebhooksController(GuildService guildService)
        {
            _guildService = guildService;
        }

        /// <summary>
        /// Processes a message to add a webhook url to a Guild object.
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
            var idealGuild = _guildService.Get(req.GuildId);

            if (idealGuild != null)
            {
                idealGuild.DailyVerseWebhook = req.Body;
                _guildService.Update(req.GuildId, idealGuild);

                return new CommandResponse
                {
                    OK = true
                };
            }

            return new CommandResponse
            {
                OK = false
            };
        }
    }
}