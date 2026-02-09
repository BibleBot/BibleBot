/*
* Copyright (C) 2016-2026 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BibleBot.Models;
using Microsoft.Extensions.DependencyInjection;
using NpgsqlTypes;
using Sentry;
using Serilog;

namespace BibleBot.Backend.Services
{
    public class VerseMetricsService
    {
        private readonly MetricsContext _ctx;

        public VerseMetricsService(IServiceProvider serviceProvider)
        {
            _ctx = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<MetricsContext>();
            _ctx.Database.EnsureCreated();
        }

        private static NpgsqlRange<int> CreateRange(int first, int last) => last < first
                ? throw new VerseRangeInvalidException($"Verse range has an illogical sequence, {last} is lesser than {first}.")
                : new NpgsqlRange<int>(first, true, last, true);

        public async Task<List<VerseMetric>> Create(string userId, string guildId, Reference reference, int startingChapterEndingVerse = 0)
        {
            SentrySdk.ConfigureScope(scope =>
            {
                scope.Contexts["VerseMetricReference"] = reference;
            });

            List<VerseMetric> verseMetricsToAdd = [];

            VerseMetric verseMetric = new()
            {
                UserId = userId,
                GuildId = guildId,
                Book = reference.Book.Name,
                Version = reference.AliasingVersion != null ? reference.AliasingVersion.Abbreviation : reference.Version.Abbreviation,
                IsOT = reference.IsOT,
                IsNT = reference.IsNT,
                IsDEU = reference.IsDEU
            };

            if (reference.Version.Publisher != null)
            {
                verseMetric.Publisher = reference.Version.Publisher;
            }

            foreach (Tuple<int, int> appendedVerseTuple in reference.AppendedVerses)
            {
                try
                {
                    AppendedVerse appendedVerse = new()
                    {
                        VerseRange = CreateRange(appendedVerseTuple.Item1, appendedVerseTuple.Item2)
                    };

                    verseMetric.AppendedVerses.Add(appendedVerse);
                }
                catch (VerseRangeInvalidException ex)
                {
                    SentrySdk.CaptureException(ex);
                }
            }

            verseMetric.Chapter = reference.Book.ProperName == "Psalm 151" ? 151 : reference.StartingChapter;

            if (startingChapterEndingVerse == 0)
            {
                try
                {
                    verseMetric.VerseRange = CreateRange(reference.StartingVerse, reference.EndingVerse);
                    verseMetricsToAdd.Add(verseMetric);
                }
                catch (VerseRangeInvalidException ex)
                {
                    SentrySdk.CaptureException(ex);
                }
            }
            else if (startingChapterEndingVerse != 0 && (reference.EndingChapter - reference.StartingChapter) == 1)
            {
                try
                {
                    verseMetric.VerseRange = CreateRange(reference.StartingVerse, startingChapterEndingVerse);
                    verseMetricsToAdd.Add(verseMetric);

                    verseMetricsToAdd.Add(new()
                    {
                        UserId = userId,
                        GuildId = guildId,
                        Book = reference.Book.Name,
                        Chapter = reference.EndingChapter,
                        VerseRange = CreateRange(1, reference.EndingVerse),
                        Version = reference.Version.Abbreviation,
                        IsOT = reference.IsOT,
                        IsNT = reference.IsNT,
                        IsDEU = reference.IsDEU
                    });
                }
                catch (VerseRangeInvalidException ex)
                {
                    SentrySdk.CaptureException(ex);
                }
            }
            else if (reference.IsExpandoVerse || startingChapterEndingVerse > 0)
            {
                try
                {
                    verseMetric.VerseRange = CreateRange(reference.StartingVerse, startingChapterEndingVerse);
                    verseMetricsToAdd.Add(verseMetric);
                }
                catch (VerseRangeInvalidException ex)
                {
                    if (startingChapterEndingVerse == verseMetric.AppendedVerses[^1].VerseRange.UpperBound)
                    {
                        verseMetric.VerseRange = CreateRange(reference.StartingVerse, reference.EndingVerse);
                        verseMetricsToAdd.Add(verseMetric);
                    }
                    else
                    {
                        SentrySdk.CaptureException(ex);
                    }
                }
            }

            if (verseMetricsToAdd.Count == 0 && (reference.EndingChapter - reference.StartingChapter) > 1)
            {
                // Despite not having the functionality for it, we can still gauge the need to implement metrics
                // for these scenarios. I'm assuming it isn't very commonplace, so I'm making it a TODO.
                Log.Information("VerseMetricsService: Ignoring metrics on references with more than 2 chapters.");
                SentrySdk.CaptureException(new Exception("Failed to create metric for a reference with more than 2 chapters."));
            }

            SentrySdk.ConfigureScope(scope =>
            {
                scope.Contexts["VerseMetricObjects"] = verseMetricsToAdd;
            });

            try
            {
                await _ctx.AddRangeAsync(verseMetricsToAdd);
                await _ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (!ex.StackTrace.Contains("ReferenceConsistencyTest"))
                {
                    Log.Error(ex.Message);
                    SentrySdk.CaptureException(ex);
                }
            }

            return verseMetricsToAdd;
        }
    }
}