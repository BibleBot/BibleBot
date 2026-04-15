/*
 * Copyright (C) 2016-2026 Kerygma Digital Co.
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BibleBot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BibleBot.Backend.Services
{
    /// <summary>
    /// Service responsible for storing verse content fetched from external providers
    /// (BibleGateway, API.Bible) into the database for future local retrieval.
    /// </summary>
    public partial class VerseStorageService(IServiceScopeFactory scopeFactory, ILogger<VerseStorageService> logger)
    {
        /// <summary>
        /// Regex to extract verse numbers from the <c>&lt;**N**&gt;</c> or <c>&lt;**C:V**&gt;</c> marker format used by providers.
        /// </summary>
        [GeneratedRegex(@"<\*\*(?:\d+:)?(\d+)\*\*>", RegexOptions.Compiled)]
        private static partial Regex VerseMarkerRegex();

        /// <summary>
        /// Splits a provider's formatted text blob into individual verses and stores them in the database.
        /// </summary>
        /// <param name="reference">The reference that was fetched.</param>
        /// <param name="text">The raw text from the provider, containing <c>&lt;**N**&gt;</c> verse markers.</param>
        /// <param name="source">The source identifier ("bg" or "ab").</param>
        public async Task StoreFromProvider(Reference reference, string text, string source)
        {
            if (reference?.Book == null || string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            List<(int number, string content)> verseEntries = SplitByVerseMarkers(text);
            if (verseEntries.Count == 0)
            {
                return;
            }

            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                PgContext context = scope.ServiceProvider.GetRequiredService<PgContext>();

                // Resolve chapters for the reference range
                int startChapter = reference.StartingChapter;
                int endChapter = reference.EndingChapter > 0 ? reference.EndingChapter : startChapter;

                List<Chapter> chapters = await context.Chapters
                    .Where(c => c.BookId == reference.Book.Id
                             && c.Number >= startChapter
                             && c.Number <= endChapter)
                    .Include(c => c.Verses)
                    .OrderBy(c => c.Number)
                    .ToListAsync();

                if (chapters.Count == 0)
                {
                    logger.LogWarning("No chapters found for book {BookId} chapters {Start}-{End}",
                        reference.Book.Id, startChapter, endChapter);
                    return;
                }

                // For single-chapter references, all verses belong to the same chapter.
                // For multi-chapter references, we use the chapter:verse markers from the text
                // to determine which chapter each verse belongs to.
                if (chapters.Count == 1)
                {
                    UpsertVerses(context, chapters[0], verseEntries, source);
                }
                else
                {
                    UpsertMultiChapterVerses(context, chapters, verseEntries, source);
                }

                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error storing verses for {Reference}", reference.ToString(true));
            }
        }

        /// <summary>
        /// Splits text by <c>&lt;**N**&gt;</c> markers into individual (number, content) tuples.
        /// </summary>
        private static List<(int number, string content)> SplitByVerseMarkers(string text)
        {
            List<(int number, string content)> results = [];
            MatchCollection matches = VerseMarkerRegex().Matches(text);

            for (int i = 0; i < matches.Count; i++)
            {
                if (!int.TryParse(matches[i].Groups[1].Value, out int verseNumber))
                {
                    continue;
                }

                int contentStart = matches[i].Index + matches[i].Length;
                int contentEnd = (i + 1 < matches.Count) ? matches[i + 1].Index : text.Length;
                string content = text[contentStart..contentEnd].Trim();

                if (!string.IsNullOrWhiteSpace(content))
                {
                    results.Add((verseNumber, content));
                }
            }

            return results;
        }

        private static void UpsertVerses(PgContext context, Chapter chapter,
            List<(int number, string content)> verses, string source)
        {
            foreach ((int number, string content) in verses)
            {
                Verse existing = chapter.Verses.FirstOrDefault(v => v.Number == number);
                if (existing != null)
                {
                    existing.Content = content;
                    existing.Source = source;
                    existing.FetchedAt = DateTime.UtcNow;
                }
                else
                {
                    context.Verses.Add(new Verse
                    {
                        ChapterId = chapter.Id,
                        Number = number,
                        Content = content,
                        Source = source,
                        FetchedAt = DateTime.UtcNow
                    });
                }
            }
        }

        private static void UpsertMultiChapterVerses(PgContext context, List<Chapter> chapters,
            List<(int number, string content)> verses, string source)
        {
            // For multi-chapter references, the text may contain chapter:verse markers like <**5:1**>.
            // We use these to determine chapter boundaries. If a verse number resets (goes lower than previous),
            // we advance to the next chapter.
            int chapterIndex = 0;
            int previousVerseNumber = 0;

            foreach ((int number, string content) in verses)
            {
                // If verse number is less than or equal to previous, we've moved to the next chapter
                if (number <= previousVerseNumber && chapterIndex < chapters.Count - 1)
                {
                    chapterIndex++;
                }

                if (chapterIndex < chapters.Count)
                {
                    Chapter chapter = chapters[chapterIndex];
                    Verse existing = chapter.Verses.FirstOrDefault(v => v.Number == number);
                    if (existing != null)
                    {
                        existing.Content = content;
                        existing.Source = source;
                        existing.FetchedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        context.Verses.Add(new Verse
                        {
                            ChapterId = chapter.Id,
                            Number = number,
                            Content = content,
                            Source = source,
                            FetchedAt = DateTime.UtcNow
                        });
                    }
                }

                previousVerseNumber = number;
            }
        }
    }
}
