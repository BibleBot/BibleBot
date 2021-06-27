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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BibleBot.Frontend.Models;
using BibleBot.Lib;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using RestSharp;
using Serilog;
using Serilog.Extensions.Logging;
using Serilog.Sinks.SystemConsole.Themes;

namespace BibleBot.Frontend
{
    class Program
    {
        static DiscordShardedClient bot;

        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(outputTemplate: "[{Level:w4}] {Message:lj}{NewLine}{Exception}", theme: AnsiConsoleTheme.Code)
                .CreateLogger();

            Log.Information($"BibleBot v{Utils.Version} (Frontend) by Kerygma Digital");

            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            bot = new DiscordShardedClient(new DiscordConfiguration
            {
                Token = Environment.GetEnvironmentVariable("DISCORD_TOKEN"),
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged,
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Error
            });

            await bot.UseInteractivityAsync();

            bot.SocketOpened += (s, e) => { Log.Information($"<global> shard {s.ShardId + 1} is connecting"); return Task.CompletedTask; };
            bot.SocketClosed += (s, e) => { Log.Information($"<global> shard {s.ShardId + 1} disconnected"); return Task.CompletedTask; };

            bot.Ready += UpdateStatus;
            bot.Ready += (s, e) => { Log.Information($"<global> shard {s.ShardId + 1} is ready"); return Task.CompletedTask; };

            bot.Resumed += (s, e) => { Log.Information($"<global> shard {s.ShardId + 1} resumed"); return Task.CompletedTask; };

            bot.MessageCreated += MessageCreatedHandler;

            bot.GuildCreated += UpdateTopggStats;
            bot.GuildDeleted += UpdateTopggStats;

            await bot.StartAsync();
            await Task.Delay(-1);
        }

