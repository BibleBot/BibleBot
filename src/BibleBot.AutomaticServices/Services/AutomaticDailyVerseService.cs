/*
* Copyright (C) 2016-2024 Kerygma Digital Co.
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
using BibleBot.Backend;
using BibleBot.Backend.Services;
using BibleBot.Backend.Services.Providers;
using BibleBot.Models;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
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
            int count = 0;
            int idealCount = 0;
            List<string> guildsCleared = new();

            Instant currentInstant = SystemClock.Instance.GetCurrentInstant();
            ZonedDateTime dateTimeInStandardTz = currentInstant.InZone(DateTimeZoneProviders.Tzdb["America/Detroit"]);

            IEnumerable<Guild> matches = (await _guildService.Get()).Where((guild) =>
            {
                if (guild.DailyVerseTime != null && guild.DailyVerseTimeZone != null && guild.DailyVerseWebhook != null)
                {
                    string[] guildTime = guild.DailyVerseTime.Split(":");
                    DateTimeZone preferredTimeZone = DateTimeZoneProviders.Tzdb[guild.DailyVerseTimeZone];
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
                    InternalEmbed embed;
                    WebhookRequestBody webhookRequestBody;
                    string version = guild.Version ?? "RSV";
                    Models.Version idealVersion = await _versionService.Get(version) ?? await _versionService.Get("RSV");

                    if (!idealVersion.SupportsOldTestament || !idealVersion.SupportsNewTestament)
                    {
                        embed = Utils.GetInstance().Embedify("BibleBot Automatic Daily Verse Notice", "Automatic daily verse will no longer support versions that do not have both Testaments. Please change your server's preferred version (`/setserverversion`) to one that has both.", true);
                        webhookRequestBody = new()
                        {
                            Username = "BibleBot Automatic Daily Verses",
                            AvatarURL = embed.Footer.IconURL,
                            Embeds = new List<InternalEmbed> { embed }
                        };
                    }
                    else
                    {
                        string votdRef = await _spProvider.GetDailyVerse();
                        IBibleProvider provider = _bibleProviders.FirstOrDefault(pv => pv.Name == idealVersion.Source);

                        if (provider == null)
                        {
                            continue;
                        }

                        Verse verse = await provider.GetVerse(votdRef, true, true, idealVersion);

                        // If API.Bible gives us a null result...
                        if (verse == null)
                        {
                            continue;
                        }

                        string content = guild.DailyVerseRoleId != null ? $"<@&{guild.DailyVerseRoleId}> - Here is the daily verse:" : "Here is the daily verse:";
                        embed = Utils.GetInstance().Embedify($"{verse.Reference.AsString} - {verse.Reference.Version.Name}", verse.Title, verse.Text, false, null);
                        webhookRequestBody = new()
                        {
                            Content = content,
                            Username = "BibleBot Automatic Daily Verses",
                            AvatarURL = embed.Footer.IconURL,
                            Embeds = new List<InternalEmbed> { embed }
                        };
                    }

                    RestRequest request = new(guild.DailyVerseWebhook);
                    request.AddJsonBody(webhookRequestBody);

                    IRestResponse resp = await _restClient.ExecuteAsync(request, Method.POST);
                    if (resp.StatusCode == System.Net.HttpStatusCode.NoContent)
                    {
                        count += 1;

                        UpdateDefinition<Guild> update = Builders<Guild>.Update
                                     .Set(guild => guild.DailyVerseLastSentDate, dateTimeInStandardTz.ToString("MM/dd/yyyy", null));

                        await _guildService.Update(guild.GuildId, update);
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

        public void Dispose() => _timer?.Dispose();
    }
}
