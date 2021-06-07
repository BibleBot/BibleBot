using System.Linq;
using System.Collections.Generic;

using BibleBot.Backend.Models;

using BibleBot.Lib;

namespace BibleBot.Backend
{
    public class Utils
    {
        public enum Colors
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
            string footerText = "BibleBot v9.1 by Kerygma Digital";

            var embed = new InternalEmbed();
            embed.Title = title;
            embed.Description = description;
            embed.Color = isError ? (int) Colors.ERROR_COLOR : (int) Colors.NORMAL_COLOR;

            embed.Footer = new Footer();
            embed.Footer.Text = copyright != null ? $"{copyright}\n{footerText}" : footerText;
            embed.Footer.IconURL = "https://i.imgur.com/hr4RXpy.png";

            if (author != null)
            {
                embed.Author = new Author
                {
                    Name = author
                };
            }
            

            return embed;
        }

        public List<InternalEmbed> EmbedifyResource(IResource resource, string section, int page)
        {
            if (resource.Style == ResourceStyle.FULL_TEXT)
            {
                if (resource.Type == ResourceType.CREED)
                {
                    var creedResource = resource as CreedResource;
                    string copyright = null;

                    if ((new string[] {"apostles", "nicene"}).Contains(creedResource.CommandReference))
                    {
                        copyright = "© 1998 English Language Liturgical Consultation (ELLC)";
                    }


                    return new List<InternalEmbed>
                    {
                        Embedify(null, creedResource.Title, creedResource.Text, false, copyright)
                    };
                }
            }
            else if (resource.Style == ResourceStyle.PARAGRAPHED)
            {
                return null;
            }
            else if (resource.Style == ResourceStyle.SECTIONED)
            {
                return null;
            }

            return null;
        }
    }
}