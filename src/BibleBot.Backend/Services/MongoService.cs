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
using MongoDB.Driver.Search;

namespace BibleBot.Backend.Services
{
    public class MongoService
    {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Guild> _guilds;
        private readonly IMongoCollection<Models.Version> _versions;
        private readonly IMongoCollection<FrontendStats> _frontendStats;
        private readonly IMongoCollection<Language> _languages;

        public MongoService(IDatabaseSettings settings)
        {
            MongoClient client = new(Environment.GetEnvironmentVariable("MONGODB_CONN"));
            IMongoDatabase database = client.GetDatabase(settings.DatabaseName);

            _users = database.GetCollection<User>(settings.UserCollectionName);
            _guilds = database.GetCollection<Guild>(settings.GuildCollectionName);
            _versions = database.GetCollection<Models.Version>(settings.VersionCollectionName);
            _frontendStats = database.GetCollection<FrontendStats>(settings.FrontendStatsCollectionName);
            _languages = database.GetCollection<Language>(settings.LanguageCollectionName);
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
            else if (typeOfT == typeof(Language))
            {
                cursor = (IAsyncCursor<T>)await _languages.FindAsync(language => true);
            }

            return cursor != null ? await cursor.ToListAsync() : throw new NotImplementedException("No established path for provided type");
        }

        // TODO(srp): Replace these FindAsync()s with aggregate+$lookup for performance benefits.
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
            else if (typeOfT == typeof(Language))
            {
                cursor = (IAsyncCursor<T>)await _languages.FindAsync(language => language.Culture == query);
            }

            return cursor != null ? await cursor.FirstOrDefaultAsync() : throw new NotImplementedException("No established path for provided type");
        }

        public async Task<List<T>> Search<T>(SearchDefinition<T> def)
        {
            Type typeOfT = typeof(T);
            IAsyncCursor<T> cursor = null;

            if (typeOfT == typeof(Models.Version))
            {
                cursor = (IAsyncCursor<T>)await _versions.Aggregate().Search((SearchDefinition<Models.Version>)(object)def).ToCursorAsync();
            }
            else if (typeOfT == typeof(User))
            {
                cursor = (IAsyncCursor<T>)await _users.Aggregate().Search((SearchDefinition<User>)(object)def).ToCursorAsync();
            }
            else if (typeOfT == typeof(Guild))
            {
                cursor = (IAsyncCursor<T>)await _guilds.Aggregate().Search((SearchDefinition<Guild>)(object)def).ToCursorAsync();
            }

            return cursor != null ? await cursor.ToListAsync() : throw new NotImplementedException("No established path for provided type");
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

        public async Task<T> Create<T>(T t)
        {
            Type typeOfT = typeof(T);

            if (typeOfT == typeof(Models.Version))
            {
                await _versions.InsertOneAsync(t as Models.Version);
            }
            else if (typeOfT == typeof(User))
            {
                await _users.InsertOneAsync(t as User);
            }
            else if (typeOfT == typeof(Guild))
            {
                await _guilds.InsertOneAsync(t as Guild);
            }
            else if (typeOfT == typeof(FrontendStats))
            {
                await _frontendStats.InsertOneAsync(t as FrontendStats);
            }

            return t;
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
