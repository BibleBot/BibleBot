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
using Version = BibleBot.Models.Version;

namespace BibleBot.Backend.Services
{
    public class VersionService(IDistributedCache cache, MongoService mongoService)
    {
        private readonly IDistributedCache _cache = cache;
        private readonly MongoService _mongoService = mongoService;
        private static readonly ConnectionMultiplexer _connectionMultiplexer = ConnectionMultiplexer.Connect("127.0.0.1:6379");
        private readonly IServer _redisServer = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().First());

        public async Task<List<Version>> Get()
        {
            List<Version> versions = [];

            try
            {
                RedisKey[] keys = [.. _redisServer.Keys(pattern: "version:*")];

                foreach (RedisKey key in keys)
                {
                    string cachedVersionStr = await _cache.GetStringAsync(key);
                    versions.Add(JsonSerializer.Deserialize<Version>(cachedVersionStr));
                }
            }
            catch (ArgumentNullException)
            {
                versions = await _mongoService.Get<Version>();

                foreach (Version version in versions)
                {
                    await _cache.SetStringAsync($"version:{version.Abbreviation}", JsonSerializer.Serialize(version));
                }
            }

            return versions;
        }

        public async Task<Version> Get(string abbreviation)
        {
            Version version;

            try
            {
                string cachedVersionStr = await _cache.GetStringAsync($"version:{abbreviation}");
                version = JsonSerializer.Deserialize<Version>(cachedVersionStr);
            }
            catch (ArgumentNullException)
            {

                version = await _mongoService.Get<Version>(abbreviation);

                if (version != null)
                {
                    await _cache.SetStringAsync($"version:{version.Abbreviation}", JsonSerializer.Serialize(version));
                }
            }

            return version;
        }

        public async Task<int> GetCount() => (await Get()).Count;

        public async Task<Version> GetPreferenceOrDefault(User idealUser, Guild idealGuild, bool isBot)
        {
            Version idealVersion = await Get("RSV");

            if (idealUser != null && !isBot)
            {
                idealVersion = await Get(idealUser.Version);
            }
            else if (idealGuild != null)
            {
                idealVersion = await Get(idealGuild.Version);
            }

            return idealVersion ?? await Get("RSV");
        }

        public async Task<Version> GetPreferenceOrDefault(User idealUser, bool isBot)
        {
            Version idealVersion = await Get("RSV");

            if (idealUser != null && !isBot)
            {
                idealVersion = await Get(idealUser.Version);
            }

            return idealVersion ?? await Get("RSV");
        }

        public async Task<Version> GetPreferenceOrDefault(Guild idealGuild, bool isBot)
        {
            Version idealVersion = await Get("RSV");

            if (idealGuild != null)
            {
                idealVersion = await Get(idealGuild.Version);
            }

            return idealVersion ?? await Get("RSV");
        }

        public async Task<Version> Create(Version version)
        {
            Version createdVersion = await _mongoService.Create(version);
            await _cache.SetStringAsync($"version:{version.Abbreviation}", JsonSerializer.Serialize(version));

            return createdVersion;
        }

        public async Task Update(string abbreviation, UpdateDefinition<Version> updateDefinition)
        {
            await _mongoService.Update(abbreviation, updateDefinition);

            Version version = await _mongoService.Get<Version>(abbreviation);
            await _cache.SetStringAsync($"version:{abbreviation}", JsonSerializer.Serialize(version));
        }
        public async Task Remove(Version idealVersion)
        {
            await _mongoService.Remove(idealVersion);
            await _cache.RemoveAsync($"version:{idealVersion.Abbreviation}");
        }
    }
}
