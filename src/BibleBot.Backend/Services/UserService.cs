using BibleBot.Backend.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

namespace BibleBot.Backend.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;
        
        public UserService(IDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
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