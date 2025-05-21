/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BibleBot.Models
{
    /// <summary>
    /// The model for guild preferences.
    /// </summary>
    [BsonIgnoreExtraElements]
    public class Guild : IPreference
    {
        /// <summary>
        /// The internal database ID.
        /// <br/><br/>
        /// <b>DO NOT USE THIS AS IF IT IS THE DISCORD ID OF THE GUILD.</b>
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        /// <summary>
        /// The Discord Snowflake identifier of the guild.
        /// </summary>
        [BsonElement("GuildId")]
        public string GuildId { get; init; }

        /// <summary>
        /// The default language of the guild.
        /// </summary>
        [BsonElement("Language")]
        public string Language { get; set; }

        /// <summary>
        /// The default version of the guild.
        /// </summary>
        [BsonElement("Version")]
        public string Version { get; set; }

        /// <summary>
        /// The default display style for verses sent in the guild.
        /// </summary>
        [BsonElement("DisplayStyle")]
        public string DisplayStyle { get; init; } = "embed";

        /// <summary>
        /// The default brackets that BibleBot will ignore verse references within.
        /// </summary>
        /// <remarks>
        /// <c>&lt;&gt;</c> are persistent, changing this setting will only add another
        /// set of brackets to ignore within.
        /// </remarks>
        [BsonElement("IgnoringBrackets")]
        public string IgnoringBrackets { get; init; } = "<>";

        /// <summary>
        /// The Discord Snowflake identifier of the channel that the guild desires the daily verses be sent to.
        /// </summary>
        /// <remarks>
        /// This is used to compose the webhook URL in AutomaticServices.
        /// </remarks>
        [BsonElement("DailyVerseChannelId")]
        public string DailyVerseChannelId { get; init; }

        /// <summary>
        /// The identifier of the webhook that frontend created. Surprisingly, this is one of the few Discord
        /// models that does not use the Snowflake.
        /// </summary>
        /// <remarks>
        /// This is used to compose the webhook URL in AutomaticServices.
        /// </remarks>
        [BsonElement("DailyVerseWebhook")]
        public string DailyVerseWebhook { get; init; }

        /// <summary>
        /// The 24-hour timestamp when the daily verse should be sent in <see cref="DailyVerseTimeZone"/>.
        /// </summary>
        [BsonElement("DailyVerseTime")]
        public string DailyVerseTime { get; init; }

        /// <summary>
        /// The TZ-database identifier for the time zone that should be consulted when sending daily verses.
        /// </summary>
        [BsonElement("DailyVerseTimeZone")]
        public string DailyVerseTimeZone { get; init; }

        /// <summary>
        /// The local date that the daily verse was last sent in.
        /// </summary>
        /// <remarks>
        /// This is used to avoid duplicate daily verses.
        /// </remarks>
        [BsonElement("DailyVerseLastSentDate")]
        public string DailyVerseLastSentDate { get; init; }

        /// <summary>
        /// The Discord Snowflake identifier of the role that should be @mention'd when the daily verse is sent.
        /// </summary>
        [BsonElement("DailyVerseRoleId")]
        public string DailyVerseRoleId { get; init; }

        /// <summary>
        /// Whether the DailyVerse channel is a thread.
        /// </summary>
        [BsonElement("DailyVerseIsThread")]
        public bool DailyVerseIsThread { get; init; }

        /// <summary>
        /// Indicates whether this preference represents a Direct Messages channel, instead of a proper guild.
        /// </summary>
        /// <remarks>
        /// As of writing (Jan. 8, 2024), this is only used to ensure that automatic daily verses cannot be
        /// setup in DMs as they do not support webhooks.
        /// </remarks>
        [BsonElement("IsDM")]
        public bool IsDM { get; init; }
    }
}
