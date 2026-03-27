/*
* Copyright (C) 2016-2026 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.Threading.Tasks;
using BibleBot.Models;

namespace BibleBot.Backend.Services
{
    public class UserService(PreferenceService preferenceService)
    {
        public async Task<List<User>> Get() => await preferenceService.Get<User>();
        public async Task<User> Get(long userId) => await preferenceService.Get<User>(userId);
        public async Task<int> GetCount() => await preferenceService.GetCount<User>();
        public async Task<User> Create(User user) => await preferenceService.Create(user);
        public async Task Update(long userId, UpdateDef<User> updateDef) => await preferenceService.Update(userId, updateDef);
        public async Task Remove(User idealUser) => await preferenceService.Remove(idealUser);
    }
}
