/*
* Copyright (C) 2016-2024 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.Threading.Tasks;
using BibleBot.Models;
using MongoDB.Driver;

namespace BibleBot.Backend.Services
{
    public class GuildService(MongoService mongoService)
    {
        private readonly MongoService _mongoService = mongoService;

        public async Task<List<Guild>> Get() => await _mongoService.Get<Guild>();
        public async Task<Guild> Get(string guildId) => await _mongoService.Get<Guild>(guildId);
        public async Task<long> GetCount() => await _mongoService.GetCount<Guild>();

        public async Task<Guild> Create(Guild guild) => await _mongoService.Create(guild);

        public async Task Update(string guildId, UpdateDefinition<Guild> updateDefinition) => await _mongoService.Update(guildId, updateDefinition);
        public async Task Remove(Guild idealGuild) => await _mongoService.Remove(idealGuild);
    }
}
