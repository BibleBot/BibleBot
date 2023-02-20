/*
* Copyright (C) 2016-2023 Kerygma Digital Co.
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
using Microsoft.Extensions.Hosting;
using NodaTime;
using RestSharp;
using RestSharp.Serializers.SystemTextJson;
using Serilog;

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

            _restClient = new RestClient("https://discord.com/api/webhooks");
            _restClient.UseSystemTextJson(new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            Log.Information("VersionStatsService: Starting service...");

            _timer = new Timer(RunVersionStats, null, TimeSpan.Zero, TimeSpan.FromHours(1));

            return Task.CompletedTask;
        }

        public async void RunVersionStats(object state)
        {
            Instant currentInstant = SystemClock.Instance.GetCurrentInstant();
            ZonedDateTime dateTimeInStandardTz = currentInstant.InZone(DateTimeZoneProviders.Tzdb["Europe/Amsterdam"]);

            bool sentStats = false;

            if (dateTimeInStandardTz.Day == 11 && dateTimeInStandardTz.Hour == 11)
            {
                var preferences = _userService.Get().Concat<IPreference>(_guildService.Get()).ToList();
                var versions = _versionService.Get();

                Dictionary<string, int> versionStats = new Dictionary<string, int>();

                foreach (var version in versions)
                {
                    if (!versionStats.ContainsKey(version.Abbreviation))
                    {
                        versionStats.Add(version.Abbreviation, 0);
                    }
                }

                foreach (var preference in preferences)
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

                string fileContents = "";

                var sortedStats = versionStats.ToList();
                sortedStats.Sort((p1, p2) => p2.Value.CompareTo(p1.Value));

                foreach (var kvp in sortedStats)
                {
                    fileContents += $"{kvp.Key},{kvp.Value}\n";
                }

                var webhookRequestBody = new WebhookRequestBody
                {
                    Content = $"henlo mr. <@304602975446499329>, here are those version stats you asked for:\n\n```\n{fileContents}\n```\n\nthese will be sent out on the 11th day of every month at the 11th hour in ur time zone, mr. <@304602975446499329>. ty have good day",
                    Username = "BibleBot Version Stats",
                    AvatarURL = "https://i.imgur.com/hr4RXpy.png"
                };

                var request = new RestRequest(Environment.GetEnvironmentVariable("STATS_WEBHOOK"));
                request.AddJsonBody(webhookRequestBody);

                var resp = await _restClient.ExecuteAsync(request, Method.POST);
                sentStats = resp.StatusCode == System.Net.HttpStatusCode.NoContent;
            }

            if (sentStats)
            {
                Log.Information("VersionStatsService: Sent version stats.");
            }
            else
            {
                Log.Information("VersionStatsService: Did not send version stats.");
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
            _timer?.Dispose();
        }
    }
}