        static Task UpdateStatus(DiscordClient s, DiscordEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                await s.UpdateStatusAsync(new DiscordActivity
                {
                    Name = $"+biblebot v{Utils.Version} | Shard {s.ShardId + 1} / {s.ShardCount}",
                    ActivityType = ActivityType.Playing
                });
            });

            return Task.CompletedTask;
        }

        static Task UpdateTopggStats(DiscordClient s, DiscordEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                int shardCount = bot.ShardClients.Count();
                int guildCount = 0;
                int userCount = 0;
                int channelCount = 0;

                foreach (var client in bot.ShardClients)
                {
                    guildCount += client.Value.Guilds.Count();

                    foreach (var count in client.Value.Guilds.Select((guild) => { return guild.Value.MemberCount; }))
                    {
                        userCount += count;
                    }

                    foreach (var count in client.Value.Guilds.Select((guild) => { return guild.Value.Channels.Count(); }))
                    {
                        channelCount += count;
                    }
                }

                var cli = new RestClient("https://top.gg/api");
                var req = new RestRequest($"bots/{bot.CurrentUser.Id}/stats");
                req.AddHeader("Authorization", Environment.GetEnvironmentVariable("TOPGG_TOKEN"));
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

        static Task MessageCreatedHandler(DiscordClient s, MessageCreateEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                var utils = new Utils();
                var cli = new RestClient(Environment.GetEnvironmentVariable("ENDPOINT"));

                var acceptablePrefixes = new List<string> { "+", "-", "!", "=", "$", "%", "^", "*", ".", ",", "?", "~", "|" };

                if (e.Author == s.CurrentUser)
                {
                    return;
                }

                Permissions permissions = Permissions.None;
                string guildId;
                bool isDM = false;

                if (e.Channel.IsPrivate)
                {
                    permissions = Permissions.Administrator;
                    guildId = e.Channel.Id.ToString();
                    isDM = true;
                }
                else
                {
                    permissions = (await e.Guild.GetMemberAsync(e.Author.Id)).PermissionsIn(e.Channel);
                    guildId = e.Guild.Id.ToString();
                }

                var msg = (new Regex(@"https?:")).Replace(e.Message.Content, "");
                var requestObj = new BibleBot.Lib.Request
                {
                    UserId = e.Author.Id.ToString(),
                    UserPermissions = (long)permissions,
                    GuildId = guildId,
                    IsDM = isDM,
                    Body = msg,
                    Token = Environment.GetEnvironmentVariable("ENDPOINT_TOKEN")
                };

                IResponse response = null;


                if (acceptablePrefixes.Contains(msg.ElementAtOrDefault(0).ToString()))
                {
                    if (msg.StartsWith("+stats"))
                    {
                        int shardCount = bot.ShardClients.Count();
                        int guildCount = 0;
                        int userCount = 0;
                        int channelCount = 0;

                        foreach (var client in bot.ShardClients)
                        {
                            guildCount += client.Value.Guilds.Count();

                            foreach (var count in client.Value.Guilds.Select((guild) => { return guild.Value.MemberCount; }))
                            {
                                userCount += count;
                            }

                            foreach (var count in client.Value.Guilds.Select((guild) => { return guild.Value.Channels.Count(); }))
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

                        await cli.PostAsync<CommandResponse>(req);
                    }

                    var request = new RestRequest("commands/process");
                    request.AddJsonBody(requestObj);

                    response = await cli.PostAsync<CommandResponse>(request);
                }
                else if (msg.Contains(":"))
                {
                    var request = new RestRequest("verses/process");
                    request.AddJsonBody(requestObj);

                    response = await cli.PostAsync<VerseResponse>(request);
                }

                var logStatement = $"[{s.ShardId + 1}] <{e.Author.Id}@{(requestObj.IsDM ? "Direct Messages" : e.Guild.Id)}#{e.Channel.Id}> {response.LogStatement}";
                if (response.OK)
                {
                    Log.Information(logStatement);
                }
                else if (response.LogStatement != null)
                {
                    Log.Error(logStatement);
                }

                if (response.GetType().Equals(typeof(CommandResponse)))
                {
                    var commandResp = response as CommandResponse;

                    if (commandResp.RemoveWebhook)
                    {
                        try
                        {
                            var webhooks = await e.Guild.GetWebhooksAsync();

                            foreach (var webhook in webhooks)
                            {
                                if (webhook.User.Id == s.CurrentUser.Id)
                                {
                                    await webhook.DeleteAsync();
                                }
                            }
                        }
                        catch
                        {
                            await e.Channel.SendMessageAsync(
                                utils.Embedify("+dailyverse set", "I was unable to remove our existing webhooks for this server. I need the **`Manage Webhooks`** permission to manage automatic daily verses.", false)
                            );
                        }
                    }

                    if (commandResp.CreateWebhook)
                    {
                        var request = new RestRequest("webhooks/process");

                        try
                        {
                            var webhook = await e.Channel.CreateWebhookAsync("BibleBot Automatic Daily Verses", default, "For automatic daily verses from BibleBot.");

                            requestObj.Body = $"{webhook.Id}/{webhook.Token}||{e.Channel.Id}";
                            request.AddJsonBody(requestObj);

                            await cli.PostAsync<CommandResponse>(request);
                            await e.Channel.SendMessageAsync(utils.Embed2Embed(commandResp.Pages[0]));
                        }
                        catch
                        {
                            await e.Channel.SendMessageAsync(
                                utils.Embedify("+dailyverse set", "I was unable to create a webhook for this channel. I need the **`Manage Webhooks`** permission to enable automatic daily verses.", false)
                            );
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
                                    Embed = utils.Embed2Embed(page)
                                });
                            }

                            await e.Channel.SendPaginatedMessageAsync(e.Author, properPages, paginationEmojis, PaginationBehaviour.Ignore, PaginationDeletion.DeleteEmojis, TimeSpan.FromSeconds(180));
                        }
                        else
                        {
                            await e.Channel.SendMessageAsync(utils.Embed2Embed(commandResp.Pages[0]));
                        }
                    }
                }
                else if (response.GetType().Equals(typeof(VerseResponse)))
                {
                    var verseResp = response as VerseResponse;

                    if (verseResp.Verses.Count() > 1)
                    {
                        var properPages = new List<Page>();

                        var paginationEmojis = new PaginationEmojis();
                        paginationEmojis.SkipLeft = null;
                        paginationEmojis.SkipRight = null;
                        paginationEmojis.Left = DiscordEmoji.FromUnicode("⬅");
                        paginationEmojis.Right = DiscordEmoji.FromUnicode("➡");
                        paginationEmojis.Stop = DiscordEmoji.FromUnicode("❌");

                        foreach (Verse verse in verseResp.Verses)
                        {
                            var referenceTitle = $"{verse.Reference.AsString} - {verse.Reference.Version.Name}";

                            if (verseResp.DisplayStyle == "embed")
                            {
                                properPages.Add(new Page
                                {
                                    Embed = utils.Embedify(referenceTitle, verse.Title, verse.Text, false, null)
                                });
                            }
                            else if (verseResp.DisplayStyle == "code")
                            {
                                verse.Text = verse.Text.Replace("*", "");
                                properPages.Add(new Page
                                {
                                    Content = $"**{referenceTitle}**\n\n```json\n{(verse.Title.Length > 0 ? $"{verse.Title}\n\n" : "")} {verse.Text}```"
                                });
                            }
                            else if (verseResp.DisplayStyle == "blockquote")
                            {
                                properPages.Add(new Page
                                {
                                    Content = $"**{referenceTitle}**\n\n> {(verse.Title.Length > 0 ? $"**{verse.Title}**\n> \n> " : "")}{verse.Text}"
                                });
                            }
                        }

                        await e.Channel.SendPaginatedMessageAsync(e.Author, properPages, paginationEmojis, PaginationBehaviour.WrapAround, PaginationDeletion.DeleteEmojis, TimeSpan.FromSeconds(120));
                    }
                    else if (verseResp.Verses.Count == 1)
                    {
                        var verse = verseResp.Verses[0];
                        var referenceTitle = $"{verse.Reference.AsString} - {verse.Reference.Version.Name}";

                        if (verseResp.DisplayStyle == "embed")
                        {
                            var embed = utils.Embedify(referenceTitle, verse.Title, verse.Text, false, null);
                            await e.Channel.SendMessageAsync(embed);
                        }
                        else if (verseResp.DisplayStyle == "code")
                        {
                            verse.Text = verse.Text.Replace("*", "");
                            await e.Channel.SendMessageAsync($"**{referenceTitle}**\n\n```json\n{(verse.Title.Length > 0 ? $"{verse.Title}\n\n" : "")} {verse.Text}```");
                        }
                        else if (verseResp.DisplayStyle == "blockquote")
                        {
                            await e.Channel.SendMessageAsync($"**{referenceTitle}**\n\n> {(verse.Title.Length > 0 ? $"**{verse.Title}**\n> \n> " : "")}{verse.Text}");
                        }
                    }
                    else if (verseResp.LogStatement.Contains("does not support the"))
                    {
                        await e.Channel.SendMessageAsync(utils.Embedify("Verse Error", verseResp.LogStatement, true));
                    }
                }
            });

            return Task.CompletedTask;
        }
    }
}
