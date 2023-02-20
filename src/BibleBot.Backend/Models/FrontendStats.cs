/*
* Copyright (C) 2016-2023 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

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
