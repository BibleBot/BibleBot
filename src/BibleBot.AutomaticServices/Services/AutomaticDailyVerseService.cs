/*
* Copyright (C) 2016-2026 Kerygma Digital Co.
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using NodaTime;
using RestSharp;
using RestSharp.Serializers.Json;
using Serilog;
using Version = BibleBot.Models.Version;

namespace BibleBot.AutomaticServices.Services
{
    public class AutomaticDailyVerseService : IHostedService, IDisposable
    {
        private readonly IServiceScopeFactory _serviceServiceScopeFactory;
        private readonly IStringLocalizer<AutomaticDailyVerseService> _localizer;

        private readonly ConcurrentDictionary<long, Guild> _previousMinuteFailedGuilds = new();

        private readonly RestClient _restClient;
        private Timer _timer;

        public AutomaticDailyVerseService(IServiceScopeFactory serviceScopeFactory,
                                          IStringLocalizer<AutomaticDailyVerseService> localizer)
        {
            _serviceServiceScopeFactory = serviceScopeFactory;
            _localizer = localizer;

            _restClient = new RestClient("https://discord.com/api/webhooks", configureSerialization: s => s.UseSystemTextJson(new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, IncludeFields = true }));
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

            ConcurrentBag<long> removedGuilds = [];

            List<Guild> matches;
            // Use a dedicated scope just for fetching the guild list.
            using (IServiceScope fetchScope = _serviceServiceScopeFactory.CreateScope())
            {
                GuildService fetchGuildService = fetchScope.ServiceProvider.GetRequiredService<GuildService>();
                matches = [.. (await fetchGuildService.Get()).Where((guild) =>
                    {
                        if (isTesting && guild.Id != 769709969796628500)
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
            }

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
                    // Each concurrent task gets its own DI scope so that each gets a
                    // separate PgContext / NpgsqlConnection, avoiding the
                    // "A command is already in progress" error.
                    using IServiceScope guildScope = _serviceServiceScopeFactory.CreateScope();
                    GuildService guildService = guildScope.ServiceProvider.GetRequiredService<GuildService>();
                    VersionService versionService = guildScope.ServiceProvider.GetRequiredService<VersionService>();
                    LanguageService languageService = guildScope.ServiceProvider.GetRequiredService<LanguageService>();
                    SpecialVerseProcessingService specialVerseProcessingService = guildScope.ServiceProvider.GetRequiredService<SpecialVerseProcessingService>();

                    return await ProcessGuild(guild, resultsByVersion, dateTimeInStandardTz, removedGuilds, guildService, versionService, languageService, specialVerseProcessingService);
                }
                catch (Exception ex)
                {
                    Log.Error($"AutomaticDailyVerseService: Caught unhandled exception, received {ex.Message} for guild {guild.Id}.");
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

        public async Task<bool> ProcessGuild(Guild guild, ConcurrentDictionary<string, Task<VerseResult>> resultsByVersion, ZonedDateTime dateTimeInStandardTz, ConcurrentBag<long> removedGuilds, GuildService _guildService, VersionService _versionService, LanguageService _languageService, SpecialVerseProcessingService _specialVerseProcessingService)
        {
            InternalEmbed embed;
            InternalContainer container;
            WebhookRequestBody webhookRequestBody = null;
            bool isSuccess = false;

            string version = guild.Version ?? "RSV";
            string culture = guild.Language ?? "en-US";

            Version idealVersion = await _versionService.Get(version) ?? await _versionService.Get("RSV");
            Language idealLanguage = await _languageService.Get(culture) ?? await _languageService.Get("en-US");

            CultureInfo.CurrentUICulture = new CultureInfo(idealLanguage.Id);

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
                Task<VerseResult> verseResultTask = resultsByVersion.GetOrAdd(idealVersion.Id, _ => _specialVerseProcessingService.GetDailyVerse(idealVersion, true, true));
                VerseResult verse = await verseResultTask;

                if (verse == null)
                {
                    return false;
                }

                string rolePing = guild.DailyVerseRoleId == guild.Id ? "@everyone" : $"<@&{guild.DailyVerseRoleId}>";
                string content = guild.DailyVerseRoleId != 0 ? $"{rolePing} - {_localizer["AutomaticDailyVerseLeadIn"]}:" : $"{_localizer["AutomaticDailyVerseLeadIn"]}:";

                // Ensure verse is formatted according to guild preference for display.
                string displayStyle = guild.DisplayStyle ?? "embed";
                verse = Utils.GetInstance().FormatVerseForDisplay(verse, displayStyle);

                if (displayStyle == "embed")
                {
                    container = Utils.GetInstance().VerseToContainer(verse);

                    webhookRequestBody = new WebhookRequestBody
                    {
                        Username = _localizer["AutomaticDailyVerseWebhookUsername"],
                        AvatarURL = Utils.GetIconURL(),
                        Components = [new TextDisplayComponent(content), container],
                        Flags = 32768 // 1 << 15 (IS_COMPONENTS_V2)
                    };
                }
                else if (displayStyle == "blockquote")
                {
                    string blockquoteText = $"**{verse.Reference.AsString} - {verse.Reference.Version.Name}**\n\n> {verse.Text.Replace("\n", "\n> ")}\n\n-# {Utils.GetInstance().emoji["logo_emoji"]}  {Utils.GetInstance().GetLocalizedFooterString()}";

                    if (verse.Reference.Version.Publisher == "biblica")
                    {
                        blockquoteText += " ∙ [Biblica](<https://biblica.com>)";
                    }
                    else if (verse.Reference.Version.Publisher == "lockman")
                    {
                        blockquoteText += " ∙ [The Lockman Foundation](<https://www.lockman.org>)";
                    }

                    webhookRequestBody = new WebhookRequestBody
                    {
                        Content = $"{content}\n\n{blockquoteText}",
                        Username = _localizer["AutomaticDailyVerseWebhookUsername"],
                        AvatarURL = Utils.GetIconURL()
                    };
                }
                else if (displayStyle == "code")
                {
                    string codeBlockText = $"**{verse.Reference.AsString} - {verse.Reference.Version.Name}**\n\n```json\n{verse.Text.Replace("*", "")}\n```\n\n-# {Utils.GetInstance().emoji["logo_emoji"]}  {Utils.GetInstance().GetLocalizedFooterString()}";

                    if (verse.Reference.Version.Publisher == "biblica")
                    {
                        codeBlockText += " ∙ [Biblica](<https://biblica.com>)";
                    }
                    else if (verse.Reference.Version.Publisher == "lockman")
                    {
                        codeBlockText += " ∙ [The Lockman Foundation](<https://www.lockman.org>)";
                    }

                    webhookRequestBody = new WebhookRequestBody
                    {
                        Content = $"{content}\n\n{codeBlockText}",
                        Username = _localizer["AutomaticDailyVerseWebhookUsername"],
                        AvatarURL = Utils.GetIconURL()
                    };
                }
            }

            RestRequest request = new(guild.DailyVerseWebhook);
            request.AddQueryParameter("wait", "true"); // Discord will return a message body instead of 204 No Content
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

                if (!_previousMinuteFailedGuilds.ContainsKey(guild.Id))
                {
                    Log.Error($"AutomaticDailyVerseService: Caught exception, received {statusCode} for guild {guild.Id}. Adding to failures queue...");
                    _previousMinuteFailedGuilds.TryAdd(guild.Id, guild);
                }
                else
                {
                    Log.Error($"AutomaticDailyVerseService: Failed guild {guild.Id} has failed again, removing from queue...");
                    _previousMinuteFailedGuilds.TryRemove(guild.Id, out _);
                }

                isSuccess = false;
            }

            List<UpdateDef<Guild>> updates = [];
            if (statusCode is HttpStatusCode.NoContent or HttpStatusCode.OK)
            {
                if (_previousMinuteFailedGuilds.TryRemove(guild.Id, out _))
                {
                    Log.Information($"AutomaticDailyVerseService: Failed guild {guild.Id} has been resent successfully.");
                }

                updates.Add(UpdateDef<Guild>.Set(
                    guildToUpdate => guildToUpdate.DailyVerseLastSentDate,
                    dateTimeInStandardTz.ToString("MM/dd/yyyy", null)));

                isSuccess = true;
            }
            else if (statusCode == HttpStatusCode.NotFound)
            {
                // This webhook no longer exists, so we'll remove the daily verse preferences of the guild.
                removedGuilds.Add(guild.Id);

                updates.Add(UpdateDef<Guild>.Set(guildToUpdate => guildToUpdate.DailyVerseTime, null));
                updates.Add(UpdateDef<Guild>.Set(guildToUpdate => guildToUpdate.DailyVerseTimeZone, null));
                updates.Add(UpdateDef<Guild>.Set(guildToUpdate => guildToUpdate.DailyVerseChannelId, 0));
                updates.Add(UpdateDef<Guild>.Set(guildToUpdate => guildToUpdate.DailyVerseRoleId, 0));
                updates.Add(UpdateDef<Guild>.Set(guildToUpdate => guildToUpdate.DailyVerseWebhook, null));
                updates.Add(UpdateDef<Guild>.Set(guildToUpdate => guildToUpdate.DailyVerseLastSentDate, null));
                updates.Add(UpdateDef<Guild>.Set(guildToUpdate => guildToUpdate.DailyVerseIsThread, false));

                isSuccess = false;
            }

            updates.Add(UpdateDef<Guild>.Set(guildToUpdate => guildToUpdate.DailyVerseLastStatusCode, statusCode));
            await _guildService.Update(guild.Id, updates.Combine());

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
