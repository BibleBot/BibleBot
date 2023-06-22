/*
* Copyright (C) 2016-2023 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using BibleBot.Backend.Services;
using BibleBot.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        /// <param name="req">A BibleBot.Backend.Request object</param>
        /// <response code="200">Returns BibleBot.Backend.CommandResponse</response>
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

            var idealGuild = _guildService.Get(req.GuildId);

            if (idealGuild != null)
            {
                if (req.Body == "delete")
                {
                    idealGuild.DailyVerseWebhook = null;
                    idealGuild.DailyVerseChannelId = null;
                    idealGuild.DailyVerseTime = null;
                    idealGuild.DailyVerseTimeZone = null;
                    idealGuild.DailyVerseLastSentDate = null;

                    _guildService.Update(req.GuildId, idealGuild);
                }
                else
                {
                    var fields = req.Body.Split("||");

                    idealGuild.DailyVerseWebhook = fields[0];
                    idealGuild.DailyVerseChannelId = fields[1];
                    _guildService.Update(req.GuildId, idealGuild);

                    return new CommandResponse
                    {
                        OK = true
                    };
                }
            }

            return new CommandResponse
            {
                OK = false
            };
        }
    }
}
