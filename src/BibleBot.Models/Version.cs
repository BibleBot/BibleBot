/*
* Copyright (C) 2016-2023 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BibleBot.Models
{
    public class Version
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

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
