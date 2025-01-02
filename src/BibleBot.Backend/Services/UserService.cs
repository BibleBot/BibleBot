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
    public class UserService(MongoService mongoService)
    {
        private readonly MongoService _mongoService = mongoService;

        public async Task<List<User>> Get() => await _mongoService.Get<User>();
        public async Task<User> Get(string userId) => await _mongoService.Get<User>(userId);
        public async Task<long> GetCount() => await _mongoService.GetCount<User>();

        public async Task<User> Create(User user) => await _mongoService.Create(user);

        public async Task Update(string userId, UpdateDefinition<User> updateDefinition) => await _mongoService.Update(userId, updateDefinition);
        public async Task Remove(User idealUser) => await _mongoService.Remove(idealUser);
    }
}
