using BibleBot.Lib;
using DSharpPlus.Entities;

namespace BibleBot.Frontend
{
    public class Utils
    {
        public enum Colours
        {
            NORMAL_COLOR = 6709986,
            ERROR_COLOR = 16723502
        }

        public DiscordEmbed Embed2Embed(InternalEmbed embed)
        {
            var builder = new DiscordEmbedBuilder();

            builder.WithTitle(embed.Title);
            builder.WithDescription(embed.Description);
            builder.WithColor(new DiscordColor((int) embed.Colour));
            builder.WithFooter(embed.Footer.Text, "https://i.imgur.com/hr4RXpy.png");
            
            if (embed.Author != null)
            {
                builder.WithAuthor(embed.Author.Name, null, null);
            }

            return builder.Build();
        }


        public DiscordEmbed Embedify(string title, string description, bool isError)
        {
            return Embedify(null, title, description, isError, null);
        }

        public DiscordEmbed Embedify(string author, string title, string description, bool isError, string copyright)
        {
            // TODO: Do not use hard-coded version tags.
            string footerText = "BibleBot v9.1-beta by Kerygma Digital";

            var builder = new DiscordEmbedBuilder();
            builder.WithTitle(title);
            builder.WithDescription(description);
            builder.WithColor(isError ? (int) Colours.ERROR_COLOR : (int) Colours.NORMAL_COLOR);

            builder.WithFooter(footerText, "https://i.imgur.com/hr4RXpy.png");

            if (author != null)
            {
                builder.WithAuthor(author, null, null);
            }

            return builder.Build();
        }
    }
}