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
    public class UserService(IDistributedCache cache, MongoService mongoService)
    {
        private readonly IDistributedCache _cache = cache;
        private readonly MongoService _mongoService = mongoService;
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
                    string cachedUserStr = await _cache.GetStringAsync(key);
                    users.Add(JsonSerializer.Deserialize<User>(cachedUserStr));
                }
            }
            catch (ArgumentNullException)
            {
                users = await _mongoService.Get<User>();

                foreach (User user in users)
                {
                    await _cache.SetStringAsync($"user:{user.UserId}", JsonSerializer.Serialize(user));
                }
            }

            return users;
        }

        public async Task<User> Get(string userId)
        {
            User user;

            try
            {
                string cachedUserStr = await _cache.GetStringAsync($"user:{userId}");
                user = JsonSerializer.Deserialize<User>(cachedUserStr);
            }
            catch (ArgumentNullException)
            {

                user = await _mongoService.Get<User>(userId);

                if (user != null)
                {
                    await _cache.SetStringAsync($"user:{user.UserId}", JsonSerializer.Serialize(user));
                }
            }

            return user;
        }

        public async Task<int> GetCount() => (await Get()).Count;

        public async Task<User> Create(User user)
        {
            User createdUser = await _mongoService.Create(user);
            await _cache.SetStringAsync($"user:{user.UserId}", JsonSerializer.Serialize(createdUser));

            return createdUser;
        }

        public async Task Update(string userId, UpdateDefinition<User> updateDefinition)
        {
            await _mongoService.Update(userId, updateDefinition);

            User user = await _mongoService.Get<User>(userId);
            await _cache.SetStringAsync($"user:{user.UserId}", JsonSerializer.Serialize(user));
        }

        public async Task Remove(User idealUser)
        {
            await _mongoService.Remove(idealUser);
            await _cache.RemoveAsync($"user:{idealUser.UserId}");
        }
    }
}
