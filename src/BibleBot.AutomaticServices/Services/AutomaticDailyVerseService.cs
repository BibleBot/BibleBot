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
using BibleBot.Backend.Services.Providers.Content;
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

        private readonly SpecialVerseProvider _spProvider;
        private readonly List<IContentProvider> _bibleProviders;

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

        private async void RunAutomaticDailyVerses(object state)
        {
            try
            {
                int count = 0;
                bool isTesting = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
                List<string> guildsCleared = [];

                Instant currentInstant = SystemClock.Instance.GetCurrentInstant();
                ZonedDateTime dateTimeInStandardTz = currentInstant.InZone(DateTimeZoneProviders.Tzdb["America/Detroit"]);

                Log.Information($"AutomaticDailyVerseService: Fetching guilds to process for {dateTimeInStandardTz.ToString("h:mm tt x", new CultureInfo("en-US"))}...");

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
                })];

                int idealCount = matches.Count;
                Log.Information($"AutomaticDailyVerseService: Fetched {idealCount} guilds to process for {dateTimeInStandardTz.ToString("h:mm tt x", new CultureInfo("en-US"))}.");

                Stopwatch watch = Stopwatch.StartNew();
                string votdRef = await _spProvider.GetDailyVerse();
                Dictionary<string, VerseResult> resultsByVersion = [];

                foreach (Guild guild in matches.Where(guild => !guildsCleared.Contains(guild.GuildId)))
                {
                    InternalEmbed embed;
                    WebhookRequestBody webhookRequestBody;

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
                        IContentProvider provider = _bibleProviders.FirstOrDefault(pv => pv.Name == idealVersion.Source);

                        if (provider == null)
                        {
                            continue;
                        }

                        // TODO(srp): Cache results globally instead of per-minute to save even more trouble.
                        // Will require keeping track of the current daily verse, however.
                        if (!resultsByVersion.ContainsKey(idealVersion.Abbreviation))
                        {
                            resultsByVersion[idealVersion.Abbreviation] = await provider.GetVerse(votdRef, true, true, idealVersion);
                        }

                        VerseResult verse = resultsByVersion[idealVersion.Abbreviation];
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

                    RestResponse resp = await _restClient.PostAsync(request);

                    UpdateDefinitionBuilder<Guild> update = Builders<Guild>.Update;
                    List<UpdateDefinition<Guild>> updates = [];
                    if (resp.StatusCode == System.Net.HttpStatusCode.NoContent)
                    {
                        count += 1;
                        updates.Add(update.Set(guildToUpdate => guildToUpdate.DailyVerseLastSentDate, dateTimeInStandardTz.ToString("MM/dd/yyyy", null)));
                    }

                    updates.Add(update.Set(guildToUpdate => guildToUpdate.DailyVerseLastStatusCode, resp.StatusCode));

                    guildsCleared.Add(guild.Id);
                    await _guildService.Update(guild.GuildId, update.Combine(updates));
                }

                watch.Stop();
                string timeToProcess = $"{(watch.Elapsed.Hours != 0 ? $"{watch.Elapsed.Hours} hours, " : "")}{(watch.Elapsed.Minutes != 0 ? $"{watch.Elapsed.Minutes} minutes, " : "")}{watch.Elapsed.Seconds} seconds";
                Log.Information($"AutomaticDailyVerseService: Sent {(idealCount > 0 ? $"{count} of {idealCount}" : "0")} daily verse(s) for {dateTimeInStandardTz.ToString("h:mm tt x", new CultureInfo("en-US"))} in {timeToProcess}.");
            }
            catch (Exception e)
            {
                Log.Error($"AutomaticDailyVerseService: Exception caught - {e}");
            }
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
