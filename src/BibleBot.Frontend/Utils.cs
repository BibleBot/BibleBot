/*
* Copyright (C) 2016-2021 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Threading.Tasks;
using BibleBot.Lib;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using RestSharp;

namespace BibleBot.Frontend
{
    public class Utils
    {
        private RestClient cli = new RestClient(Environment.GetEnvironmentVariable("ENDPOINT"));

        public enum Colors
        {
            NORMAL_COLOR = 6709986,
            ERROR_COLOR = 16723502
        }

        public static string Version = "9.1-beta";

        public static DiscordEmbed Embed2Embed(InternalEmbed embed)
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


        public static DiscordEmbed Embedify(string title, string description, bool isError)
        {
            return Embedify(null, title, description, isError, null);
        }

        public static DiscordEmbed Embedify(string author, string title, string description, bool isError, string copyright)
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

        private static Request CreateRequest(InteractionContext ctx, string body)
        {
            bool isDM = ctx.Channel.IsPrivate;
            string guildId = (isDM ? ctx.Channel.Id : ctx.Guild.Id).ToString();
            Permissions permissions = isDM ? Permissions.Administrator : ctx.Member.PermissionsIn(ctx.Channel);

            return new BibleBot.Lib.Request
            {
                UserId = ctx.User.Id.ToString(),
                UserPermissions = (long)permissions,
                GuildId = guildId,
                IsDM = isDM,
                Body = body,
                Token = Environment.GetEnvironmentVariable("ENDPOINT_TOKEN")
            };
        }

        public static async Task<CommandResponse> SubmitCommand(InteractionContext ctx, string body)
        {
            var req = Utils.CreateRequest(ctx, body);
            var restRequest = new RestRequest("commands/process");
            restRequest.AddJsonBody(req);

            RestClient cli = new RestClient(Environment.GetEnvironmentVariable("ENDPOINT"));
            return await cli.PostAsync<CommandResponse>(restRequest);
        }
    }
}
