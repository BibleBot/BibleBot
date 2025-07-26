/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using BibleBot.Backend;
using BibleBot.Backend.Services;
using BibleBot.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using MongoDB.Driver;
using NodaTime;
using RestSharp;
using RestSharp.Serializers.Json;
using Serilog;
using Version = BibleBot.Models.Version;

namespace BibleBot.AutomaticServices.Services
{
    public class AutomaticDailyVerseService : IHostedService, IDisposable
    {
        private readonly GuildService _guildService;
        private readonly VersionService _versionService;
        private readonly LanguageService _languageService;

        private readonly SpecialVerseProcessingService _specialVerseProcessingService;

        private readonly IStringLocalizer<AutomaticDailyVerseService> _localizer;

        private readonly ConcurrentDictionary<string, Guild> _previousMinuteFailedGuilds = new();

        private readonly RestClient _restClient;
        private Timer _timer;

        public AutomaticDailyVerseService(GuildService guildService, VersionService versionService, LanguageService languageService,
                                          SpecialVerseProcessingService specialVerseProcessingService,
                                          IStringLocalizer<AutomaticDailyVerseService> localizer)
        {
            _guildService = guildService;
            _versionService = versionService;
            _languageService = languageService;
            _specialVerseProcessingService = specialVerseProcessingService;
            _localizer = localizer;

            _restClient = new RestClient("https://discord.com/api/webhooks", configureSerialization: s => s.UseSystemTextJson(new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }));
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            Log.Information("AutomaticDailyVerseService: Starting service...");

            _timer = new Timer(RunAutomaticDailyVerses, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));

