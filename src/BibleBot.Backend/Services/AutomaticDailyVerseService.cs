using System;
using System.Linq;
using System.Threading;
using System.Text.Json;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NodaTime;
using RestSharp;
using RestSharp.Serializers.SystemTextJson;

using BibleBot.Lib;
using BibleBot.Backend.Models;
using BibleBot.Backend.Services.Providers;

namespace BibleBot.Backend.Services
{
    public class AutomaticDailyVerseService : IHostedService, IDisposable
    {
        private readonly ILogger<AutomaticDailyVerseService> _logger;

        private readonly GuildService _guildService;
        private readonly VersionService _versionService;
        
        private readonly BibleGatewayProvider _bgProvider;

        private readonly RestClient _restClient;
        private Timer _timer;

        public AutomaticDailyVerseService(ILogger<AutomaticDailyVerseService> logger, GuildService guildService, VersionService versionService, BibleGatewayProvider bibleGatewayProvider)
        {
            _logger = logger;
            _guildService = guildService;
            _versionService = versionService;
            _bgProvider = bibleGatewayProvider;
            _restClient = new RestClient("https://discord.com/api/webhooks");
            _restClient.UseSystemTextJson(new JsonSerializerOptions { IgnoreNullValues = true });
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting automatic daily verse service...");

            _timer = new Timer(RunAutomaticDailyVerses, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));

            return Task.CompletedTask;
        }

        public async void RunAutomaticDailyVerses(object state)
        {
            var count = 0;
            var idealCount = 0;

            Instant currentInstant = SystemClock.Instance.GetCurrentInstant();
            ZonedDateTime dateTimeInStandardTz = currentInstant.InZone(DateTimeZoneProviders.Tzdb["America/Detroit"]);

            var matches = _guildService.Get().Where<Guild>((guild) =>
            {
                if (guild.DailyVerseTime != null && guild.DailyVerseTimeZone != null && guild.DailyVerseWebhook != null)
                {
                    var guildTime = guild.DailyVerseTime.Split(":");
                    var preferredTimeZone = DateTimeZoneProviders.Tzdb[guild.DailyVerseTimeZone];
                    ZonedDateTime dateTimeInPreferredTz = currentInstant.InZone(preferredTimeZone);

                    try
                    {
                        return (dateTimeInPreferredTz.Hour == int.Parse(guildTime[0]) && dateTimeInPreferredTz.Minute == int.Parse(guildTime[1]));
                    }
                    catch
                    {
                        return false;
                    }
                }

                return false;
            });

            idealCount = matches.Count();

            foreach (Guild guild in matches)
            {
                var version = guild.Version != null ? guild.Version : "RSV";
                var idealVersion = _versionService.Get(version);

                if (idealVersion == null)
                {
                    idealVersion = _versionService.Get("RSV");
                }

                string votdRef = _bgProvider.GetDailyVerse().GetAwaiter().GetResult();
                Verse verse = _bgProvider.GetVerse(votdRef, true, true, idealVersion).GetAwaiter().GetResult();

                var embed = new Utils().Embedify($"{verse.Reference.AsString} - {verse.Reference.Version.Name}", verse.Title, verse.Text, false, null);
                var webhookRequestBody = new WebhookRequestBody
                {
                    Content = "Here is the daily verse:",
                    Username = "BibleBot Automatic Daily Verses",
                    AvatarURL = embed.Footer.IconURL,
                    Embeds = new InternalEmbed[] { embed }
                };

                var request = new RestRequest(guild.DailyVerseWebhook);
                request.AddJsonBody(webhookRequestBody);

                var resp = await _restClient.ExecuteAsync(request, Method.POST);
                if (resp.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    count += 1;
                }
            }

            _logger.LogInformation($"Sent {count} of {idealCount} daily verse(s) at {dateTimeInStandardTz.ToString("h:mm tt x", new CultureInfo("en-US"))}.");
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Stopping automatic daily verse service...");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}