/*
* Copyright (C) 2016-2021 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BibleBot.Backend.Models
{
    public class User : IPreference
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

        [BsonElement("DisplayStyle")]
        public string DisplayStyle { get; set; }
    }
}
