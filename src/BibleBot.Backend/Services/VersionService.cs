/*
* Copyright (C) 2016-2023 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BibleBot.Models;
using MongoDB.Driver;

namespace BibleBot.Backend.Services
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

        public async Task<List<Version>> Get() => (await _versions.FindAsync(version => true)).ToList();
        public async Task<Version> Get(string abbv) => (await _versions.FindAsync<Version>(version => version.Abbreviation.ToUpperInvariant() == abbv.ToUpperInvariant())).FirstOrDefault();
        public async Task<long> GetCount() => await _versions.EstimatedDocumentCountAsync();

        public async Task<Version> Create(Version version)
        {
            await _versions.InsertOneAsync(version);
            return version;
        }

        public async Task Update(string abbv, UpdateDefinition<Version> updateDefinition) => await _versions.UpdateOneAsync(version => version.Abbreviation.ToUpperInvariant() == abbv.ToUpperInvariant(), updateDefinition);
        public async Task Remove(Version idealVersion) => await this.Remove(idealVersion.Abbreviation);
        public async Task Remove(string abbv) => await _versions.DeleteOneAsync(version => version.Abbreviation.ToUpperInvariant() == abbv.ToUpperInvariant());
    }
}
