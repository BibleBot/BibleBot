/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BibleBot.Models
{
    /// <summary>
    /// The model for Discord's webhook request body (see <seealso href="https://discord.com/developers/docs/resources/webhook#execute-webhook"/>).
    /// This is a bit of an anomaly in that the Discord API doesn't have a proper model for this, it's just a request body specification.
    /// </summary>
    /// <remarks>
    /// This is sent directly to a webhook URL. Frontend will never see this.
    /// </remarks>.
    public class WebhookRequestBody
    {
        /// <summary>
        /// The message content. Message content is limited to 2000 characters.
        /// </summary>
        [JsonPropertyName("content")]
        public string Content { get; set; }

        /// <summary>
        /// The username displayed on the message.
        /// </summary>
        /// <value>
        /// If set, Discord will use it as the username; otherwise, the name of the webhook will be used.
        /// </value>
        [JsonPropertyName("username")]
        public string Username { get; set; }

        /// <summary>
        /// The avatar URL displayed on the message.
        /// </summary>
        /// <value>
        /// If set, Discord will use it as the avatar; otherwise, the avatar of the webhook will be used.
        /// </value>
        [JsonPropertyName("avatar_url")]
        public string AvatarURL { get; set; }

        /// <summary>
        /// Indicates whether this is a text-to-speech message.
        /// </summary>
        [JsonPropertyName("tts")]
        public bool IsTTSMessage { get; set; }

        /// <summary>
        /// File contents attached to the message.
        /// </summary>
        /// <remarks>
        /// The type is purposefully incorrect as we never use this and it will otherwise default to null.
        /// </remarks>
        [JsonPropertyName("file")]
        public string File { get; set; }

        /// <summary>
        /// Embeds attached to the message. There can be a maximum of 10 embeds in a single message.
        /// </summary>
        [JsonPropertyName("embeds")]
        public List<InternalEmbed> Embeds { get; set; }

        /// <summary>
        /// Components attached to the message.
        /// </summary>
        [JsonPropertyName("components")]
        public List<IDiscordComponent> Components { get; set; }

        /// <summary>
        /// Discord describes this as "JSON encoded body of non-file params", used only when sending the request
        /// as <c>multipart/form-data</c>.
        /// </summary>
        [JsonPropertyName("payload_json")]
        public string PayloadJSON { get; set; }

        /// <summary>
        /// Allowed mentions for the message.
        /// </summary>
        /// <remarks>
        /// The type is purposefully incorrect as we never use this and it will otherwise default to null.
        /// </remarks>
        [JsonPropertyName("allowed_mentions")]
        public string AllowedMentions { get; set; }
    }
}
