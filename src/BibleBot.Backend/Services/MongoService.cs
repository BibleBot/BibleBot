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
using Version = BibleBot.Models.Version;

namespace BibleBot.Backend.Services
{
    public class MongoService
    {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Guild> _guilds;
        private readonly IMongoCollection<Version> _versions;
        private readonly IMongoCollection<Language> _languages;
        private readonly IMongoCollection<FrontendStats> _frontendStats;
        private readonly IMongoCollection<Experiment> _experiments;

        public MongoService(IDatabaseSettings settings)
        {
            MongoClientSettings clientSettings = MongoClientSettings.FromConnectionString(Environment.GetEnvironmentVariable("MONGODB_CONN"));
            clientSettings.ConnectTimeout = TimeSpan.FromSeconds(5);
            clientSettings.MaxConnectionPoolSize = 200;

            MongoClient client = new(clientSettings);
            IMongoDatabase database = client.GetDatabase(settings.DatabaseName);

            _users = database.GetCollection<User>(settings.UserCollectionName);
            _guilds = database.GetCollection<Guild>(settings.GuildCollectionName);
            _versions = database.GetCollection<Version>(settings.VersionCollectionName);
            _languages = database.GetCollection<Language>(settings.LanguageCollectionName);
            _frontendStats = database.GetCollection<FrontendStats>(settings.FrontendStatsCollectionName);
            _experiments = database.GetCollection<Experiment>(settings.ExperimentCollectionName);
        }

        public async Task<List<T>> Get<T>()
        {
            Type typeOfT = typeof(T);
            IAsyncCursor<T> cursor = null;

            if (typeOfT == typeof(Version))
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
            else if (typeOfT == typeof(Experiment))
            {
                cursor = (IAsyncCursor<T>)await _experiments.FindAsync(experiment => true);
            }

            return cursor != null ? await cursor.ToListAsync() : throw new NotImplementedException("No established path for provided type");
        }

        // TODO(srp): Replace these FindAsync()s with aggregate+$lookup for performance benefits.
        public async Task<T> Get<T>(string query)
        {
            Type typeOfT = typeof(T);
            IAsyncCursor<T> cursor = null;

            FindOptions<T> findOptions = new()
            {
                AllowDiskUse = true,
                Limit = 1
            };

            if (typeOfT == typeof(Version))
            {
                cursor = (IAsyncCursor<T>)await _versions.FindAsync(version => query.ToLowerInvariant() == version.Abbreviation.ToLowerInvariant(), findOptions as FindOptions<Version>);
            }
            else if (typeOfT == typeof(User))
            {
                FilterDefinition<User> filterDefinition = Builders<User>.Filter.Eq(user => user.UserId, query);
                cursor = (IAsyncCursor<T>)await _users.FindAsync(filterDefinition, findOptions as FindOptions<User>);
            }
            else if (typeOfT == typeof(Guild))
            {
                FilterDefinition<Guild> filterDefinition = Builders<Guild>.Filter.Eq(guild => guild.GuildId, query);
                cursor = (IAsyncCursor<T>)await _guilds.FindAsync(filterDefinition, findOptions as FindOptions<Guild>);
            }
            else if (typeOfT == typeof(Language))
            {
                FilterDefinition<Language> filterDefinition = Builders<Language>.Filter.Eq(language => language.Culture, query);
                cursor = (IAsyncCursor<T>)await _languages.FindAsync(filterDefinition, findOptions as FindOptions<Language>);
            }
            else if (typeOfT == typeof(Experiment))
            {
                FilterDefinition<Experiment> filterDefinition = Builders<Experiment>.Filter.Eq(experiment => experiment.Name, query);
                cursor = (IAsyncCursor<T>)await _experiments.FindAsync(filterDefinition, findOptions as FindOptions<Experiment>);
            }

            return cursor != null ? await cursor.FirstOrDefaultAsync() : throw new NotImplementedException("No established path for provided type");
        }

        [Obsolete("Does not seem to work as intended, try Get<T>() and filter via LINQ instead.")]
        public async Task<List<T>> Search<T>(SearchDefinition<T> def)
        {
            Type typeOfT = typeof(T);
            IAsyncCursor<T> cursor = null;

            if (typeOfT == typeof(Version))
            {
                cursor = (IAsyncCursor<T>)await _versions.Aggregate().Search((SearchDefinition<Version>)Convert.ChangeType(def, typeof(SearchDefinition<Version>))).ToCursorAsync();
            }
            else if (typeOfT == typeof(User))
            {
                cursor = (IAsyncCursor<T>)await _users.Aggregate().Search((SearchDefinition<User>)Convert.ChangeType(def, typeof(SearchDefinition<User>))).ToCursorAsync();
            }
            else if (typeOfT == typeof(Guild))
            {
                cursor = (IAsyncCursor<T>)await _guilds.Aggregate().Search((SearchDefinition<Guild>)Convert.ChangeType(def, typeof(SearchDefinition<Guild>))).ToCursorAsync();
            }

            return cursor != null ? await cursor.ToListAsync() : throw new NotImplementedException("No established path for provided type");
        }

        public async Task<long> GetCount<T>()
        {
            Type typeOfT = typeof(T);

            if (typeOfT == typeof(Version))
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

            if (typeOfT == typeof(Version))
            {
                await _versions.InsertOneAsync(t as Version);
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
            else if (typeOfT == typeof(Experiment))
            {
                await _experiments.InsertOneAsync(t as Experiment);
            }

            return t;
        }

        public async Task Update(string userId, UpdateDefinition<User> updateDefinition) => await _users.UpdateOneAsync(user => user.UserId == userId, updateDefinition);
        public async Task Update(string guildId, UpdateDefinition<Guild> updateDefinition) => await _guilds.UpdateOneAsync(guild => guild.GuildId == guildId, updateDefinition);
        public async Task Update(string abbreviation, UpdateDefinition<Version> updateDefinition) => await _versions.UpdateOneAsync(version => string.Equals(version.Abbreviation, abbreviation, StringComparison.OrdinalIgnoreCase), updateDefinition);
        public async Task Update(string experimentName, UpdateDefinition<Experiment> updateDefinition) => await _experiments.UpdateOneAsync(experiment => experiment.Name == experimentName, updateDefinition);
        public async Task Update(FrontendStats frontendStats, UpdateDefinition<FrontendStats> updateDefinition) => await _frontendStats.UpdateOneAsync(stats => true, updateDefinition);

        public async Task Remove(User idealUser) => await Remove<User>(idealUser.UserId);
        public async Task Remove(Guild idealGuild) => await Remove<Guild>(idealGuild.GuildId);
        public async Task Remove(Version idealVersion) => await Remove<Version>(idealVersion.Abbreviation);
        public async Task Remove(Experiment idealExperiment) => await Remove<Experiment>(idealExperiment.Name);

        public async Task<DeleteResult> Remove<T>(string query)
        {
            Type typeOfT = typeof(T);

            if (typeOfT == typeof(Version))
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
            else if (typeOfT == typeof(Experiment))
            {
                return await _experiments.DeleteOneAsync(experiment => experiment.Name == query);
            }

            throw new NotImplementedException("No established path for provided type");
        }
    }
}
