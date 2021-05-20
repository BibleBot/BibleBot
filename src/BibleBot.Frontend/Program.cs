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
            
            bot.MessageCreated += async (s, e) =>
            {
                if (e.Author.Id.ToString() != "186046294286925824" || e.Author == bot.CurrentUser)
                {
                    return;
                }

                if (acceptablePrefixes.Contains(e.Message.Content.ElementAtOrDefault(0).ToString()))
                {
                    var authorAsMember = await e.Guild.GetMemberAsync(e.Author.Id);

                    var request = new RestRequest("api/commands/process");
                    request.AddJsonBody(new BibleBot.Lib.Request
                    {
                        UserId = e.Author.Id.ToString(),
                        UserPermissions = (long) authorAsMember.PermissionsIn(e.Channel),
                        GuildId = e.Guild.Id.ToString(),
                        Body = e.Message.Content,
                        Token = Environment.GetEnvironmentVariable("ENDPOINT_TOKEN")
                    });

                    var resp = await cli.PostAsync<CommandResponse>(request);
                    await e.Message.RespondAsync(utils.Embed2Embed(resp.Pages[0]));
                }
                else
                {
                    var authorAsMember = await e.Guild.GetMemberAsync(e.Author.Id);

                    var request = new RestRequest("api/verses/process");
                    request.AddJsonBody(new BibleBot.Lib.Request
                    {
                        UserId = e.Author.Id.ToString(),
                        UserPermissions = (long) authorAsMember.PermissionsIn(e.Channel),
                        GuildId = e.Guild.Id.ToString(),
                        Body = e.Message.Content,
                        Token = Environment.GetEnvironmentVariable("ENDPOINT_TOKEN")
                    });

                    var resp = await cli.PostAsync<VerseResponse>(request);
                    await e.Message.RespondAsync(resp.Verses[0].Text);
                }
            };

            await bot.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}
