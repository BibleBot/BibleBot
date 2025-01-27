/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace BibleBot.Models
{
    public class Language
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("Name")]
        public string Name { get; set; }

        [BsonElement("Culture")]
        public string Culture { get; set; }

        [BsonElement("DefaultVersion")]
        public string DefaultVersion { get; set; }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member