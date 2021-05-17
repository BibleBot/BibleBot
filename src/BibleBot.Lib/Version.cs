using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BibleBot.Lib
{
    public class Version
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set;}

        [BsonElement("Name")]
        public string Name { get; set; }

        [BsonElement("Abbreviation")]
        public string Abbreviation { get; set; }
        
        [BsonElement("Source")]
        public string Source { get; set; }

        [BsonElement("SupportsOldTestament")]
        public bool SupportsOldTestament { get; set; }

        [BsonElement("SupportsNewTestament")]
        public bool SupportsNewTestament { get; set; }

        [BsonElement("SupportsDeuterocanon")]
        public bool SupportsDeuterocanon { get; set; }
    }
}