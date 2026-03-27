/*
* Copyright (C) 2016-2026 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.Threading.Tasks;
using BibleBot.Models;
using NUnit.Framework;
using FluentAssertions;
using Version = BibleBot.Models.Version;

namespace BibleBot.Tests.Backend.Services
{
    [TestFixture]
    public class VerseMetricsServiceTests : TestBaseClass
    {
        private Version _version;
        private Book _psalms;

        [OneTimeSetUp]
        public void Setup()
        {
            _version = new Version
            {
                Id = "KJV",
                Source = "bg"
            };

            _psalms = new Book
            {
                Name = "PSA",
                ProperName = "Psalms"
            };
        }

        [Test]
        public async Task ShouldHandleOneChapter()
        {
            // Psalm 1:1-3
            Reference reference = new()
            {
                Book = _psalms,
                StartingChapter = 1,
                StartingVerse = 1,
                EndingChapter = 1,
                EndingVerse = 3,
                Version = _version,
                AliasingVersion = _version,
                IsOT = true
            };

            List<VerseMetric> metrics = await _verseMetricsService.Create(111111, 111111, reference, isTest: true);

            metrics.Should().HaveCount(1);
            metrics[0].Chapter.Should().Be(1);
            metrics[0].VerseRange.LowerBound.Should().Be(1);
            metrics[0].VerseRange.UpperBound.Should().Be(3);
        }

        [Test]
        public async Task ShouldHandleTwoChapters()
        {
            // Psalm 1:1-2:12
            Reference reference = new()
            {
                Book = _psalms,
                StartingChapter = 1,
                StartingVerse = 1,
                EndingChapter = 2,
                EndingVerse = 12,
                Version = _version,
                AliasingVersion = _version,
                IsOT = true
            };

            // We need to provide ending verses for chapters that aren't the last one
            Dictionary<int, int> chapterEndingVerses = new() { { 1, 6 } }; // Psalm 1 has 6 verses

            List<VerseMetric> metrics = await _verseMetricsService.Create(111111, 111111, reference, chapterEndingVerses, true);

            metrics.Should().HaveCount(2);

            // Chapter 1: 1-6
            metrics[0].Chapter.Should().Be(1);
            metrics[0].VerseRange.LowerBound.Should().Be(1);
            metrics[0].VerseRange.UpperBound.Should().Be(6);

            // Chapter 2: 1-12
            metrics[1].Chapter.Should().Be(2);
            metrics[1].VerseRange.LowerBound.Should().Be(1);
            metrics[1].VerseRange.UpperBound.Should().Be(12);
        }

        [Test]
        public async Task ShouldHandleThreeChapters()
        {
            // Psalm 1:1-3:8
            Reference reference = new()
            {
                Book = _psalms,
                StartingChapter = 1,
                StartingVerse = 1,
                EndingChapter = 3,
                EndingVerse = 8,
                Version = _version,
                AliasingVersion = _version,
                IsOT = true
            };
            Dictionary<int, int> chapterEndingVerses = new() { { 1, 6 }, { 2, 12 } };

            List<VerseMetric> metrics = await _verseMetricsService.Create(111111, 111111, reference, chapterEndingVerses, true);

            metrics.Should().HaveCount(3);

            // Chapter 1: 1-6
            metrics[0].Chapter.Should().Be(1);
            metrics[0].VerseRange.LowerBound.Should().Be(1);
            metrics[0].VerseRange.UpperBound.Should().Be(6);

            // Chapter 2: 1-12
            metrics[1].Chapter.Should().Be(2);
            metrics[1].VerseRange.LowerBound.Should().Be(1);
            metrics[1].VerseRange.UpperBound.Should().Be(12);

            // Chapter 3: 1-8
            metrics[2].Chapter.Should().Be(3);
            metrics[2].VerseRange.LowerBound.Should().Be(1);
            metrics[2].VerseRange.UpperBound.Should().Be(8);
        }

        [Test]
        public async Task ShouldHandleExpandoVerse()
        {
            // Psalm 1:1- (expando)
            Reference reference = new()
            {
                Book = _psalms,
                StartingChapter = 1,
                StartingVerse = 1,
                EndingChapter = 1,
                EndingVerse = 0, // In expando, ending verse is often 0 or same as starting in parsing, but IsExpandoVerse is true
                IsExpandoVerse = true,
                Version = _version,
                AliasingVersion = _version,
                IsOT = true
            };
            Dictionary<int, int> chapterEndingVerses = new() { { 1, 6 } };

            List<VerseMetric> metrics = await _verseMetricsService.Create(111111, 111111, reference, chapterEndingVerses, true);

            metrics.Should().HaveCount(1);
            metrics[0].Chapter.Should().Be(1);
            metrics[0].VerseRange.LowerBound.Should().Be(1);
            metrics[0].VerseRange.UpperBound.Should().Be(6);
        }
    }
}
