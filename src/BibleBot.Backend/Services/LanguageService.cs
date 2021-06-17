using BibleBot.Backend.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System;

namespace BibleBot.Backend.Services
{
    public class LanguageService
    {
        private readonly IMongoCollection<Language> _languages;
        
        public LanguageService(IDatabaseSettings settings)
        {
            var client = new MongoClient(Environment.GetEnvironmentVariable("MONGODB_CONN"));
            var database = client.GetDatabase(settings.DatabaseName);

            _languages = database.GetCollection<Language>(settings.LanguageCollectionName);
        }

        public List<Language> Get() => _languages.Find(language => true).ToList();
        public Language Get(string objectName) => _languages.Find<Language>(language => language.ObjectName == objectName).FirstOrDefault();

        public Language Create(Language language) 
        {
            _languages.InsertOne(language);
            return language;
        }

        public void Update(string objectName, Language newLanguage) => _languages.ReplaceOne(language => language.ObjectName == objectName, newLanguage);
        public void Remove(Language idealLanguage) => _languages.DeleteOne(language => language.Id == idealLanguage.Id);
        public void Remove(string objectName) => _languages.DeleteOne(language => language.ObjectName == objectName);
    }
}