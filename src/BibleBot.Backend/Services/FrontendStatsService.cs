/*
* Copyright (C) 2016-2023 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Linq;
using System.Threading.Tasks;
using BibleBot.Models;
using MongoDB.Driver;

namespace BibleBot.Backend.Services
{
    public class FrontendStatsService
    {
        private readonly IMongoCollection<FrontendStats> _frontendStats;

        public FrontendStatsService(IDatabaseSettings settings)
        {
            var client = new MongoClient(Environment.GetEnvironmentVariable("MONGODB_CONN"));
            var database = client.GetDatabase(settings.DatabaseName);

            _frontendStats = database.GetCollection<FrontendStats>(settings.FrontendStatsCollectionName);
        }

        public async Task<FrontendStats> Get() => (await _frontendStats.FindAsync(frontendStats => true)).FirstOrDefault();

        public async Task<FrontendStats> Create(FrontendStats frontendStats)
        {
            await _frontendStats.InsertOneAsync(frontendStats);
            return frontendStats;
        }

        public async Task Update(FrontendStats frontendStats, UpdateDefinition<FrontendStats> updateDefinition) => await _frontendStats.UpdateOneAsync(frontendStats => true, updateDefinition);
    }
}
