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
    public class UserService(PreferenceService preferenceService)
    {
        public async Task<List<User>> Get(bool isAutoServ = false) => await preferenceService.Get<User>(isAutoServ);
        public async Task<User> Get(string userId) => await preferenceService.Get<User>(userId);
        public async Task<long> GetCount() => await preferenceService.GetCount<User>();
        public async Task<User> Create(User user) => await preferenceService.Create(user);
        public async Task Update(string userId, UpdateDefinition<User> updateDefinition) => await preferenceService.Update(userId, updateDefinition);
        public async Task Remove(User idealUser) => await preferenceService.Remove(idealUser);
    }
}
