/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using BibleBot.Models;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Driver;
using Sentry;

namespace BibleBot.Backend.Services
{
    public class PreferenceService(IDistributedCache cache, MongoService mongoService)
    {
        public async Task<List<T>> Get<T>() where T : IPreference => await mongoService.Get<T>();

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
