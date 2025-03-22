/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Microsoft.Extensions.Localization;
using MongoDB.Driver;
using NodaTime;
using RestSharp;
using RestSharp.Serializers.Json;
using Serilog;

namespace BibleBot.AutomaticServices.Services
{
    public class AutomaticDailyVerseService : IHostedService, IDisposable
    {
        private readonly GuildService _guildService;
        private readonly VersionService _versionService;
        private readonly LanguageService _languageService;

        private readonly SpecialVerseProvider _spProvider;
        private readonly List<IBibleProvider> _bibleProviders;

        private readonly IStringLocalizer<AutomaticDailyVerseService> _localizer;

        private readonly RestClient _restClient;
        private Timer _timer;

        public AutomaticDailyVerseService(GuildService guildService, VersionService versionService, LanguageService languageService,
                                          SpecialVerseProvider spProvider, BibleGatewayProvider bgProvider, APIBibleProvider abProvider,
                                          IStringLocalizer<AutomaticDailyVerseService> localizer)
        {
            _guildService = guildService;
            _versionService = versionService;
            _languageService = languageService;
            _spProvider = spProvider;
            _localizer = localizer;

            _bibleProviders =
            [
                bgProvider,
                abProvider
            ];

            _restClient = new RestClient("https://discord.com/api/webhooks", configureSerialization: s => s.UseSystemTextJson(new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }));

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
            bool isTesting = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
            List<string> guildsCleared = [];

            Instant currentInstant = SystemClock.Instance.GetCurrentInstant();
            ZonedDateTime dateTimeInStandardTz = currentInstant.InZone(DateTimeZoneProviders.Tzdb["America/Detroit"]);

            IEnumerable<Guild> matches = (await _guildService.Get()).Where((guild) =>
            {
                if (isTesting && guild.GuildId != "769709969796628500")
                {
                    return false;
                }

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

            Stopwatch watch = Stopwatch.StartNew();

            foreach (Guild guild in matches)
            {
                if (!guildsCleared.Contains(guild.GuildId))
                {
                    InternalEmbed embed;
                    WebhookRequestBody webhookRequestBody;

                    string version = guild.Version ?? "RSV";
                    string culture = guild.Language ?? "en-US";

                    Models.Version idealVersion = await _versionService.Get(version) ?? await _versionService.Get("RSV");
                    Language idealLanguage = await _languageService.Get(culture) ?? await _languageService.Get("en-US");

                    CultureInfo.CurrentUICulture = new CultureInfo(idealLanguage.Culture);

                    if (!idealVersion.SupportsOldTestament || !idealVersion.SupportsNewTestament)
                    {
                        embed = Utils.GetInstance().Embedify(_localizer["AutomaticDailyVerseWebhookUsernameAlt"], _localizer["AutomaticDailyVerseBothTestamentsWarning"], true);
                        webhookRequestBody = new()
                        {
                            Username = _localizer["AutomaticDailyVerseWebhookUsername"],
                            AvatarURL = embed.Footer.IconURL,
                            Embeds = [embed]
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

                        string rolePing = guild.DailyVerseRoleId == guild.GuildId ? "@everyone" : $"<@&{guild.DailyVerseRoleId}>";
                        string content = guild.DailyVerseRoleId != null ? $"{rolePing} - {_localizer["AutomaticDailyVerseLeadIn"]}:" : $"{_localizer["AutomaticDailyVerseLeadIn"]}:";
                        embed = Utils.GetInstance().Embedify($"{verse.Reference.AsString} - {verse.Reference.Version.Name}", verse.Title, verse.Text, false, null);

                        if (verse.Reference.Version.Publisher == "biblica")
                        {
                            embed.Author.Name += " (Biblica)";
                            embed.Author.URL = "https://biblica.org";
                        }

                        webhookRequestBody = new()
                        {
                            Content = content,
                            Username = _localizer["AutomaticDailyVerseWebhookUsername"],
                            AvatarURL = embed.Footer.IconURL,
                            Embeds = [embed]
                        };
                    }

                    RestRequest request = new(guild.DailyVerseWebhook);
                    request.AddJsonBody(webhookRequestBody);

                    RestResponse resp = await _restClient.PostAsync(request);
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

            watch.Stop();
            string timeToProcess = $"{(watch.Elapsed.Hours != 0 ? $"{watch.Elapsed.Hours} hours, " : "")}{(watch.Elapsed.Minutes != 0 ? $"{watch.Elapsed.Minutes} minutes, " : "")}{watch.Elapsed.Seconds} seconds";
            Log.Information($"AutomaticDailyVerseService: Sent {(idealCount > 0 ? $"{count} of {idealCount}" : "0")} daily verse(s) for {dateTimeInStandardTz.ToString("h:mm tt x", new CultureInfo("en-US"))} in {timeToProcess}.");
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            Log.Information("AutomaticDailyVerseService: Stopping service...");

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
            if (disposing)
            {
                if (_timer != null)
                {
                    _timer.Dispose();
                    _timer = null;
                }
            }
        }
    }
}
