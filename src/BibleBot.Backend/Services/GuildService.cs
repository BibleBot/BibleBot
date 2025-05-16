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
using BibleBot.Models;
using MongoDB.Driver;

namespace BibleBot.Backend.Services
{
    public class GuildService(MongoService mongoService)
    {
        private readonly MongoService _mongoService = mongoService;
        private List<Guild> _guilds = null;

        private async Task<List<Guild>> GetGuilds(bool forcePull = false)
        {
            if (forcePull || _guilds == null)
            {
                _guilds = await _mongoService.Get<Guild>();
            }

            return _guilds;
        }

        public async Task<List<Guild>> Get(bool forcePull = false) => await GetGuilds(forcePull);
        public async Task<Guild> Get(string guildId) => (await GetGuilds()).FirstOrDefault(guild => guild.GuildId == guildId);
        public async Task<int> GetCount() => (await GetGuilds()).Count;

        public async Task<Guild> Create(Guild guild)
        {
            Guild createdGuild = await _mongoService.Create(guild);
            _guilds.Add(createdGuild);

            return createdGuild;
        }

        public async Task Update(string guildId, UpdateDefinition<Guild> updateDefinition)
        {
            Guild beforeGuild = await Get(guildId);
            await _mongoService.Update(guildId, updateDefinition);

            Guild afterGuild = await _mongoService.Get<Guild>(guildId);

            _guilds.Remove(beforeGuild);
            _guilds.Add(afterGuild);
        }
        public async Task Remove(Guild idealGuild)
        {
            await _mongoService.Remove(idealGuild);
            await GetGuilds(true);
        }
    }
}
