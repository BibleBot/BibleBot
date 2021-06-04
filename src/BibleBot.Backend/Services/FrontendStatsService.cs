using BibleBot.Backend.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

namespace BibleBot.Backend.Services
{
    public class FrontendStatsService
    {
        private readonly IMongoCollection<FrontendStats> _frontendStats;
        
        public FrontendStatsService(IDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _frontendStats = database.GetCollection<FrontendStats>(settings.FrontendStatsCollectionName);
        }

        public FrontendStats Get() => _frontendStats.Find(frontendStats => true).FirstOrDefault();

        public FrontendStats Create(FrontendStats frontendStats) 
        {
            _frontendStats.InsertOne(frontendStats);
            return frontendStats;
        }

        public void Update(FrontendStats frontendStats) => _frontendStats.ReplaceOne(frontendStats => true, frontendStats);
    }
}