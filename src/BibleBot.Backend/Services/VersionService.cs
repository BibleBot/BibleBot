using BibleBot.Backend.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

using BibleBot.Lib;

namespace BibleBot.Backend.Services
{
    public class VersionService
    {
        private readonly IMongoCollection<Version> _versions;
        
        public VersionService(IDatabaseSettings settings)
        {
            var client = new MongoClient(System.Environment.GetEnvironmentVariable("MONGODB_CONN"));
            var database = client.GetDatabase(settings.DatabaseName);

            _versions = database.GetCollection<Version>(settings.VersionCollectionName);
        }

        public List<Version> Get() => _versions.Find(version => true).ToList();
        public Version Get(string abbv) => _versions.Find<Version>(version => version.Abbreviation == abbv).FirstOrDefault();

        public Version Create(Version version) 
        {
            _versions.InsertOne(version);
            return version;
        }

        public void Update(string abbv, Version newVersion) => _versions.ReplaceOne(version => version.Abbreviation == abbv, newVersion);
        public void Remove(Version idealVersion) => _versions.DeleteOne(version => version.Id == idealVersion.Id);
        public void Remove(string abbv) => _versions.DeleteOne(version => version.Abbreviation == abbv);
    }
}