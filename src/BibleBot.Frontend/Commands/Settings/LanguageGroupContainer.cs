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
/*using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using RestSharp;

[SlashCommandGroup("language", "Set your preferred language, list all available language, etc.")]
public class FormattingGroupContainer : ApplicationCommandModule
{
    private RestClient cli = new RestClient(Environment.GetEnvironmentVariable("ENDPOINT"));

    [SlashCommand("set", "Set your preferred language.")]
    public async Task SetCommand(InteractionContext ctx, [Option("obj_name", "Object name of the language")] string objName)
    {
        CommandResponse resp = await Utils.SubmitCommand(ctx, $"+language set {objName}");
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(Utils.Embed2Embed(resp.Pages[0])));
    }

    [SlashCommand("setserver", "Set the server's preferred language.")]
    public async Task SetServerCommand(InteractionContext ctx, [Option("obj_name", "Object name of the language")] string objName)
    {
        CommandResponse resp = await Utils.SubmitCommand(ctx, $"+language setserver {objName}");
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(Utils.Embed2Embed(resp.Pages[0])));
    }

    [SlashCommand("list", "List all available languages.")]
    public async Task ListCommand(InteractionContext ctx)
    {
        CommandResponse resp = await Utils.SubmitCommand(ctx, "+language list");

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AddEmbed(Utils.Embed2Embed(resp.Pages[0])));
    }
}*/