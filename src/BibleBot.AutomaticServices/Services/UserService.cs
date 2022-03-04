/*
* Copyright (C) 2016-2021 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using BibleBot.AutomaticServices.Models;
using MongoDB.Driver;

namespace BibleBot.AutomaticServices.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;

        public UserService(IDatabaseSettings settings)
        {
            var client = new MongoClient(Environment.GetEnvironmentVariable("MONGODB_CONN"));
            var database = client.GetDatabase(settings.DatabaseName);

            _users = database.GetCollection<User>(settings.UserCollectionName);
        }

        public List<User> Get() => _users.Find(user => true).ToList();
        public User Get(string userId) => _users.Find<User>(user => user.UserId == userId).FirstOrDefault();

        public User Create(User user)
        {
            _users.InsertOne(user);
            return user;
        }

        public void Update(string userId, User newUser) => _users.ReplaceOne(user => user.UserId == userId, newUser);
        public void Remove(User idealUser) => _users.DeleteOne(user => user.Id == idealUser.Id);
        public void Remove(string userId) => _users.DeleteOne(user => user.UserId == userId);
    }
}
