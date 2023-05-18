/*
* Copyright (C) 2016-2023 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.Linq;
using BibleBot.AutomaticServices.Models;
using MongoDB.Driver;

namespace BibleBot.AutomaticServices.Services
{
    public class VersionService
    {
        private readonly IMongoCollection<Version> _versions;

        public VersionService(IDatabaseSettings settings)
        {
            var client = new MongoClient(System.Environment.GetEnvironmentVariable("MONGODB_CONN"));
            var database = client.GetDatabase(settings.DatabaseName);

            _versions = database.GetCollection<Version>(settings.VersionCollectionName);
        }

        public List<Version> Get() => _versions.Find(version => true).ToList();
        public Version Get(string abbv) => _versions.Find<Version>(version => version.Abbreviation.ToUpperInvariant() == abbv.ToUpperInvariant()).FirstOrDefault();
    }
}
