using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BibleBot.Backend.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("UserId")]
        public string UserId { get; set; }

        [BsonElement("InputMethod")]
        public string InputMethod { get; set; }

        [BsonElement("Language")]
        public string Language { get; set; }

        [BsonElement("Version")]
        public string Version { get; set; }

        [BsonElement("TitlesEnabled")]
        public bool TitlesEnabled { get; set; }

        [BsonElement("VerseNumbersEnabled")]
        public bool VerseNumbersEnabled { get; set; }

        [BsonElement("DisplayMode")]
        public string DisplayMode { get; set; }
    }
}