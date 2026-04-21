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
using System.Text;
using System.Threading.Tasks;
using BibleBot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BibleBot.Backend.Providers.Content
{
    public partial class HouseProvider(IServiceScopeFactory scopeFactory) : IContentProvider
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        public string Name { get; set; } = "usx";

        public async Task<VerseResult> GetVerse(Reference reference, bool titlesEnabled)
        {
            if (reference?.Book == null)
            {
                return null;
            }

            using IServiceScope scope = _scopeFactory.CreateScope();
            PgContext context = scope.ServiceProvider.GetRequiredService<PgContext>();

            int startChapter = reference.StartingChapter;
            int endChapter = reference.EndingChapter > 0 ? reference.EndingChapter : startChapter;

            List<Chapter> chapters = await context.Chapters
                .Where(c => c.BookId == reference.Book.Id
                          && c.Number >= startChapter
                          && c.Number <= endChapter)
                .Include(c => c.Verses.Where(v => v.Content != null))
                .OrderBy(c => c.Number)
                .ToListAsync();

            if (chapters.Count == 0 || !chapters.Any(c => c.Verses.Count > 0))
            {
                return null;
            }

            // Assemble the verse text with <**N**> markers
            StringBuilder textBuilder = new();
            string title = "";
            string psalmTitle = "";

            foreach (Chapter chapter in chapters)
            {
                // Determine the verse range for this chapter
                int startVerse = (chapter.Number == startChapter) ? reference.StartingVerse : 1;
                int endVerse;

                if (chapter.Number == endChapter && reference.EndingVerse > 0)
                {
                    endVerse = reference.EndingVerse;
                }
                else if (chapter.Number == endChapter && reference.EndingVerse == 0 && reference.IsExpandoVerse)
                {
                    endVerse = chapter.Verses.Count > 0 ? chapter.Verses.Max(v => v.Number) : startVerse;
                }
                else if (chapter.Number != startChapter && chapter.Number != endChapter)
                {
                    // Middle chapter in multi-chapter range — include all verses
                    startVerse = 1;
                    endVerse = chapter.Verses.Count > 0 ? chapter.Verses.Max(v => v.Number) : 1;
                }
                else
                {
                    endVerse = chapter.Verses.Count > 0 ? chapter.Verses.Max(v => v.Number) : startVerse;
                }

                List<Verse> versesInRange = [.. chapter.Verses
                    .Where(v => v.Number >= startVerse && v.Number <= endVerse)
                    .OrderBy(v => v.Number)];

                foreach (Verse verse in versesInRange)
                {
                    if (verse.Source == "ab" && verse.FetchedAt.HasValue
                                             && verse.FetchedAt.Value.AddDays(30) < DateTime.UtcNow)
                    {
                        // At least one verse is stale — signal the controller to re-fetch
                        return null;
                    }
                }

                if (versesInRange.Count == 0 || versesInRange.Count != (endVerse - startVerse) + 1)
                {
                    // No content for this range — can't serve from local, signal fallback
                    return null;
                }

                // Look up titles if enabled
                if (titlesEnabled && chapter.Titles?.Count > 0)
                {
                    List<ChapterTitle> matchingTitle = [.. chapter.Titles.Where(t => t.StartVerse <= endVerse && t.EndVerse >= startVerse)];

                    if (matchingTitle.Count != 0)
                    {
                        if (title.Length > 0)
                        {
                            title += " / ";
                        }

                        title += string.Join(" / ", matchingTitle);
                    }

                    // Check for psalm superscription (d style, attached to verse 1)
                    if (startVerse == 1)
                    {
                        ChapterTitle psalmSuperscription = chapter.Titles
                            .FirstOrDefault(t => t.StartVerse == 1 && t.EndVerse == 1
                                && t.Title != title);

                        if (psalmSuperscription != null)
                        {
                            psalmTitle = psalmSuperscription.Title;
                        }
                    }
                }

                foreach (Verse verse in versesInRange)
                {
                    if (chapters.Count > 1 && chapter.Number != startChapter && verse.Number == 1)
                    {
                        textBuilder.Append($"<**{chapter.Number}:{verse.Number}**>");
                    }
                    else
                    {
                        textBuilder.Append($"<**{verse.Number}**>");
                    }
                    textBuilder.Append(' ').Append(verse.Content).Append(' ');
                }
            }

            // Handle appended verses (comma-separated references like John 3:16, 18)
            if (reference.AppendedVerses?.Count > 0)
            {
                foreach (Tuple<int, int> appended in reference.AppendedVerses)
                {
                    Chapter chapter = chapters.FirstOrDefault(c => c.Number == startChapter);
                    if (chapter == null)
                    {
                        continue;
                    }

                    List<Verse> appendedVerses = [.. chapter.Verses
                        .Where(v => v.Number >= appended.Item1 && v.Number <= appended.Item2)
                        .OrderBy(v => v.Number)];

                    foreach (Verse verse in appendedVerses)
                    {
                        textBuilder.Append($"<**{verse.Number}**>");
                        textBuilder.Append(' ').Append(verse.Content).Append(' ');
                    }
                }
            }

            string text = textBuilder.ToString().Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            reference.AsString = reference.ToString(true);

            return new VerseResult
            {
                Reference = reference,
                Title = title,
                PsalmTitle = psalmTitle,
                Text = text
            };
        }

        public async Task<VerseResult> GetVerse(Reference reference, bool titlesEnabled, bool verseNumbersEnabled) => await GetVerse(reference, titlesEnabled);

        public async Task<VerseResult> GetVerse(string reference, bool titlesEnabled, BibleBot.Models.Version version) => await GetVerse(reference, titlesEnabled, true, version);
        public async Task<VerseResult> GetVerse(string reference, bool titlesEnabled, bool verseNumbersEnabled, BibleBot.Models.Version version) => await GetVerse(new Reference { Book = null, Version = version, AsString = reference }, titlesEnabled, verseNumbersEnabled);

        public async Task<List<SearchResult>> Search(string query, BibleBot.Models.Version version) => [];
    }
}
