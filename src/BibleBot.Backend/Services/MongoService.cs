/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BibleBot.Models;
using MongoDB.Driver;

namespace BibleBot.Backend.Services
{
    public class MongoService
    {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Guild> _guilds;
        private readonly IMongoCollection<Models.Version> _versions;
        private readonly IMongoCollection<FrontendStats> _frontendStats;

        public MongoService(IDatabaseSettings settings)
        {
            MongoClient client = new(Environment.GetEnvironmentVariable("MONGODB_CONN"));
            IMongoDatabase database = client.GetDatabase(settings.DatabaseName);

            _users = database.GetCollection<User>(settings.UserCollectionName);
            _guilds = database.GetCollection<Guild>(settings.GuildCollectionName);
            _versions = database.GetCollection<Models.Version>(settings.VersionCollectionName);
            _frontendStats = database.GetCollection<FrontendStats>(settings.FrontendStatsCollectionName);
        }

        public async Task<List<T>> Get<T>()
        {
            Type typeOfT = typeof(T);
            IAsyncCursor<T> cursor = null;

            if (typeOfT == typeof(Models.Version))
            {
                cursor = (IAsyncCursor<T>)await _versions.FindAsync(version => true);
            }
            else if (typeOfT == typeof(User))
            {
                cursor = (IAsyncCursor<T>)await _users.FindAsync(user => true);
            }
            else if (typeOfT == typeof(Guild))
            {
                cursor = (IAsyncCursor<T>)await _guilds.FindAsync(guild => true);
            }
            else if (typeOfT == typeof(FrontendStats))
            {
                cursor = (IAsyncCursor<T>)await _frontendStats.FindAsync(frontendStats => true);
            }

            return cursor != null ? await cursor.ToListAsync() : throw new NotImplementedException("No established path for provided type");
        }

        public async Task<T> Get<T>(string query)
        {
            Type typeOfT = typeof(T);
            IAsyncCursor<T> cursor = null;

            if (typeOfT == typeof(Models.Version))
            {
                cursor = (IAsyncCursor<T>)await _versions.FindAsync(version => string.Equals(version.Abbreviation, query, StringComparison.OrdinalIgnoreCase));
            }
            else if (typeOfT == typeof(User))
            {
                cursor = (IAsyncCursor<T>)await _users.FindAsync(user => user.UserId == query);
            }
            else if (typeOfT == typeof(Guild))
            {
                cursor = (IAsyncCursor<T>)await _guilds.FindAsync(guild => guild.GuildId == query);
            }

            return cursor != null ? await cursor.FirstOrDefaultAsync() : throw new NotImplementedException("No established path for provided type");
        }

        public async Task<long> GetCount<T>()
        {
            Type typeOfT = typeof(T);

            if (typeOfT == typeof(Models.Version))
            {
                return await _versions.EstimatedDocumentCountAsync();
            }
            else if (typeOfT == typeof(User))
            {
                return await _users.EstimatedDocumentCountAsync();
            }
            else if (typeOfT == typeof(Guild))
            {
                return await _guilds.EstimatedDocumentCountAsync();
            }

            throw new NotImplementedException("No established path for provided type");
        }

        public async Task<User> Create(User user)
        {
            await _users.InsertOneAsync(user);
            return user;
        }

        public async Task<Guild> Create(Guild guild)
        {
            await _guilds.InsertOneAsync(guild);
            return guild;
        }

        public async Task<Models.Version> Create(Models.Version version)
        {
            await _versions.InsertOneAsync(version);
            return version;
        }

        public async Task<FrontendStats> Create(FrontendStats frontendStats)
        {
            await _frontendStats.InsertOneAsync(frontendStats);
            return frontendStats;
        }

        public async Task Update(string userId, UpdateDefinition<User> updateDefinition) => await _users.UpdateOneAsync(user => user.UserId == userId, updateDefinition);
        public async Task Update(string guildId, UpdateDefinition<Guild> updateDefinition) => await _guilds.UpdateOneAsync(guild => guild.GuildId == guildId, updateDefinition);
        public async Task Update(string abbv, UpdateDefinition<Models.Version> updateDefinition) => await _versions.UpdateOneAsync(version => string.Equals(version.Abbreviation, abbv, StringComparison.OrdinalIgnoreCase), updateDefinition);
        public async Task Update(FrontendStats frontendStats, UpdateDefinition<FrontendStats> updateDefinition) => await _frontendStats.UpdateOneAsync(frontendStats => true, updateDefinition);

        public async Task Remove(User idealUser) => await Remove<User>(idealUser.UserId);
        public async Task Remove(Guild idealGuild) => await Remove<Guild>(idealGuild.GuildId);
        public async Task Remove(Models.Version idealVersion) => await Remove<Models.Version>(idealVersion.Abbreviation);

        public async Task<DeleteResult> Remove<T>(string query)
        {
            Type typeOfT = typeof(T);

            if (typeOfT == typeof(Models.Version))
            {
                return await _versions.DeleteOneAsync(version => string.Equals(version.Abbreviation, query, StringComparison.OrdinalIgnoreCase));
            }
            else if (typeOfT == typeof(User))
            {
                return await _users.DeleteOneAsync(user => user.UserId == query);
            }
            else if (typeOfT == typeof(Guild))
            {
                return await _guilds.DeleteOneAsync(guild => guild.GuildId == query);
            }

            throw new NotImplementedException("No established path for provided type");
        }
    }
}
