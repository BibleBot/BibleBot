/*
* Copyright (C) 2016-2022 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BibleBot.Backend.Models
{
    public class Guild : IPreference
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

        [BsonElement("DisplayStyle")]
        public string DisplayStyle { get; set; }

        [BsonElement("IgnoringBrackets")]
        public string IgnoringBrackets { get; set; }

        [BsonElement("DailyVerseChannelId")]
        public string DailyVerseChannelId { get; set; }

        [BsonElement("DailyVerseWebhook")]
        public string DailyVerseWebhook { get; set; }

        [BsonElement("DailyVerseTime")]
        public string DailyVerseTime { get; set; }

        [BsonElement("DailyVerseTimeZone")]
        public string DailyVerseTimeZone { get; set; }

        [BsonElement("DailyVerseLastSentDate")]
        public string DailyVerseLastSentDate { get; set; }

        [BsonElement("IsDM")]
        public bool IsDM { get; set; }
    }
}
