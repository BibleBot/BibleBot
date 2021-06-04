using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BibleBot.Backend.Models
{
    public class FrontendStats
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("ShardCount")]
        public int ShardCount { get; set; }

        [BsonElement("ServerCount")]
        public int ServerCount { get; set; }

        [BsonElement("UserCount")]
        public int UserCount { get; set; }

        [BsonElement("ChannelCount")]
        public int ChannelCount { get; set; }
    }
}