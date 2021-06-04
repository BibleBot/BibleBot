using BibleBot.Backend.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System;

namespace BibleBot.Backend.Services
{
    public class GuildService
    {
        private readonly IMongoCollection<Guild> _guilds;
        
        public GuildService(IDatabaseSettings settings)
        {
            var client = new MongoClient(Environment.GetEnvironmentVariable("MONGODB_CONN"));
            var database = client.GetDatabase(settings.DatabaseName);

            _guilds = database.GetCollection<Guild>(settings.GuildCollectionName);
        }

        public List<Guild> Get() => _guilds.Find(guild => true).ToList();
        public Guild Get(string guildId) => _guilds.Find<Guild>(guild => guild.GuildId == guildId).FirstOrDefault();

        public Guild Create(Guild guild) 
        {
            _guilds.InsertOne(guild);
            return guild;
        }

        public void Update(string guildId, Guild newGuild) => _guilds.ReplaceOne(guild => guild.GuildId == guildId, newGuild);
        public void Remove(Guild idealGuild) => _guilds.DeleteOne(guild => guild.Id == idealGuild.Id);
        public void Remove(string guildId) => _guilds.DeleteOne(guild => guild.GuildId == guildId);
    }
}