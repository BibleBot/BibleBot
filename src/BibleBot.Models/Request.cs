/*
* Copyright (C) 2016-2024 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Text.Json.Serialization;

namespace BibleBot.Models
{
    /// <summary>
    /// The model for request objects sent from BibleBot.Frontend.
    /// </summary>
    public class Request
    {
        /// <summary>
        /// The Discord Snowflake identifier of the <see cref="User"/> that created the request. 
        /// </summary>
        [JsonPropertyName("userId")]
        public string UserId { get; set; }

        /// <summary>
        /// The Discord Snowflake identifier of the <see cref="Guild"/> that the request was made in.
        /// If the request came from Direct Messages (see <see cref="IsDM"/>), this
        /// is the ID of the Direct Messages channel that the request was made in.
        /// </summary>
        [JsonPropertyName("guildId")]
        public string GuildId { get; set; }

        /// <summary>
        /// The Discord Snowflake identifier of the channel that the request was made in.
        /// If the request came from Direct Messages (see <see cref="IsDM"/>), the
        /// <see cref="GuildId"/> will match this value.
        /// </summary>
        [JsonPropertyName("channelId")]
        public string ChannelId { get; set; }

        /// <summary>
        /// This indicates whether the request came from Direct Messages.
        /// If it is, <see cref="GuildId"/> is the ID of the Direct Messages channel that the request was made in.
        /// </summary>
        [JsonPropertyName("isDM")]
        public bool IsDM { get; set; }

        /// <summary>
        /// This indicates whether the request was created by a bot.
        /// </summary>
        /// <remarks>
        /// This is only relevant for verse referencing. Once upon a
        /// time bots could perform commands but, with the advent of
        /// slash commands, this is no longer possible.
        /// </remarks>
        [JsonPropertyName("isBot")]
        public bool IsBot { get; set; }

        /// <summary>
        /// The authorization token of the request.
        /// </summary>
        [JsonPropertyName("token")]
        public string Token { get; set; }

        /// <summary>
        /// The content of the request, this may be a whole message or simply a command.
        /// </summary>
        [JsonPropertyName("body")]
        public string Body { get; set; }
    }
}
