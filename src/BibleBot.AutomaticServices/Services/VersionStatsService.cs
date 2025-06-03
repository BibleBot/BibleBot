/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using BibleBot.Backend.Services;
using BibleBot.Models;
using Microsoft.Extensions.Hosting;
using NodaTime;
using RestSharp;
using RestSharp.Serializers.Json;
using Serilog;
using Version = BibleBot.Models.Version;

namespace BibleBot.AutomaticServices.Services
{
    public class VersionStatsService : IHostedService, IDisposable
    {
        private readonly GuildService _guildService;
        private readonly UserService _userService;
        private readonly VersionService _versionService;

        private readonly RestClient _restClient;
        private Timer _timer;

        public VersionStatsService(GuildService guildService, UserService userService, VersionService versionService)
        {
            _guildService = guildService;
            _userService = userService;
            _versionService = versionService;

            _restClient = new RestClient("https://discord.com/api/webhooks", configureSerialization: s => s.UseSystemTextJson(new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }));
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            Log.Information("VersionStatsService: Starting service...");

            _timer = new Timer(RunVersionStats, null, TimeSpan.Zero, TimeSpan.FromHours(1));

            return Task.CompletedTask;
        }

        private async void RunVersionStats(object state)
        {
            try
            {
                Instant currentInstant = SystemClock.Instance.GetCurrentInstant();
                ZonedDateTime dateTimeInStandardTz = currentInstant.InZone(DateTimeZoneProviders.Tzdb["Europe/Amsterdam"]);

                bool sentStats = false;

                if (dateTimeInStandardTz is { Day: 11, Hour: 11 })
                {
                    var preferences = (await _userService.Get(isAutoServ: true)).Concat<IPreference>(await _guildService.Get(isAutoServ: true)).ToList();
                    List<Version> versions = await _versionService.Get();

                    Dictionary<string, int> versionStats = [];

                    foreach (Version version in versions)
                    {
                        versionStats.TryAdd(version.Abbreviation, 0);
                    }

                    foreach (IPreference preference in preferences)
                    {
                        try
                        {
                            versionStats[preference.Version] = versionStats[preference.Version] + 1;
                        }
                        catch (KeyNotFoundException)
                        {
                            versionStats["RSV"] = versionStats["RSV"] + 1;
                        }
                    }

                    StringBuilder fileContents = new();

                    var sortedStats = versionStats.ToList();
                    sortedStats.Sort((p1, p2) => p2.Value.CompareTo(p1.Value));

                    foreach (KeyValuePair<string, int> kvp in sortedStats)
                    {
                        fileContents.Append($"{kvp.Key},{kvp.Value}\n");
                    }

                    WebhookRequestBody webhookRequestBody = new()
                    {
                        Content = $"henlo <@304602975446499329>, here are those version stats you asked for:\n\n```\n{fileContents}\n```\n\nthese will be sent out on the 11th day of every month at the 11th hour in ur time zone, <@304602975446499329>. ty have good day",
                        Username = "BibleBot Version Stats",
                        AvatarURL = "https://i.imgur.com/hr4RXpy.png"
                    };

                    RestRequest request = new(Environment.GetEnvironmentVariable("STATS_WEBHOOK"));
                    request.AddJsonBody(webhookRequestBody);

                    RestResponse resp = await _restClient.PostAsync(request);
                    sentStats = resp.StatusCode == System.Net.HttpStatusCode.NoContent;
                }

                Log.Information(sentStats ? "VersionStatsService: Sent version stats." : "VersionStatsService: Did not send version stats.");
            }
            catch (Exception e)
            {
                Log.Error($"VersionStatsService: Exception caught - {e}");
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            Log.Information("VersionStatsService: Stopping service...");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || _timer == null)
            {
                return;
            }

            _timer.Dispose();
            _timer = null;
        }
    }
}
