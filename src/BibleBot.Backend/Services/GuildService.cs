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
        private static readonly ConnectionMultiplexer _connectionMultiplexer = ConnectionMultiplexer.Connect("127.0.0.1:6379");
        private readonly IServer _redisServer = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().First());

        public async Task<List<Guild>> Get(bool isAutoServ = false)
        {
            List<Guild> guilds = [];

            try
            {
                if (isAutoServ)
                {
                    throw new Exception();
                }

                RedisKey[] keys = [.. _redisServer.Keys(pattern: "guild:*")];

                foreach (RedisKey key in keys)
                {
                    string cachedGuildStr = await cache.GetStringAsync(key);
                    guilds.Add(JsonSerializer.Deserialize<Guild>(cachedGuildStr!));
                }
            }
            catch (Exception)
            {
                guilds = await mongoService.Get<Guild>();

                foreach (Guild guild in guilds)
                {
                    await cache.SetStringAsync($"guild:{guild.GuildId}", JsonSerializer.Serialize(guild));
                }
            }

            return guilds;
        }

        public async Task<Guild> Get(string guildId)
        {
            Guild guild;

            try
            {
                string cachedGuildStr = await cache.GetStringAsync($"guild:{guildId}");
                guild = JsonSerializer.Deserialize<Guild>(cachedGuildStr!);
            }
            catch (ArgumentNullException)
            {

                guild = await mongoService.Get<Guild>(guildId);

                if (guild != null)
                {
                    await cache.SetStringAsync($"guild:{guild.GuildId}", JsonSerializer.Serialize(guild));
                }
            }

            return guild;
        }

        public async Task<int> GetCount() => (await Get()).Count;

        public async Task<Guild> Create(Guild guild)
        {
            Guild createdGuild = await mongoService.Create(guild);
            await cache.SetStringAsync($"guild:{guild.GuildId}", JsonSerializer.Serialize(createdGuild));

            return createdGuild;
        }

        public async Task Update(string guildId, UpdateDefinition<Guild> updateDefinition)
        {
            await mongoService.Update(guildId, updateDefinition);

            Guild guild = await mongoService.Get<Guild>(guildId);
            await cache.SetStringAsync($"guild:{guild.GuildId}", JsonSerializer.Serialize(guild));
        }

        public async Task Remove(Guild idealGuild)
        {
            await mongoService.Remove(idealGuild);
            await cache.RemoveAsync($"guild:{idealGuild.GuildId}");
        }
    }
}
