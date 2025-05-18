/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BibleBot.Models;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Driver;
using StackExchange.Redis;

namespace BibleBot.Backend.Services
{
    public class GuildService(IDistributedCache cache, MongoService mongoService)
    {
        private readonly IDistributedCache _cache = cache;
        private readonly MongoService _mongoService = mongoService;
        private static readonly ConnectionMultiplexer _connectionMultiplexer = ConnectionMultiplexer.Connect("127.0.0.1:6379");
        private readonly IServer _redisServer = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().First());

        public async Task<List<Guild>> Get()
        {
            List<Guild> guilds = [];

            try
            {
                RedisKey[] keys = [.. _redisServer.Keys(pattern: "guild:*")];

                foreach (RedisKey key in keys)
                {
                    string cachedGuildStr = await _cache.GetStringAsync(key);
                    guilds.Add(JsonSerializer.Deserialize<Guild>(cachedGuildStr));
                }
            }
            catch (ArgumentNullException)
            {
                guilds = await _mongoService.Get<Guild>();

                foreach (Guild guild in guilds)
                {
                    await _cache.SetStringAsync($"guild:{guild.GuildId}", JsonSerializer.Serialize(guild));
                }
            }

            return guilds;
        }

        public async Task<Guild> Get(string guildId)
        {
            Guild guild;

            try
            {
                string cachedGuildStr = await _cache.GetStringAsync($"guild:{guildId}");
                guild = JsonSerializer.Deserialize<Guild>(cachedGuildStr);
            }
            catch (ArgumentNullException)
            {

                guild = await _mongoService.Get<Guild>(guildId);

                if (guild != null)
                {
                    await _cache.SetStringAsync($"guild:{guild.GuildId}", JsonSerializer.Serialize(guild));
                }
            }

            return guild;
        }

        public async Task<int> GetCount() => (await Get()).Count;

        public async Task<Guild> Create(Guild guild)
        {
            Guild createdGuild = await _mongoService.Create(guild);
            await _cache.SetStringAsync($"guild:{guild.GuildId}", JsonSerializer.Serialize(createdGuild));

            return createdGuild;
        }

        public async Task Update(string guildId, UpdateDefinition<Guild> updateDefinition)
        {
            await _mongoService.Update(guildId, updateDefinition);

            Guild guild = await _mongoService.Get<Guild>(guildId);
            await _cache.SetStringAsync($"guild:{guild.GuildId}", JsonSerializer.Serialize(guild));
        }

        public async Task Remove(Guild idealGuild)
        {
            await _mongoService.Remove(idealGuild);
            await _cache.RemoveAsync($"guild:{idealGuild.GuildId}");
        }
    }
}
