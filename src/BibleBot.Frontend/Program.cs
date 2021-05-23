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
                    await e.Message.RespondAsync(utils.Embed2Embed((response as CommandResponse).Pages[0]));
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
            };

            await bot.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}
