using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BibleBot.Lib
{
    public class InternalEmbed 
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("type")]
        public string Type = "rich";

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("url")]
        public string URL { get; set; }

        [JsonPropertyName("color")]
        public int Color { get; set; }

        [JsonPropertyName("footer")]
        public Footer Footer { get; set; }

        [JsonPropertyName("image")]
        public Media Image { get; set; }

        [JsonPropertyName("thumbnail")]
        public Media Thumbnail { get; set; }

        [JsonPropertyName("video")]
        public Media Video { get; set; }

        [JsonPropertyName("provider")]
        public Provider Provider { get; set; }

        [JsonPropertyName("author")]
        public Author Author { get; set; }

        [JsonPropertyName("fields")]
        public List<EmbedField> Fields { get; set; }
    }

    public class Footer
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("icon_url")]
        public string IconURL { get; set; }

        [JsonPropertyName("proxy_icon_url")]
        public string ProxyIconURL { get; set; }
    }
    
    public class Media
    {
        [JsonPropertyName("url")]
        public string URL { get; set; }

        [JsonPropertyName("proxy_url")]
        public string ProxyURL { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }
    }

    public class Provider
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("url")]
        public string URL { get; set; }
    }

    public class Author
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("url")]
        public string URL { get; set; }

        [JsonPropertyName("icon_url")]
        public string IconURL { get; set; }

        [JsonPropertyName("proxy_icon_url")]
        public string ProxyIconURL { get; set; }
    }

    public class EmbedField
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("inline")]
        public bool Inline { get; set; }
    }
}