using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BibleBot.Lib
{
    public interface IDiscordEmbed 
    {
        string Title { get; set; }
        string Description { get; set; }
        string URL { get; set; }
        int Colour { get; set; }
        IFooter Footer { get; set; }
        IMedia Image { get; set; }
        IMedia Thumbnail { get; set; }
        IMedia Video { get; set; }
        IAuthor Author { get; set; }
        List<IEmbedField> Fields { get; set; }
    }

    public interface IFooter
    {
        string Text { get; set; }
        
        [JsonPropertyName("icon_url")]
        string IconURL { get; set; }
    }

    public interface IMedia
    {
        string URL { get; set; }
    }

    public interface IAuthor
    {
        string Name { get; set; }
        string URL { get; set; }

        [JsonPropertyName("icon_url")]
        string IconURL { get; set; }
    }

    public interface IEmbedField
    {
        string Name { get; set; }
        string Value { get; set; }
        bool Inline { get; set; }
    }
}