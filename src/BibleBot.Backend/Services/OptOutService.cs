/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
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
    public class OptOutService(MongoService mongoService)
    {
        public async Task<List<OptOutUser>> Get() => await mongoService.Get<OptOutUser>();
        public async Task<OptOutUser> Get(string userId) => await mongoService.Get<OptOutUser>(userId);
        public async Task<long> GetCount() => await mongoService.GetCount<OptOutUser>();

        public async Task<OptOutUser> Create(OptOutUser user) => await mongoService.Create(user);

        public async Task Update(string userId, UpdateDefinition<OptOutUser> updateDefinition) => await mongoService.Update(userId, updateDefinition);
        public async Task Remove(OptOutUser idealUser) => await mongoService.Remove(idealUser);
    }
}
