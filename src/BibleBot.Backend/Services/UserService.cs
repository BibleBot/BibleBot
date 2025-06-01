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
    public class UserService(IDistributedCache cache, MongoService mongoService)
    {
        private static readonly ConnectionMultiplexer _connectionMultiplexer = ConnectionMultiplexer.Connect("127.0.0.1:6379");
        private readonly IServer _redisServer = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().First());

        public async Task<List<User>> Get()
        {
            List<User> users = [];

            try
            {
                RedisKey[] keys = [.. _redisServer.Keys(pattern: "user:*")];

                foreach (RedisKey key in keys)
                {
                    string cachedUserStr = await cache.GetStringAsync(key);
                    users.Add(JsonSerializer.Deserialize<User>(cachedUserStr!));
                }
            }
            catch (ArgumentNullException)
            {
                users = await mongoService.Get<User>();

                foreach (User user in users)
                {
                    await cache.SetStringAsync($"user:{user.UserId}", JsonSerializer.Serialize(user));
                }
            }

            return users;
        }

        public async Task<User> Get(string userId)
        {
            User user;

            try
            {
                string cachedUserStr = await cache.GetStringAsync($"user:{userId}");
                user = JsonSerializer.Deserialize<User>(cachedUserStr!);
            }
            catch (ArgumentNullException)
            {

                user = await mongoService.Get<User>(userId);

                if (user != null)
                {
                    await cache.SetStringAsync($"user:{user.UserId}", JsonSerializer.Serialize(user));
                }
            }

            if (user != null)
            {
                SentrySdk.ConfigureScope(scope =>
                {
                    scope.Contexts["userPreference"] = user;
                });
            }

            return user;
        }

        public async Task<int> GetCount() => (await Get()).Count;

        public async Task<User> Create(User user)
        {
            User createdUser = await mongoService.Create(user);
            await cache.SetStringAsync($"user:{user.UserId}", JsonSerializer.Serialize(createdUser));

            return createdUser;
        }

        public async Task Update(string userId, UpdateDefinition<User> updateDefinition)
        {
            await mongoService.Update(userId, updateDefinition);

            User user = await mongoService.Get<User>(userId);
            await cache.SetStringAsync($"user:{user.UserId}", JsonSerializer.Serialize(user));
        }

        public async Task Remove(User idealUser)
        {
            await mongoService.Remove(idealUser);
            await cache.RemoveAsync($"user:{idealUser.UserId}");
        }
    }
}
