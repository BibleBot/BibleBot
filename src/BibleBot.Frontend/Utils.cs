/*
* Copyright (C) 2016-2021 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using BibleBot.Lib;
using DSharpPlus.Entities;

namespace BibleBot.Frontend
{
    public class Utils
    {
        public enum Colors
        {
            NORMAL_COLOR = 6709986,
            ERROR_COLOR = 16723502
        }

        public static string Version = "9.1-beta";

        public DiscordEmbed Embed2Embed(InternalEmbed embed)
        {
            var builder = new DiscordEmbedBuilder();

            builder.WithTitle(embed.Title);
            builder.WithColor(new DiscordColor(embed.Color));
            builder.WithFooter(embed.Footer.Text, "https://i.imgur.com/hr4RXpy.png");

            if (embed.Author != null)
            {
                builder.WithAuthor(embed.Author.Name, null, null);
            }

            if (embed.Description != null)
            {
                builder.WithDescription(embed.Description);
            }

            if (embed.Fields != null)
            {
                foreach (EmbedField field in embed.Fields)
                {
                    builder.AddField(field.Name, field.Value, field.Inline);
                }
            }

            if (embed.Thumbnail != null)
            {
                builder.WithThumbnail(embed.Thumbnail.URL);
            }

            return builder.Build();
        }


        public DiscordEmbed Embedify(string title, string description, bool isError)
        {
            return Embedify(null, title, description, isError, null);
        }

        public DiscordEmbed Embedify(string author, string title, string description, bool isError, string copyright)
        {
            string footerText = $"BibleBot v{Utils.Version} by Kerygma Digital";

            var builder = new DiscordEmbedBuilder();
            builder.WithTitle(title);
            builder.WithDescription(description);
            builder.WithColor(isError ? (int)Colors.ERROR_COLOR : (int)Colors.NORMAL_COLOR);

            builder.WithFooter(footerText, "https://i.imgur.com/hr4RXpy.png");

            if (author != null)
            {
                builder.WithAuthor(author, null, null);
            }

            return builder.Build();
        }
    }
}
