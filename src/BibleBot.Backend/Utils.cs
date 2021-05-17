using BibleBot.Lib;

namespace BibleBot.Backend
{
    public class Utils
    {
        public enum Colours
        {
            NORMAL_COLOR = 6709986,
            ERROR_COLOR = 16723502
        }

        public DiscordEmbed Embedify(string title, string description, bool isError)
        {
            return Embedify(null, title, description, isError, null);
        }

        public DiscordEmbed Embedify(string author, string title, string description, bool isError, string copyright)
        {
            // TODO: Do not use hard-coded version tags.
            string footerText = "BibleBot v9.1-beta by Kerygma Digital";

            return new DiscordEmbed
            {
                Title = title,
                Description = description,
                URL = null,
                Colour = isError ? (int) Colours.ERROR_COLOR : (int) Colours.NORMAL_COLOR,
                Footer = new Footer
                {
                    Text = copyright != null? $"{copyright} // ${footerText}" : footerText,
                    IconURL = null,
                },
                Image = null,
                Thumbnail = null,
                Video = null,
                Author = author != null ? new Author { Name = author, URL = null, IconURL = null } : null,
                Fields = null
            };
        }
    }
}