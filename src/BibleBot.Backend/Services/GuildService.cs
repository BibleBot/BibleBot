/*
* Copyright (C) 2016-2023 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BibleBot.Models;
using MongoDB.Driver;

namespace BibleBot.Backend.Services
{
    public class GuildService
    {
        private readonly IMongoCollection<Guild> _guilds;

        public GuildService(IDatabaseSettings settings)
        {
            var client = new MongoClient(Environment.GetEnvironmentVariable("MONGODB_CONN"));
            var database = client.GetDatabase(settings.DatabaseName);

            _guilds = database.GetCollection<Guild>(settings.GuildCollectionName);
        }

        public async Task<List<Guild>> Get() => (await _guilds.FindAsync(guild => true)).ToList();
        public async Task<Guild> Get(string guildId) => (await _guilds.FindAsync<Guild>(guild => guild.GuildId == guildId)).FirstOrDefault();
        public async Task<long> GetCount() => await _guilds.EstimatedDocumentCountAsync();

        public async Task<Guild> Create(Guild guild)
        {
            await _guilds.InsertOneAsync(guild);
            return guild;
        }

        public async Task Update(string guildId, UpdateDefinition<Guild> updateDefinition) => await _guilds.UpdateOneAsync(guild => guild.GuildId == guildId, updateDefinition);
        public async Task Remove(Guild idealGuild) => await this.Remove(idealGuild.GuildId);
        public async Task Remove(string guildId) => await _guilds.DeleteOneAsync(guild => guild.GuildId == guildId);
    }
}
