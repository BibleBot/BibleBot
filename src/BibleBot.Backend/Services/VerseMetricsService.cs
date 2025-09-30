/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
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

        public async Task<List<VerseMetric>> Create(string userId, string guildId, Reference reference, int startingChapterEndingVerse = 0)
        {
            List<VerseMetric> verseMetricsToAdd = [];

            VerseMetric verseMetric = new()
            {
                UserId = userId,
                GuildId = guildId,
                Book = reference.Book.Name,
                Version = reference.Version.Abbreviation,
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
                AppendedVerse appendedVerse = new()
                {
                    VerseRange = new NpgsqlRange<int>(appendedVerseTuple.Item1, true, appendedVerseTuple.Item2, true)
                };

                verseMetric.AppendedVerses.Add(appendedVerse);
            }

            verseMetric.Chapter = reference.StartingChapter;

            if (startingChapterEndingVerse == 0)
            {
                verseMetric.VerseRange = new NpgsqlRange<int>(reference.StartingVerse, true, reference.EndingVerse, true);
                verseMetricsToAdd.Add(verseMetric);
            }
            else if (reference.IsExpandoVerse)
            {
                verseMetric.VerseRange = new NpgsqlRange<int>(reference.StartingVerse, true, startingChapterEndingVerse, true);
                verseMetricsToAdd.Add(verseMetric);
            }
            else if (startingChapterEndingVerse != 0 && (reference.EndingChapter - reference.StartingChapter) == 1)
            {
                verseMetric.VerseRange = new NpgsqlRange<int>(reference.StartingVerse, true, startingChapterEndingVerse, true);
                verseMetricsToAdd.Add(verseMetric);

                verseMetricsToAdd.Add(new()
                {
                    UserId = userId,
                    GuildId = guildId,
                    Book = reference.Book.Name,
                    Chapter = reference.EndingChapter,
                    VerseRange = new NpgsqlRange<int>(1, true, reference.EndingVerse, true),
                    Version = reference.Version.Abbreviation,
                    IsOT = reference.IsOT,
                    IsNT = reference.IsNT,
                    IsDEU = reference.IsDEU
                });
            }

            if (verseMetricsToAdd.Count == 0 && (reference.EndingChapter - reference.StartingChapter) > 1)
            {
                // Despite not having the functionality for it, we can still gauge the need to implement metrics
                // for these scenarios. I'm assuming it isn't very commonplace, so I'm making it a TODO.
                Log.Information("VerseMetricsService: Ignoring metrics on references with more than 2 chapters.");
                SentrySdk.CaptureException(new Exception("Failed to create metric for a reference with more than 2 chapters."));
            }

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