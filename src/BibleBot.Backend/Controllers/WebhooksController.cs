/*
* Copyright (C) 2016-2024 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Threading.Tasks;
using BibleBot.Backend.Services;
using BibleBot.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace BibleBot.Backend.Controllers
{
    [Produces("application/json")]
    [Route("api/webhooks")]
    [ApiController]
    public class WebhooksController : ControllerBase
    {
        private readonly GuildService _guildService;

        public WebhooksController(GuildService guildService) => _guildService = guildService;

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

            Guild idealGuild = await _guildService.Get(req.GuildId);

            if (idealGuild != null)
            {
                if (req.Body == "delete")
                {
                    // i.e. if channel id is given, then make sure it's not in the database, we do not want to delete then
                    // channel ids are only included in this when frontend is checking for manually deleted webhooks
                    bool stopDeletion = req.ChannelId != null && req.ChannelId != idealGuild.DailyVerseChannelId;

                    if (!stopDeletion)
                    {
                        UpdateDefinition<Guild> update = Builders<Guild>.Update
                                 .Set(guild => guild.DailyVerseWebhook, null)
                                 .Set(guild => guild.DailyVerseChannelId, null)
                                 .Set(guild => guild.DailyVerseTime, null)
                                 .Set(guild => guild.DailyVerseTimeZone, null)
                                 .Set(guild => guild.DailyVerseLastSentDate, null)
                                 .Set(guild => guild.DailyVerseRoleId, null);

                        await _guildService.Update(req.GuildId, update);
                    }
                }
                else
                {
                    string[] fields = req.Body.Split("||");

                    UpdateDefinition<Guild> update = Builders<Guild>.Update
                                 .Set(guild => guild.DailyVerseWebhook, fields[0])
                                 .Set(guild => guild.DailyVerseChannelId, fields[1]);

                    await _guildService.Update(req.GuildId, update);
                }

                return Ok(new CommandResponse
                {
                    OK = true
                });
            }

            return BadRequest(new CommandResponse
            {
                OK = false
            });
        }
    }
}
