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
using Sentry;
using StackExchange.Redis;

namespace BibleBot.Backend.Services
{
    public class PreferenceService(IDistributedCache cache, MongoService mongoService)
    {
        private static readonly ConnectionMultiplexer _connectionMultiplexer = ConnectionMultiplexer.Connect("127.0.0.1:6379");
        private readonly IServer _redisServer = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().First());

        public async Task<List<T>> Get<T>(bool isAutoServ = false) where T : IPreference
        {
            Type typeOfT = typeof(T);

            if (isAutoServ)
            {
                return await mongoService.Get<T>();
            }

            List<T> results = [];

            string set = typeOfT.Name switch
            {
                nameof(User) => "user",
                nameof(Guild) => "guild",
                _ => throw new NotImplementedException("No established path for provided type")
            };

            try
            {
                RedisKey[] keys = [.. _redisServer.Keys(pattern: $"{set}:*")];

                foreach (RedisKey key in keys)
                {
                    string cachedStr = await cache.GetStringAsync(key);
                    results.Add(JsonSerializer.Deserialize<T>(cachedStr!));
                }
            }
            catch (ArgumentNullException)
            {
                results = await mongoService.Get<T>();

                foreach (T t in results)
                {
                    await cache.SetStringAsync($"{set}:{t.SnowflakeId}", JsonSerializer.Serialize(t));
                }
            }

            return results;
        }

        public async Task<T> Get<T>(string query) where T : IPreference
        {
            Type typeOfT = typeof(T);
            T result;

            string set = typeOfT.Name switch
            {
                nameof(User) => "user",
                nameof(Guild) => "guild",
                _ => throw new NotImplementedException("No established path for provided type")
            };

            try
            {
                string cachedStr = await cache.GetStringAsync($"{set}:{query}");
                result = JsonSerializer.Deserialize<T>(cachedStr!);
            }
            catch (ArgumentNullException)
            {
                result = await mongoService.Get<T>(query);

                if (result != null)
                {
                    await cache.SetStringAsync($"{set}:{result.SnowflakeId}", JsonSerializer.Serialize(result));
                }
            }


            SentrySdk.ConfigureScope(scope =>
            {
                scope.Contexts[$"{set}Preference"] = result;
            });

            return result;
        }

        public async Task<long> GetCount<T>() => await mongoService.GetCount<T>();

        public async Task<T> Create<T>(T t) where T : IPreference
        {
            Type typeOfT = typeof(T);

            string set = typeOfT.Name switch
            {
                nameof(User) => "user",
                nameof(Guild) => "guild",
                _ => throw new NotImplementedException("No established path for provided type")
            };

            T createdT = await mongoService.Create(t);
            await cache.SetStringAsync($"{set}:{createdT.SnowflakeId}", JsonSerializer.Serialize(createdT));

            return createdT;
        }

        public async Task Update(string userId, UpdateDefinition<User> updateDefinition)
        {
            await mongoService.Update(userId, updateDefinition);

            User user = await mongoService.Get<User>(userId);
            await cache.SetStringAsync($"user:{user.UserId}", JsonSerializer.Serialize(user));
        }

        public async Task Update(string guildId, UpdateDefinition<Guild> updateDefinition)
        {
            await mongoService.Update(guildId, updateDefinition);

            Guild guild = await mongoService.Get<Guild>(guildId);
            await cache.SetStringAsync($"guild:{guild.GuildId}", JsonSerializer.Serialize(guild));
        }

        public async Task Remove<T>(T t) where T : IPreference
        {
            await mongoService.Remove<T>(t.SnowflakeId);

            Type typeOfT = typeof(T);

            string set = typeOfT.Name switch
            {
                nameof(User) => "user",
                nameof(Guild) => "guild",
                _ => throw new NotImplementedException("No established path for provided type")
            };

            await cache.RemoveAsync($"{set}:{t.SnowflakeId}");
        }
    }
}
