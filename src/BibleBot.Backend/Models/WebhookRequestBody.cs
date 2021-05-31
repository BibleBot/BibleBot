using System.Collections.Generic;
using System.Text.Json.Serialization;

using BibleBot.Lib;

namespace BibleBot.Backend.Models
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
        public string File = null;

        [JsonPropertyName("embeds")]
        public InternalEmbed[] Embeds { get; set; }

        [JsonPropertyName("payload_json")]
        public string PayloadJSON = null;

        [JsonPropertyName("allowed_mentions")]
        public string AllowedMentions = null;
    }
}