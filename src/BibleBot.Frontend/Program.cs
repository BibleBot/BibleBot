using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using RestSharp;
using DSharpPlus;

using BibleBot.Lib;

namespace BibleBot.Frontend
{
    class Program
    {

        static void Main(string[] args)
        {
            MainAsync(new Utils()).GetAwaiter().GetResult();
        }

        static async Task MainAsync(Utils utils)
        {
            var bot = new DiscordClient(new DiscordConfiguration
            {
                Token = Environment.GetEnvironmentVariable("DISCORD_TOKEN"),
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged
            });

            var cli = new RestClient("http://localhost:5000");
            var acceptablePrefixes = new List<string>{ "+", "-", "!", "=", "$", "%", "^", "*", ".", ",", "?", "~", "|" };
            
            bot.MessageCreated += (s, e) =>
            {
                _ = Task.Run(async () =>
                {
                    if (e.Author.Id.ToString() != "186046294286925824" || e.Author == bot.CurrentUser)
                    {
                        return;
                    }

                    var authorAsMember = await e.Guild.GetMemberAsync(e.Author.Id);
                    var requestObj = new BibleBot.Lib.Request
                    {
                            UserId = e.Author.Id.ToString(),
                            UserPermissions = (long) authorAsMember.PermissionsIn(e.Channel),
                            GuildId = e.Guild.Id.ToString(),
                            IsDM = e.Channel.IsPrivate,
                            Body = e.Message.Content,
                            Token = Environment.GetEnvironmentVariable("ENDPOINT_TOKEN")
                    };
                    IResponse response;

                    if (acceptablePrefixes.Contains(e.Message.Content.ElementAtOrDefault(0).ToString()))
                    {

                        var request = new RestRequest("api/commands/process");
                        request.AddJsonBody(requestObj);

                        response = await cli.PostAsync<CommandResponse>(request);
                    }
                    else
                    {
                        var request = new RestRequest("api/verses/process");
                        request.AddJsonBody(requestObj);

                        response = await cli.PostAsync<VerseResponse>(request);
                    }

                    if (response.GetType().Equals(typeof(CommandResponse)))
                    {
                        var commandResp = response as CommandResponse;

                        if (commandResp.WebhookCallback == true)
                        {
                            var request = new RestRequest("api/webhooks/process");
                            var existingWebhooks = (await e.Channel.GetWebhooksAsync()).Where((webhook) =>
                            {
                                return webhook.User.Id == bot.CurrentUser.Id;
                            });

                            foreach (var existingWebhook in existingWebhooks)
                            {
                                await existingWebhook.DeleteAsync();
                            }

                            var webhook = await e.Channel.CreateWebhookAsync("BibleBot Automatic Daily Verses", default, "For automatic daily verses from BibleBot.");
                            
                            requestObj.Body = $"{webhook.Id}/{webhook.Token}";
                            request.AddJsonBody(requestObj);

                            var webhookResp = await cli.PostAsync<CommandResponse>(request);

                            if (webhookResp.OK == false)
                            {
                                await e.Message.RespondAsync(
                                    utils.Embedify("+dailyverse set", "I was unable to create a webhook for this channel. I need the **`Manage Webhooks`** permission to enable automatic daily verses.", false)
                                );
                            }
                            else
                            {
                                await e.Message.RespondAsync(utils.Embed2Embed(commandResp.Pages[0]));
                            }
                        }
                        else
                        {
                            await e.Message.RespondAsync(utils.Embed2Embed(commandResp.Pages[0]));
                        }
                    }
                    else if (response.GetType().Equals(typeof(VerseResponse)))
                    {
                        var verseResp = response as VerseResponse;

                        foreach (Verse verse in verseResp.Verses)
                        {
                            var embed = utils.Embedify($"{verse.Reference.ToString()} - {verse.Reference.Version.Name}", verse.Title, verse.Text, false, null);
                            await e.Message.RespondAsync(embed);
                        }
                    }
                });

                return Task.CompletedTask;
            };

            await bot.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}