            return Task.CompletedTask;
        }

        private async void RunAutomaticDailyVerses(object state)
        {
            bool isTesting = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

            Instant currentInstant = SystemClock.Instance.GetCurrentInstant();
            ZonedDateTime dateTimeInStandardTz = currentInstant.InZone(DateTimeZoneProviders.Tzdb["America/Detroit"]);

            Log.Information($"AutomaticDailyVerseService: Fetching guilds to process for {dateTimeInStandardTz.ToString("h:mm tt x", new CultureInfo("en-US"))}...");

            ConcurrentBag<string> removedGuilds = [];

            List<Guild> matches = [.. (await _guildService.Get()).Where((guild) =>
                {
                    if (isTesting && guild.GuildId != "769709969796628500")
                    {
                        return false;
                    }

                    if (guild.DailyVerseTime == null || guild.DailyVerseTimeZone == null || guild.DailyVerseWebhook == null)
                    {
                        return false;
                    }

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
                }).Concat(_previousMinuteFailedGuilds.Values).Distinct()];

            int idealCount = matches.Count - _previousMinuteFailedGuilds.Count;
            int previousFailuresCount = _previousMinuteFailedGuilds.Count;
            Log.Information($"AutomaticDailyVerseService: Fetched {idealCount} (+ {previousFailuresCount}) guilds to process for {dateTimeInStandardTz.ToString("h:mm tt x", new CultureInfo("en-US"))}.");

            Stopwatch watch = Stopwatch.StartNew();
            ConcurrentDictionary<string, Task<VerseResult>> resultsByVersion = [];

            int maxConcurrentRequests = 10;
            SemaphoreSlim semaphore = new(maxConcurrentRequests);

            List<Task<bool>> tasks = [];
            tasks = [.. matches.Select(async guild =>
            {
                await semaphore.WaitAsync();
                try
                {
                    return await ProcessGuild(guild, resultsByVersion, dateTimeInStandardTz, removedGuilds);
                }
                catch (Exception ex)
                {
                    Log.Error($"AutomaticDailyVerseService: Caught unhandled exception, received {ex.Message} for guild {guild.GuildId}.");
                    return false;
                }
                finally
                {
                    semaphore.Release();
                }
            })];

            await Task.WhenAll(tasks);
            int count = tasks.Count(t => t.Result);

            watch.Stop();
            string timeToProcess = $"{(watch.Elapsed.Hours != 0 ? $"{watch.Elapsed.Hours} hours, " : "")}{(watch.Elapsed.Minutes != 0 ? $"{watch.Elapsed.Minutes} minutes, " : "")}{watch.Elapsed.Seconds} seconds";
            Log.Information($"AutomaticDailyVerseService: Sent {(idealCount > 0 ? $"{count} of {idealCount}" : "0")} (+{previousFailuresCount} / -{removedGuilds.Count}) daily verse(s) for {dateTimeInStandardTz.ToString("h:mm tt x", new CultureInfo("en-US"))} in {timeToProcess}.");
        }

        public async Task<bool> ProcessGuild(Guild guild, ConcurrentDictionary<string, Task<VerseResult>> resultsByVersion, ZonedDateTime dateTimeInStandardTz, ConcurrentBag<string> removedGuilds)
        {
            InternalEmbed embed;
            WebhookRequestBody webhookRequestBody;
            bool isSuccess = false;

            string version = guild.Version ?? "RSV";
            string culture = guild.Language ?? "en-US";

            Version idealVersion = await _versionService.Get(version) ?? await _versionService.Get("RSV");
            Language idealLanguage = await _languageService.Get(culture) ?? await _languageService.Get("en-US");

            CultureInfo.CurrentUICulture = new CultureInfo(idealLanguage.Culture);

            if (!idealVersion.SupportsOldTestament || !idealVersion.SupportsNewTestament)
            {
                embed = Utils.GetInstance().Embedify(_localizer["AutomaticDailyVerseWebhookUsernameAlt"], _localizer["AutomaticDailyVerseBothTestamentsWarning"], true);
                webhookRequestBody = new WebhookRequestBody
                {
                    Username = _localizer["AutomaticDailyVerseWebhookUsername"],
                    AvatarURL = embed.Footer.IconURL,
                    Embeds = [embed]
                };
            }
            else
            {
                Task<VerseResult> verseResultTask = resultsByVersion.GetOrAdd(idealVersion.Abbreviation, _ => _specialVerseProcessingService.GetDailyVerse(idealVersion, true, true));
                VerseResult verse = await verseResultTask;

                if (verse == null)
                {
                    return false;
                }

                string rolePing = guild.DailyVerseRoleId == guild.GuildId ? "@everyone" : $"<@&{guild.DailyVerseRoleId}>";
                string content = guild.DailyVerseRoleId != null ? $"{rolePing} - {_localizer["AutomaticDailyVerseLeadIn"]}:" : $"{_localizer["AutomaticDailyVerseLeadIn"]}:";
                embed = Utils.GetInstance().Embedify($"{verse.Reference.AsString} - {verse.Reference.Version.Name}", verse.Title, verse.Text, false, null);

                if (verse.Reference.Version.Publisher == "biblica")
                {
                    embed.Author.Name += " (Biblica)";
                    embed.Author.URL = "https://biblica.org";
                }

                webhookRequestBody = new WebhookRequestBody
                {
                    Content = content,
                    Username = _localizer["AutomaticDailyVerseWebhookUsername"],
                    AvatarURL = embed.Footer.IconURL,
                    Embeds = [embed]
                };
            }

            RestRequest request = new(guild.DailyVerseWebhook);
            request.AddJsonBody(webhookRequestBody);

            RestResponse resp = null;
            HttpStatusCode statusCode = HttpStatusCode.ServiceUnavailable;
            try
            {
                resp = await _restClient.PostAsync(request);
                statusCode = resp.StatusCode;
            }
            catch (HttpRequestException ex)
            {
                statusCode = (HttpStatusCode)ex.StatusCode;

                if (!_previousMinuteFailedGuilds.ContainsKey(guild.GuildId))
                {
                    Log.Error($"AutomaticDailyVerseService: Caught exception, received {statusCode} for guild {guild.GuildId}. Adding to failures queue...");
                    _previousMinuteFailedGuilds.TryAdd(guild.GuildId, guild);
                }
                else
                {
                    Log.Error($"AutomaticDailyVerseService: Failed guild {guild.GuildId} has failed again, removing from queue...");
                    _previousMinuteFailedGuilds.TryRemove(guild.GuildId, out _);
                }

                isSuccess = false;
            }

            UpdateDefinitionBuilder<Guild> update = Builders<Guild>.Update;
            List<UpdateDefinition<Guild>> updates = [];
            if (statusCode == HttpStatusCode.NoContent)
            {
                if (_previousMinuteFailedGuilds.TryRemove(guild.GuildId, out _))
                {
                    Log.Information($"AutomaticDailyVerseService: Failed guild {guild.GuildId} has been resent successfully.");
                }

                updates.Add(update.Set(guildToUpdate => guildToUpdate.DailyVerseLastSentDate, dateTimeInStandardTz.ToString("MM/dd/yyyy", null)));
                isSuccess = true;
            }
            else if (statusCode == HttpStatusCode.NotFound)
            {
                // This webhook no longer exists, so we'll remove the daily verse preferences of the guild.
                removedGuilds.Add(guild.GuildId);

                updates.Add(update.Set(guildToUpdate => guildToUpdate.DailyVerseTime, null));
                updates.Add(update.Set(guildToUpdate => guildToUpdate.DailyVerseTimeZone, null));
                updates.Add(update.Set(guildToUpdate => guildToUpdate.DailyVerseRoleId, null));
                updates.Add(update.Set(guildToUpdate => guildToUpdate.DailyVerseWebhook, null));
                updates.Add(update.Set(guildToUpdate => guildToUpdate.DailyVerseLastSentDate, null));

                isSuccess = false;
            }

            updates.Add(update.Set(guildToUpdate => guildToUpdate.DailyVerseLastStatusCode, statusCode));
            await _guildService.Update(guild.GuildId, update.Combine(updates));

            return isSuccess;
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
            if (!disposing || _timer == null)
            {
                return;
            }

            _timer.Dispose();
            _timer = null;
        }
    }
}
