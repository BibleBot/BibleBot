/*
* Copyright (C) 2016-2021 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using BibleBot.AutomaticServices.Models;
using BibleBot.AutomaticServices.Services.Providers;
using BibleBot.Lib;
using Microsoft.Extensions.Hosting;
using NodaTime;
using RestSharp;
using RestSharp.Serializers.SystemTextJson;
using Serilog;

namespace BibleBot.AutomaticServices.Services
{
    public class AutomaticDailyVerseService : IHostedService, IDisposable
    {
        private readonly GuildService _guildService;
        private readonly VersionService _versionService;

        private readonly SpecialVerseProvider _spProvider;
        private readonly List<IBibleProvider> _bibleProviders;

        private readonly RestClient _restClient;
        private Timer _timer;

        public AutomaticDailyVerseService(GuildService guildService, VersionService versionService,
                                          SpecialVerseProvider spProvider, BibleGatewayProvider bgProvider, APIBibleProvider abProvider)
        {
            _guildService = guildService;
            _versionService = versionService;
            _spProvider = spProvider;

            _bibleProviders = new List<IBibleProvider>
            {
                bgProvider,
                abProvider
            };

            _restClient = new RestClient("https://discord.com/api/webhooks");
            _restClient.UseSystemTextJson(new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            Log.Information("AutomaticDailyVerseService: Starting service...");

            _timer = new Timer(RunAutomaticDailyVerses, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));

            return Task.CompletedTask;
        }

        public async void RunAutomaticDailyVerses(object state)
        {
            var count = 0;
            var idealCount = 0;
            var guildsCleared = new List<string>();

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
                        return dateTimeInPreferredTz.Hour == int.Parse(guildTime[0])
                               && dateTimeInPreferredTz.Minute == int.Parse(guildTime[1])
                               && guild.DailyVerseLastSentDate != dateTimeInStandardTz.ToString("MM/dd/yyyy", null);
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
                if (!guildsCleared.Contains(guild.GuildId))
                {
                    var version = guild.Version != null ? guild.Version : "RSV";
                    var idealVersion = _versionService.Get(version);

                    if (idealVersion == null)
                    {
                        idealVersion = _versionService.Get("RSV");
                    }

                    string votdRef = _spProvider.GetDailyVerse().GetAwaiter().GetResult();
                    IBibleProvider provider = _bibleProviders.Where(pv => pv.Name == idealVersion.Source).FirstOrDefault();

                    if (provider != null)
                    {
                        Verse verse = provider.GetVerse(votdRef, true, true, idealVersion).GetAwaiter().GetResult();

                        var embed = new Utils().Embedify($"{verse.Reference.AsString} - {verse.Reference.Version.Name}", verse.Title, verse.Text, false, null);
                        var webhookRequestBody = new WebhookRequestBody
                        {
                            Content = "Here is the daily verse:",
                            Username = "BibleBot Automatic Daily Verses",
                            AvatarURL = embed.Footer.IconURL,
                            Embeds = new List<InternalEmbed> { embed }
                        };

                        var request = new RestRequest(guild.DailyVerseWebhook);
                        request.AddJsonBody(webhookRequestBody);

                        var resp = await _restClient.ExecuteAsync(request, Method.POST);
                        if (resp.StatusCode == System.Net.HttpStatusCode.NoContent)
                        {
                            count += 1;

                            guild.DailyVerseLastSentDate = dateTimeInStandardTz.ToString("MM/dd/yyyy", null);
                            _guildService.Update(guild.GuildId, guild);
                        }
                    }

                    guildsCleared.Add(guild.Id);
                }
            }

            Log.Information($"AutomaticDailyVerseService: Sent {(idealCount > 0 ? $"{count} of {idealCount}" : "0")} daily verse(s) at {dateTimeInStandardTz.ToString("h:mm tt x", new CultureInfo("en-US"))}.");
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            Log.Information("AutomaticDailyVerseService: Stopping service...");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
