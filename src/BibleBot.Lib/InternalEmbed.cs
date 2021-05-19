using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BibleBot.Lib
{
    public class InternalEmbed 
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string URL { get; set; }
        public int Colour { get; set; }
        public Footer Footer { get; set; }
        public Media Image { get; set; }
        public Media Thumbnail { get; set; }
        public Media Video { get; set; }
        public Author Author { get; set; }
        public List<EmbedField> Fields { get; set; }
    }

    public class Footer
    {
        public string Text { get; set; }
        public string IconURL { get; set; }
    }

    public class Media
    {
        public string URL { get; set; }
    }

    public class Author
    {
        public string Name { get; set; }
        public string URL { get; set; }
        public string IconURL { get; set; }
    }

    public class EmbedField
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool Inline { get; set; }
    }
}