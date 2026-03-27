/*
* Copyright (C) 2016-2026 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Net;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;

namespace BibleBot.Models
{
    /// <summary>
    /// The model for guild preferences.
    /// </summary>
    public class Guild : IPreference
    {
        /// <summary>
        /// The Discord Snowflake identifier of the guild.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// The default language of the guild.
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// The default version of the guild.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// The default display style for verses sent in the guild.
        /// </summary>
        public string DisplayStyle { get; set; } = "embed";

        /// <summary>
        /// The default brackets that BibleBot will ignore verse references within.
        /// </summary>
        /// <remarks>
        /// <c>&lt;&gt;</c> are persistent, changing this setting will only add another
        /// set of brackets to ignore within.
        /// </remarks>
        public string IgnoringBrackets { get; set; } = "<>";

        /// <summary>
        /// The Discord Snowflake identifier of the channel that the guild desires the daily verses be sent to.
        /// </summary>
        /// <remarks>
        /// This is used to compose the webhook URL in AutomaticServices.
        /// </remarks>
        public long DailyVerseChannelId { get; set; }

        /// <summary>
        /// The identifier of the webhook that frontend created. Surprisingly, this is one of the few Discord
        /// models that does not use the Snowflake.
        /// </summary>
        /// <remarks>
        /// This is used to compose the webhook URL in AutomaticServices.
        /// </remarks>
        public string DailyVerseWebhook { get; set; }

        /// <summary>
        /// The 24-hour timestamp when the daily verse should be sent in <see cref="DailyVerseTimeZone"/>.
        /// </summary>
        public string DailyVerseTime { get; set; }

        /// <summary>
        /// The TZ-database identifier for the time zone that should be consulted when sending daily verses.
        /// </summary>
        public string DailyVerseTimeZone { get; set; }

        /// <summary>
        /// The local date that the daily verse was last sent in.
        /// </summary>
        /// <remarks>
        /// This is used to avoid duplicate daily verses.
        /// </remarks>
        public string DailyVerseLastSentDate { get; set; }

        /// <summary>
        /// The Discord Snowflake identifier of the role that should be @mention'd when the daily verse is sent.
        /// </summary>
        public long DailyVerseRoleId { get; set; }

        /// <summary>
        /// Whether the DailyVerse channel is a thread.
        /// </summary>
        public bool DailyVerseIsThread { get; set; }

        /// <summary>
        /// The last HTTP status code of the webhook response.
        /// </summary>
        public HttpStatusCode DailyVerseLastStatusCode { get; set; }

        /// <summary>
        /// Indicates whether this preference represents a Direct Messages channel, instead of a proper guild.
        /// </summary>
        /// <remarks>
        /// As of writing (Jan. 8, 2024), this is only used to ensure that automatic daily verses cannot be
        /// setup in DMs as they do not support webhooks.
        /// </remarks>
        public bool IsDM { get; set; }
    }
}
