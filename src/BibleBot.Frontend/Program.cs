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
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BibleBot.Frontend.Models;
using BibleBot.Lib;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using RestSharp;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Sinks.SystemConsole.Themes;

namespace BibleBot.Frontend
{
    class Program
    {
        static DiscordShardedClient bot;
        static InteractionService interactionService;

        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Level:w4}] {Message:lj}{NewLine}{Exception}", theme: AnsiConsoleTheme.Code)
                .CreateLogger();

            Log.Information($"BibleBot v{Utils.Version} (Frontend) by Kerygma Digital");

            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            bot = new DiscordShardedClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged,
                LogLevel = LogSeverity.Error
            });
            interactionService = new InteractionService(bot.Rest);

            bot.Log += LogAsync;

            /*await bot.UseInteractivityAsync(new InteractivityConfiguration()
            {
                AckPaginationButtons = true,
                ButtonBehavior = ButtonPaginationBehavior.DeleteButtons,
                PaginationBehaviour = PaginationBehaviour.Ignore,
                PaginationDeletion = PaginationDeletion.DeleteEmojis,
                Timeout = TimeSpan.FromMinutes(2)
            });*/

            bot.ShardConnected += (s) => { Log.Information($"<global> shard {s.ShardId + 1} is connecting"); return Task.CompletedTask; };
            bot.ShardDisconnected += (e, s) => { Log.Information($"<global> shard {s.ShardId + 1} disconnected"); return Task.CompletedTask; };

            bot.ShardReady += UpdateStatus;
            bot.ShardReady += (s) => { Log.Information($"<global> shard {s.ShardId + 1} is ready"); return Task.CompletedTask; };

            bot.MessageReceived += MessageCreatedHandler;
            await interactionService.AddModulesAsync(Assembly.GetCallingAssembly(), null);
            await interactionService.RegisterCommandsGloballyAsync();

            bot.JoinedGuild += UpdateTopggStats;
            bot.LeftGuild += UpdateTopggStats;

            await bot.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DISCORD_TOKEN"));
            await bot.StartAsync();
            await Task.Delay(-1);
        }

        static Task UpdateStatus(DiscordSocketClient s)
        {
            _ = Task.Run(async () =>
            {
                await s.SetActivityAsync(new Game($"+biblebot v{Utils.Version} | Shard {s.ShardId + 1} / {bot.Shards.Count()}"));
            });

            return Task.CompletedTask;
        }

        static Task UpdateTopggStats(SocketGuild s)
        {
            _ = Task.Run(async () =>
            {
                int shardCount = bot.Shards.Count();
                int guildCount = 0;
                int userCount = 0;
                int channelCount = 0;

                foreach (var client in bot.Shards)
                {
                    guildCount += client.Guilds.Count();

                    foreach (var count in client.Guilds.Select((guild) => { return guild.MemberCount; }))
                    {
                        userCount += count;
                    }

                    foreach (var count in client.Guilds.Select((guild) => { return guild.Channels.Count(); }))
                    {
                        channelCount += count;
                    }
                }

                var cli = new RestClient("https://top.gg/api");
                var req = new RestRequest($"bots/361033318273384449/stats");
                req.AddHeader("Authorization", Environment.GetEnvironmentVariable("TOPGG_TOKEN"));
                req.AddHeader("Content-Type", "application/json");
                req.AddJsonBody(new TopggStats
                {
                    ServerCount = guildCount
                });

                await cli.ExecuteAsync(req, Method.POST);


                cli = new RestClient(Environment.GetEnvironmentVariable("ENDPOINT"));
                req = new RestRequest("stats/process");
                req.AddJsonBody(new BibleBot.Lib.Request
                {
                    Token = Environment.GetEnvironmentVariable("ENDPOINT_TOKEN"),
                    Body = $"{shardCount}||{guildCount}||{userCount}||{channelCount}"
                });
                await cli.PostAsync<CommandResponse>(req);

                Log.Information($"<global> sent stats to top.gg and backend");
            });

            return Task.CompletedTask;
        }

        static Task MessageCreatedHandler(SocketMessage s)
        {
            _ = Task.Run(async () =>
            {
                if (s.GetType() != typeof(SocketUserMessage)) { return; }

                var m = s as SocketUserMessage;
                var e = new SocketCommandContext(bot.GetShardFor(bot.GetGuild(m.Reference.GuildId.Value)), m);
                var cli = new RestClient(Environment.GetEnvironmentVariable("ENDPOINT"));

                if (e.User.Id == bot.CurrentUser.Id) { return; }

                string guildId;
                bool isDM = false;

                if (e.Channel.GetChannelType() == ChannelType.DM) { guildId = e.Channel.Id.ToString(); isDM = true; }
                else { guildId = e.Guild.Id.ToString(); }

                var msg = (new Regex(@"https?:")).Replace(e.Message.CleanContent, "");
                if (e.User.Id.ToString() == "186046294286925824") { msg = e.Message.CleanContent; }

                var requestObj = new BibleBot.Lib.Request
                {
                    UserId = e.User.Id.ToString(),
                    GuildId = guildId,
                    IsDM = isDM,
                    IsBot = e.User.IsBot,
                    Body = msg,
                    Token = Environment.GetEnvironmentVariable("ENDPOINT_TOKEN")
                };

                IRestResponse restResponse = null;
                VerseResponse response = null;

                /* if (msg.StartsWith("+stats"))
                {
                    int shardCount = bot.Shards.Count();
                    int guildCount = 0;
                    int userCount = 0;
                    int channelCount = 0;

                    foreach (var client in bot.Shards)
                    {
                        guildCount += client.Guilds.Count();

                        foreach (var count in client.Guilds.Select((guild) => { return guild.MemberCount; }))
                        {
                            userCount += count;
                        }

                        foreach (var count in client.Guilds.Select((guild) => { return guild.Channels.Count(); }))
                        {
                            channelCount += count;
                        }
                    }


                    var req = new RestRequest("stats/process");
                    req.AddJsonBody(new BibleBot.Lib.Request
                    {
                        Token = Environment.GetEnvironmentVariable("ENDPOINT_TOKEN"),
                        Body = $"{shardCount}||{guildCount}||{userCount}||{channelCount}"
                    });

                    restResponse = await cli.ExecuteAsync(req, Method.POST);
                } */

                if (msg.Contains(":"))
                {
                    var request = new RestRequest("verses/process");
                    request.AddJsonBody(requestObj);

                    restResponse = await cli.ExecuteAsync(request, Method.POST);


                    response = JsonSerializer.Deserialize<VerseResponse>(restResponse.Content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    var logStatement = $"[{e.Client.ShardId + 1}] <{e.User.Id}@{(requestObj.IsDM ? "Direct Messages" : e.Guild.Id)}#{e.Channel.Id}> {response.LogStatement}";
                    if (response.OK)
                    {
                        Log.Information(logStatement);
                    }
                    else if (response.LogStatement != null)
                    {
                        Log.Error(logStatement);
                    }

                    /* if (response.Type == "cmd")
                    {
                        var commandResp = response as CommandResponse;

                        if (commandResp.RemoveWebhook)
                        {
                            try
                            {
                                var ch = e.Channel as SocketTextChannel;
                                var webhooks = await ch.GetWebhooksAsync();

                                foreach (var webhook in webhooks)
                                {
                                    if (webhook.Creator.Id == bot.CurrentUser.Id) { await webhook.DeleteAsync(); }
                                }
                            }
                            catch
                            {
                                await e.Channel.SendMessageAsync(
                                    Utils.Embedify("+dailyverse set", "I was unable to remove our existing webhooks for this server. I need the **`Manage Webhooks`** permission to manage automatic daily verses.", false)
                                );
                            }
                        }

                        if (commandResp.CreateWebhook)
                        {
                            var request = new RestRequest("webhooks/process");

                            try
                            {
                                var ch = e.Channel as SocketTextChannel;
                                var webhook = await ch.CreateWebhookAsync("BibleBot Automatic Daily Verses");

                                requestObj.Body = $"{webhook.Id}/{webhook.Token}||{e.Channel.Id}";
                                request.AddJsonBody(requestObj);

                                await cli.PostAsync<CommandResponse>(request);
                                await e.Channel.SendMessageAsync(Utils.Embed2Embed(commandResp.Pages[0]));
                            }
                            catch
                            {
                                await e.Channel.SendMessageAsync(
                                    Utils.Embedify("+dailyverse set", "I was unable to create a webhook for this channel. I need the **`Manage Webhooks`** permission to enable automatic daily verses.", false)
                                );
                            }
                        }
                        else if (commandResp.SendAnnouncement)
                        {
                            // TODO(srp): This is basically broken beyond the first 25 servers in client.Value.Guilds.
                            var guilds = new List<SocketGuild>();
                            var guildsToIgnore = new List<string> { "Discord Bots", "Top.gg", "Discords.com" };
                            var preferredChannels = new List<string> { "misc", "bots", "meta", "hangout", "fellowship", "lounge", "congregation", "general", "bot-spam", "botspam", "staff" };
                            var count = 0;

                            await e.Channel.SendMessageAsync(Utils.Embed2Embed(commandResp.Pages[0]));

                            foreach (var client in bot.Shards)
                            {
                                foreach (var guild in client.Guilds) { guilds.Add(guild); }
                            }

                            foreach (var guild in guilds)
                            {
                                if (guildsToIgnore.Contains(guild.Name)) { continue; }

                                var sent = false;

                                foreach (var ch in guild.Channels)
                                {
                                    if (!sent && preferredChannels.Contains(ch.Name))
                                    {
                                        var perms = ch.GetPermissionOverwrite(bot.CurrentUser).Value.ToAllowList();

                                        if (perms.Contains(ChannelPermission.SendMessages) && perms.Contains(ChannelPermission.EmbedLinks))
                                        {
                                            await ch.SendMessageAsync(Utils.Embed2Embed(commandResp.Pages[0]));
                                            sent = true;
                                        }
                                    }
                                }

                                count += 1;
                                Log.Information($"Announcement {count}/{guilds.Count()} - {guild.Name}");
                                await e.Channel.SendMessageAsync($"{count}/{guilds.Count()} - {guild.Name}");
                            }
                        }
                        else if (commandResp.Pages != null)
                        {
                            if (commandResp.Pages.Count() > 1)
                            {
                                var properPages = new List<Page>();

                                var paginationEmojis = new PaginationEmojis();
                                paginationEmojis.SkipLeft = null;
                                paginationEmojis.SkipRight = null;
                                paginationEmojis.Left = DiscordEmoji.FromUnicode("⬅");
                                paginationEmojis.Right = DiscordEmoji.FromUnicode("➡");
                                paginationEmojis.Stop = DiscordEmoji.FromUnicode("❌");

                                if (commandResp.LogStatement.StartsWith("+resource"))
                                {
                                    paginationEmojis.SkipLeft = DiscordEmoji.FromUnicode("⏪");
                                    paginationEmojis.SkipRight = DiscordEmoji.FromUnicode("⏩");
                                }

                                foreach (var page in commandResp.Pages)
                                {
                                    properPages.Add(new Page
                                    {
                                        Embed = Utils.Embed2Embed(page)
                                    });
                                }

                                await e.Channel.SendPaginatedMessageAsync(e.Author, properPages, paginationEmojis, PaginationBehaviour.Ignore, PaginationDeletion.DeleteEmojis, TimeSpan.FromSeconds(180));
                            }
                            else
                            {
                                await e.Channel.SendMessageAsync(Utils.Embed2Embed(commandResp.Pages[0]));
                            }
                        }
                    }
                    else  */
                    /*if (response.Verses.Count() > 1 && response.Paginate)
                    {
                        var properPages = new List<Page>();

                        var paginationEmojis = new PaginationEmojis();
                        paginationEmojis.SkipLeft = null;
                        paginationEmojis.SkipRight = null;
                        paginationEmojis.Left = DiscordEmoji.FromUnicode("⬅");
                        paginationEmojis.Right = DiscordEmoji.FromUnicode("➡");
                        paginationEmojis.Stop = DiscordEmoji.FromUnicode("❌");

                        foreach (Verse verse in response.Verses)
                        {
                            var referenceTitle = $"{verse.Reference.AsString} - {verse.Reference.Version.Name}";

                            if (response.DisplayStyle == "embed")
                            {
                                properPages.Add(new Page
                                {
                                    Embed = Utils.Embedify(referenceTitle, verse.Title, verse.Text, false, null)
                                });
                            }
                            else if (response.DisplayStyle == "code")
                            {
                                verse.Text = verse.Text.Replace("*", "");
                                properPages.Add(new Page
                                {
                                    Content = $"**{referenceTitle}**\n\n```json\n{(verse.Title.Length > 0 ? $"{verse.Title}\n\n" : "")} {verse.Text}```"
                                });
                            }
                            else if (response.DisplayStyle == "blockquote")
                            {
                                properPages.Add(new Page
                                {
                                    Content = $"**{referenceTitle}**\n\n> {(verse.Title.Length > 0 ? $"**{verse.Title}**\n> \n> " : "")}{verse.Text}"
                                });
                            }
                        }

                        await e.Channel.SendPaginatedMessageAsync(e.Author, properPages, paginationEmojis, PaginationBehaviour.WrapAround, PaginationDeletion.DeleteEmojis, TimeSpan.FromSeconds(120));
                    }
                    else if (response.Verses.Count > 1 && !response.Paginate)
                    {
                        foreach (Verse verse in response.Verses)
                        {
                            var referenceTitle = $"{verse.Reference.AsString} - {verse.Reference.Version.Name}";

                            if (response.DisplayStyle == "embed")
                            {
                                var embed = Utils.Embedify(referenceTitle, verse.Title, verse.Text, false, null);
                                await e.Channel.SendMessageAsync(embed);
                            }
                            else if (response.DisplayStyle == "code")
                            {
                                verse.Text = verse.Text.Replace("*", "");
                                await e.Channel.SendMessageAsync($"**{referenceTitle}**\n\n```json\n{(verse.Title.Length > 0 ? $"{verse.Title}\n\n" : "")} {verse.Text}```");
                            }
                            else if (response.DisplayStyle == "blockquote")
                            {
                                await e.Channel.SendMessageAsync($"**{referenceTitle}**\n\n> {(verse.Title.Length > 0 ? $"**{verse.Title}**\n> \n> " : "")}{verse.Text}");
                            }
                        }
                    }
                    else if (response.Verses.Count == 1)
                    {
                        var verse = response.Verses[0];
                        var referenceTitle = $"{verse.Reference.AsString} - {verse.Reference.Version.Name}";

                        if (response.DisplayStyle == "embed")
                        {
                            var embed = Utils.Embedify(referenceTitle, verse.Title, verse.Text, false, null);
                            await e.Channel.SendMessageAsync(embed: embed);
                        }
                        else if (response.DisplayStyle == "code")
                        {
                            verse.Text = verse.Text.Replace("*", "");
                            await e.Channel.SendMessageAsync($"**{referenceTitle}**\n\n```json\n{(verse.Title.Length > 0 ? $"{verse.Title}\n\n" : "")} {verse.Text}```");
                        }
                        else if (response.DisplayStyle == "blockquote")
                        {
                            await e.Channel.SendMessageAsync($"**{referenceTitle}**\n\n> {(verse.Title.Length > 0 ? $"**{verse.Title}**\n> \n> " : "")}{verse.Text}");
                        }
                    }
                    else if (response.LogStatement.Contains("does not support the"))
                    {
                        await e.Channel.SendMessageAsync(embed: Utils.Embedify("Verse Error", response.LogStatement, true));
                    }*/
                }
            });

            return Task.CompletedTask;
        }

        static async Task LogAsync(LogMessage msg)
        {
            var severity = msg.Severity switch
            {
                LogSeverity.Critical => LogEventLevel.Fatal,
                LogSeverity.Error => LogEventLevel.Error,
                LogSeverity.Warning => LogEventLevel.Warning,
                LogSeverity.Info => LogEventLevel.Information,
                LogSeverity.Verbose => LogEventLevel.Verbose,
                LogSeverity.Debug => LogEventLevel.Debug,
                _ => LogEventLevel.Information
            };

            Log.Write(severity, msg.Exception, "({Source}) {Message:lj}{NewLine}{Exception}", msg.Source, msg.Message);

            await Task.CompletedTask;
        }
    }
}
