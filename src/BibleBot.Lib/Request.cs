using System.Text.Json.Serialization;

namespace BibleBot.Lib
{
    public class Request
    {
        [JsonPropertyName("userId")]
        public string UserId { get; set; }

        [JsonPropertyName("guildId")]
        public string GuildId { get; set; }

        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("body")]
        public string Body { get; set; }
    }
}
