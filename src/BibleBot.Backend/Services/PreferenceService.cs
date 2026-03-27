/*
* Copyright (C) 2016-2026 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BibleBot.Models;
using Microsoft.Extensions.Caching.Distributed;
using Sentry;

namespace BibleBot.Backend.Services
{
    public class PreferenceService(IDistributedCache cache, PostgresService postgresService)
    {
        public async Task<List<T>> Get<T>() where T : class => await postgresService.Get<T>();

        public async Task<T> Get<T>(long query) where T : class
        {
            string typeName = typeof(T).Name.ToLower();
            string cacheKey = $"{typeName}:{query}";

            string cachedStr = await cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedStr))
            {
                if (cachedStr == "null") return null;
                T cachedResult = JsonSerializer.Deserialize<T>(cachedStr, new JsonSerializerOptions { NumberHandling = JsonNumberHandling.AllowReadingFromString });

                SentrySdk.ConfigureScope(scope =>
                {
                    scope.Contexts[$"{typeName}Preference"] = cachedResult;
                });

                return cachedResult;
            }

            T result = await postgresService.Get<T>(query);
            string valueToCache = result != null ? JsonSerializer.Serialize(result) : "null";
            await cache.SetStringAsync(cacheKey, valueToCache);

            SentrySdk.ConfigureScope(scope =>
            {
                scope.Contexts[$"{typeName}Preference"] = result;
            });

            return result;
        }

        public async Task<int> GetCount<T>() where T : class => await postgresService.GetCount<T>();

        public async Task<T> Create<T>(T t) where T : class, IPreference
        {
            Type typeOfT = typeof(T);

            string set = typeOfT.Name switch
            {
                nameof(User) => "user",
                nameof(Guild) => "guild",
                _ => throw new NotImplementedException("No established path for provided type")
            };

            T createdT = await postgresService.Create(t);
            await cache.SetStringAsync($"{set}:{createdT.Id}", JsonSerializer.Serialize(createdT));

            return createdT;
        }

        public async Task Update(long userId, UpdateDef<User> updateDef)
        {
            await postgresService.Update(userId, updateDef);

            User user = await postgresService.Get<User>(userId);
            await cache.SetStringAsync($"user:{user.Id}", JsonSerializer.Serialize(user));
        }

        public async Task Update(long guildId, UpdateDef<Guild> updateDef)
        {
            await postgresService.Update(guildId, updateDef);

            Guild guild = await postgresService.Get<Guild>(guildId);
            await cache.SetStringAsync($"guild:{guild.Id}", JsonSerializer.Serialize(guild));
        }

        public async Task Remove<T>(T t) where T : IPreference
        {
            Type typeOfT = typeof(T);

            if (typeOfT == typeof(User))
            {
                await postgresService.Remove(t as User);
                await cache.RemoveAsync($"user:{t.Id}");
            }
            else if (typeOfT == typeof(Guild))
            {
                await postgresService.Remove(t as Guild);
                await cache.RemoveAsync($"guild:{t.Id}");
            }

            throw new NotImplementedException("No established path for provided type");
        }
    }
}
