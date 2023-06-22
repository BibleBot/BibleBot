/*
* Copyright (C) 2016-2023 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Linq;
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

        public FrontendStats Get() => _frontendStats.Find(frontendStats => true).FirstOrDefault();

        public FrontendStats Create(FrontendStats frontendStats)
        {
            _frontendStats.InsertOne(frontendStats);
            return frontendStats;
        }

        public void Update(FrontendStats frontendStats) => _frontendStats.ReplaceOne(frontendStats => true, frontendStats);
    }
}
