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
    public class LanguageService
    {
        private readonly IMongoCollection<Language> _languages;

        public LanguageService(IDatabaseSettings settings)
        {
            MongoClient client = new(Environment.GetEnvironmentVariable("MONGODB_CONN"));
            IMongoDatabase database = client.GetDatabase(settings.DatabaseName);

            _languages = database.GetCollection<Language>(settings.LanguageCollectionName);
        }

        public async Task<List<Language>> Get() => (await _languages.FindAsync(language => true)).ToList();
        public async Task<Language> Get(string culture) => (await _languages.FindAsync(language => language.Culture == culture)).FirstOrDefault();

        public async Task<Language> Create(Language language)
        {
            await _languages.InsertOneAsync(language);
            return language;
        }

        public async Task Update(string culture, UpdateDefinition<Language> updateDefinition) => await _languages.UpdateOneAsync(language => language.Culture == culture, updateDefinition);
        public async Task Remove(Language idealLanguage) => await Remove(idealLanguage.Culture);
        public async Task Remove(string culture) => await _languages.DeleteOneAsync(language => language.Culture == culture);
    }
}
