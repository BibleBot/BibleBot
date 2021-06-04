using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using RestSharp;
using DSharpPlus;
using DSharpPlus.EventArgs;

using BibleBot.Lib;
using BibleBot.Frontend.Models;

namespace BibleBot.Frontend
{
	class Program
	{

		static void Main(string[] args)
		{
			MainAsync().GetAwaiter().GetResult();
		}

		static async Task MainAsync()
		{
			var bot = new DiscordClient(new DiscordConfiguration
			{
				Token = Environment.GetEnvironmentVariable("DISCORD_TOKEN"),
				TokenType = TokenType.Bot,
				Intents = DiscordIntents.AllUnprivileged
			});
			
			bot.MessageCreated += MessageCreatedHandler;

			bot.GuildCreated += UpdateTopggStats;
			bot.GuildDeleted += UpdateTopggStats;

			await bot.ConnectAsync();
			await Task.Delay(-1);
		}

		static Task UpdateTopggStats(DiscordClient s, DiscordEventArgs e)
		{
			_ = Task.Run(async () =>
			{
				var cli = new RestClient("https://top.gg/api");
				var req = new RestRequest($"bots/{s.CurrentUser.Id}/stats");
				req.AddHeader("Authorization", Environment.GetEnvironmentVariable("TOPGG_TOKEN"));
				req.AddJsonBody(new TopggStats
				{
					ServerCount = s.Guilds.Count()
				});

				await cli.PostAsync<IRestResponse>(req);
			});

			return Task.CompletedTask;
		}

		static Task MessageCreatedHandler(DiscordClient s, MessageCreateEventArgs e)
		{
			_ = Task.Run(async () =>
			{
				var utils = new Utils();
				var cli = new RestClient("http://localhost:5000/api");

				var acceptablePrefixes = new List<string>{ "+", "-", "!", "=", "$", "%", "^", "*", ".", ",", "?", "~", "|" };

				if (e.Author.Id.ToString() != "186046294286925824" || e.Author == s.CurrentUser)
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
					var request = new RestRequest("commands/process");
					request.AddJsonBody(requestObj);

					response = await cli.PostAsync<CommandResponse>(request);
				}
				else
				{
					var request = new RestRequest("verses/process");
					request.AddJsonBody(requestObj);

					response = await cli.PostAsync<VerseResponse>(request);
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
							await e.Message.RespondAsync(
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

							requestObj.Body = $"{webhook.Id}/{webhook.Token}";
							request.AddJsonBody(requestObj);

							await cli.PostAsync<CommandResponse>(request);
							await e.Message.RespondAsync(utils.Embed2Embed(commandResp.Pages[0]));
						}
						catch
						{
							await e.Message.RespondAsync(
								utils.Embedify("+dailyverse set", "I was unable to create a webhook for this channel. I need the **`Manage Webhooks`** permission to enable automatic daily verses.", false)
							);
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
		}
	}
}
