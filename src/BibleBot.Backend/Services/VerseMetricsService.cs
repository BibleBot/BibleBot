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
    public class VerseMetricsService(MetricsContext ctx)
    {
        private static NpgsqlRange<int> CreateRange(int first, int last) => last < first
                ? throw new VerseRangeInvalidException($"Verse range has an illogical sequence, {last} is lesser than {first}.")
                : new NpgsqlRange<int>(first, true, last, true);

        /// <summary>
        /// Creates verse metrics for a reference. Supports multi-chapter references.
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="guildId">The guild ID</param>
        /// <param name="reference">The verse reference</param>
        /// <param name="chapterEndingVerses">Dictionary mapping chapter numbers to their ending verse in the reference</param>
        public async Task<List<VerseMetric>> Create(string userId, string guildId, Reference reference, Dictionary<int, int> chapterEndingVerses = null, bool isTest = false)
        {
            SentrySdk.ConfigureScope(scope =>
            {
                scope.Contexts["VerseMetricReference"] = reference;
            });

            List<VerseMetric> verseMetricsToAdd = [];
            chapterEndingVerses ??= [];

            string versionAbbreviation = reference.AliasingVersion != null
                ? reference.AliasingVersion.Abbreviation
                : reference.Version.Abbreviation;
            string publisher = reference.Version.Publisher;

            int startingChapter = reference.Book.ProperName == "Psalm 151" ? 151 : reference.StartingChapter;
            int endingChapter = reference.Book.ProperName == "Psalm 151" ? 151 : reference.EndingChapter;
            int chapterSpan = endingChapter - startingChapter;

            // Single chapter reference
            if (chapterSpan == 0 || endingChapter == 0)
            {
                try
                {
                    VerseMetric verseMetric = CreateBaseMetric(startingChapter);

                    // Add appended verses to first metric only
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

                    int endingVerse = reference.EndingVerse;
                    if (reference.IsExpandoVerse && chapterEndingVerses.TryGetValue(startingChapter, out int expandoEnding))
                    {
                        endingVerse = expandoEnding;
                    }

                    verseMetric.VerseRange = CreateRange(reference.StartingVerse, endingVerse);
                    verseMetricsToAdd.Add(verseMetric);
                }
                catch (VerseRangeInvalidException ex)
                {
                    SentrySdk.CaptureException(ex);
                }
            }
            // Multi-chapter reference
            else
            {
                for (int chapter = startingChapter; chapter <= endingChapter; chapter++)
                {
                    try
                    {
                        VerseMetric verseMetric = CreateBaseMetric(chapter);
                        int startVerse;
                        int endVerse;

                        if (chapter == startingChapter)
                        {
                            // First chapter: starts at reference.StartingVerse, ends at chapter's last verse
                            startVerse = reference.StartingVerse;
                            endVerse = chapterEndingVerses.TryGetValue(chapter, out int firstEnd) ? firstEnd : reference.StartingVerse;

                            // Add appended verses to first metric only
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
                        }
                        else if (chapter == endingChapter)
                        {
                            // Last chapter: starts at verse 1, ends at reference.EndingVerse
                            startVerse = 1;
                            endVerse = reference.EndingVerse;
                        }
                        else
                        {
                            // Middle chapters: verse 1 to last verse of chapter
                            startVerse = 1;
                            endVerse = chapterEndingVerses.TryGetValue(chapter, out int midEnd) ? midEnd : 1;
                        }

                        verseMetric.VerseRange = CreateRange(startVerse, endVerse);
                        verseMetricsToAdd.Add(verseMetric);
                    }
                    catch (VerseRangeInvalidException ex)
                    {
                        SentrySdk.CaptureException(ex);
                    }
                }
            }

            SentrySdk.ConfigureScope(scope =>
            {
                scope.Contexts["VerseMetricObjects"] = verseMetricsToAdd;
            });

            if (isTest) return verseMetricsToAdd;

            try
            {
                await ctx.AddRangeAsync(verseMetricsToAdd);
                await ctx.SaveChangesAsync();
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

            // Helper to create a base VerseMetric with common properties
            VerseMetric CreateBaseMetric(int chapter) => new()
            {
                UserId = userId,
                GuildId = guildId,
                Book = reference.Book.Name,
                Chapter = chapter,
                Version = versionAbbreviation,
                Publisher = publisher,
                IsOT = reference.IsOT,
                IsNT = reference.IsNT,
                IsDEU = reference.IsDEU
            };
        }
    }
}
