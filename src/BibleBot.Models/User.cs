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
    [BsonIgnoreExtraElements]
    public class User : IPreference
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("UserId")]
        public string UserId { get; set; }

        [BsonElement("InputMethod")]
        public string InputMethod { get; set; } = "default";

        [BsonElement("Language")]
        public string Language { get; set; } = "english_us";

        [BsonElement("Version")]
        public string Version { get; set; } = "RSV";

        [BsonElement("TitlesEnabled")]
        public bool TitlesEnabled { get; set; } = true;

        [BsonElement("VerseNumbersEnabled")]
        public bool VerseNumbersEnabled { get; set; } = true;

        [BsonElement("PaginationEnabled")]
        public bool PaginationEnabled { get; set; } = false;

        [BsonElement("DisplayStyle")]
        public string DisplayStyle { get; set; } = "embed";
    }
}
