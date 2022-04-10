/*
* Copyright (C) 2016-2021 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BibleBot.AutomaticServices.Models;
using BibleBot.Lib;

namespace BibleBot.AutomaticServices
{
    public class Utils
    {
        public enum Colors
        {
            NORMAL_COLOR = 6709986,
            ERROR_COLOR = 16723502
        }

        public static string Version = "9.2-beta";

        public InternalEmbed Embedify(string title, string description, bool isError)
        {
            return Embedify(null, title, description, isError, null);
        }

        public InternalEmbed Embedify(string author, string title, string description, bool isError, string copyright)
        {
            string footerText = $"BibleBot v{Utils.Version} by Kerygma Digital";

            var embed = new InternalEmbed();
            embed.Title = title;
            embed.Color = isError ? (int)Colors.ERROR_COLOR : (int)Colors.NORMAL_COLOR;

            embed.Footer = new Footer();
            embed.Footer.Text = copyright != null ? $"{copyright}\n{footerText}" : footerText;
            embed.Footer.IconURL = "https://i.imgur.com/hr4RXpy.png";

            if (description != null)
            {
                embed.Description = description;
            }

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
