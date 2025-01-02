/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BibleBot.Models;
using MongoDB.Driver;

namespace BibleBot.Backend.Services
{
    public class VersionDataService
    {
        private readonly IMongoCollection<User> _users;

        public VersionDataService(IDatabaseSettings settings)
        {
            MongoClient client = new(Environment.GetEnvironmentVariable("MONGODB_CONN"));
            IMongoDatabase database = client.GetDatabase(settings.DatabaseName);

            _users = database.GetCollection<User>(settings.VersionDataIndexName);
        }

        public async Task<List<User>> Get() => (await _users.FindAsync(user => true)).ToList();
        public async Task<User> Get(string userId) => (await _users.FindAsync(user => user.UserId == userId)).FirstOrDefault();
        public async Task<long> GetCount() => await _users.EstimatedDocumentCountAsync();

        public async Task<User> Create(User user)
        {
            await _users.InsertOneAsync(user);
            return user;
        }

        public async Task Update(string userId, UpdateDefinition<User> updateDefinition) => await _users.UpdateOneAsync(user => user.UserId == userId, updateDefinition);
        public async Task Remove(User idealUser) => await Remove(idealUser.UserId);
        public async Task Remove(string userId) => await _users.DeleteOneAsync(user => user.UserId == userId);
    }
}
