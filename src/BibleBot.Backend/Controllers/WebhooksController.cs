/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

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
    public class WebhooksController(GuildService guildService) : ControllerBase
    {
        /// <summary>
        /// Processes a message to add a webhook url to a Guild object.
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
            Guild idealGuild = await guildService.Get(req.GuildId);

            if (idealGuild != null)
            {
                if (req.Body == "delete")
                {
                    if (req.ChannelId != idealGuild.DailyVerseChannelId)
                    {
                        return BadRequest(new CommandResponse
                        {
                            OK = false
                        });
                    }

                    UpdateDefinition<Guild> update = Builders<Guild>.Update
                             .Set(guild => guild.DailyVerseWebhook, null)
                             .Set(guild => guild.DailyVerseChannelId, null)
                             .Set(guild => guild.DailyVerseTime, null)
                             .Set(guild => guild.DailyVerseTimeZone, null)
                             .Set(guild => guild.DailyVerseLastSentDate, null)
                             .Set(guild => guild.DailyVerseRoleId, null)
                             .Set(guild => guild.DailyVerseIsThread, false);

                    await guildService.Update(req.GuildId, update);
                }
                else
                {
                    UpdateDefinition<Guild> update = Builders<Guild>.Update
                                 .Set(guild => guild.DailyVerseWebhook, req.Body);

                    await guildService.Update(req.GuildId, update);
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
