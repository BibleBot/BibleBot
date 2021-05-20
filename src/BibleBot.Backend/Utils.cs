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

        public InternalEmbed Embedify(string title, string description, bool isError)
        {
            return Embedify(null, title, description, isError, null);
        }

        public InternalEmbed Embedify(string author, string title, string description, bool isError, string copyright)
        {
            // TODO: Do not use hard-coded version tags.
            string footerText = "BibleBot v9.1-beta by Kerygma Digital";

            var embed = new InternalEmbed();
            embed.Title = title;
            embed.Description = description;
            embed.Colour = isError ? (int) Colours.ERROR_COLOR : (int) Colours.NORMAL_COLOR;

            embed.Footer = new Footer();
            embed.Footer.Text = copyright != null ? $"{copyright} // ${footerText}" : footerText;
            // I'd put footer icon on this line but for some reason it doesn't get received by frontend.

            if (author != null)
            {
                embed.Author = new Author
                {
                    Name = author
                };
            } 
            

            return embed;
        }
    }
}