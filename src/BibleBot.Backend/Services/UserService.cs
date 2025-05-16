/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
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
    public class UserService(MongoService mongoService)
    {
        private readonly MongoService _mongoService = mongoService;
        private List<User> _users = null;

        private async Task<List<User>> GetUsers(bool forcePull = false)
        {
            if (forcePull || _users == null)
            {
                _users = await _mongoService.Get<User>();
            }

            return _users;
        }

        public async Task<List<User>> Get() => await GetUsers();
        public async Task<User> Get(string userId) => (await GetUsers()).FirstOrDefault(user => user.UserId == userId);
        public async Task<int> GetCount() => (await GetUsers()).Count;

        public async Task<User> Create(User user)
        {
            User createdUser = await _mongoService.Create(user);
            await GetUsers(true);

            return createdUser;
        }

        public async Task Update(string userId, UpdateDefinition<User> updateDefinition)
        {
            User beforeUser = await Get(userId);
            await _mongoService.Update(userId, updateDefinition);

            User afterUser = await _mongoService.Get<User>(userId);

            _users.Remove(beforeUser);
            _users.Add(afterUser);
        }
        public async Task Remove(User idealUser)
        {
            await _mongoService.Remove(idealUser);
            await GetUsers(true);
        }
    }
}
