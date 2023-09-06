/*
* Copyright (C) 2016-2023 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BibleBot.Models
{
    public class WebhookRequestBody
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("avatar_url")]
        public string AvatarURL { get; set; }

        [JsonPropertyName("tts")]
        public bool IsTTSMessage { get; set; }

        [JsonPropertyName("file")]
        public string File;

        [JsonPropertyName("embeds")]
        public List<InternalEmbed> Embeds { get; set; }

        [JsonPropertyName("payload_json")]
        public string PayloadJSON;

        [JsonPropertyName("allowed_mentions")]
        public string AllowedMentions;
    }
}
