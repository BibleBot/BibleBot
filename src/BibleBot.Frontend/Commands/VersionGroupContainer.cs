/*
* Copyright (C) 2016-2021 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BibleBot.Frontend;
using BibleBot.Lib;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using RestSharp;

[SlashCommandGroup("version", "Set your preferred version, list all available versions, etc.")]
public class VersionGroupContainer : ApplicationCommandModule
{
    private RestClient cli = new RestClient(Environment.GetEnvironmentVariable("ENDPOINT"));

    [SlashCommand("set", "Set your preferred version.")]
    public async Task SetCommand(InteractionContext ctx, [Option("abbreviation", "Abbreviation of the version")] string abbv)
    {
        CommandResponse resp = await Utils.SubmitCommand(ctx, $"+version set {abbv}");
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(Utils.Embed2Embed(resp.Pages[0])));
    }

    [SlashCommand("setserver", "Set the server's preferred version.")]
    public async Task SetServerCommand(InteractionContext ctx, [Option("abbreviation", "Abbreviation of the version")] string abbv)
    {
        CommandResponse resp = await Utils.SubmitCommand(ctx, $"+version setserver {abbv}");
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(Utils.Embed2Embed(resp.Pages[0])));
    }

    [SlashCommand("info", "Get information on a version.")]
    public async Task InfoCommand(InteractionContext ctx, [Option("abbreviation", "Abbreviation of the version")] string abbv)
    {
        CommandResponse resp = await Utils.SubmitCommand(ctx, $"+version info {abbv}");
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(Utils.Embed2Embed(resp.Pages[0])));
    }

    [SlashCommand("list", "List all available versions.")]
    public async Task ListCommand(InteractionContext ctx)
    {
        // This sends a request to backend - only takes about 20ms.
        CommandResponse resp = await Utils.SubmitCommand(ctx, "+version list");

        if (resp.Pages.Count() > 1)
        {
            var properPages = new List<Page>();

            foreach (var page in resp.Pages)
            {
                properPages.Add(new Page
                {
                    Embed = Utils.Embed2Embed(page)
                });
            }

            await ctx.Interaction.SendPaginatedResponseAsync(false, ctx.User, properPages);
        }
        else
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(Utils.Embed2Embed(resp.Pages[0])));
        }
    }
}