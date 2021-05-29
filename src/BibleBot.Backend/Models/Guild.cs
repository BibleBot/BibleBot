using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BibleBot.Backend.Models
{
    public class Guild
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("GuildId")]
        public string GuildId { get; set; }

        [BsonElement("Language")]
        public string Language { get; set; }

        [BsonElement("Version")]
        public string Version { get; set; }

        [BsonElement("Prefix")]
        public string Prefix { get; set; }

        [BsonElement("IgnoringBrackets")]
        public string IgnoringBrackets { get; set; }

        [BsonElement("DailyVerseWebhook")]
        public string DailyVerseWebhook { get; set; }

        [BsonElement("DailyVerseTime")]
        public string DailyVerseTime { get; set; }

        [BsonElement("DailyVerseTimeZone")]
        public string DailyVerseTimeZone { get; set; }

        [BsonElement("IsDM")]
        public bool IsDM { get; set; }
    }
}