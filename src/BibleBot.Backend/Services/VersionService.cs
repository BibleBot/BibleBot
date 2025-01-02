/*
* Copyright (C) 2016-2024 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.Threading.Tasks;
using BibleBot.Models;
using MongoDB.Driver;

namespace BibleBot.Backend.Services
{
    public class VersionService(MongoService mongoService)
    {
        private readonly MongoService _mongoService = mongoService;

        public async Task<List<Version>> Get() => await _mongoService.Get<Version>();
        public async Task<Version> Get(string abbv) => await _mongoService.Get<Version>(abbv);
        public async Task<long> GetCount() => await _mongoService.GetCount<Version>();

        public async Task<Version> Create(Version version) => await _mongoService.Create(version);

        public async Task Update(string abbv, UpdateDefinition<Version> updateDefinition) => await _mongoService.Update(abbv, updateDefinition);
        public async Task Remove(Version idealVersion) => await _mongoService.Remove(idealVersion);
    }
}
