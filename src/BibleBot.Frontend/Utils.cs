using BibleBot.Lib;
using DSharpPlus.Entities;

namespace BibleBot.Frontend
{
    public class Utils
    {
        public DiscordEmbed Embed2Embed(InternalEmbed embed)
        {
            var builder = new DiscordEmbedBuilder();

            var footerText = 

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
    }
}