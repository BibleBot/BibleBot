/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Threading.Tasks;
using BibleBot.Backend;
using BibleBot.Models;
using BibleBot.Tests.Backend.Mocks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace BibleBot.Tests.Backend.Controllers.Verses
{
    [TestFixture, Category("Verses")]
    public class DataNameOverlapVersesTest : TestBaseClass
    {
        [Test]
        public void ShouldProcessVerseWithDataNameSGTHR()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Song of the Three Young Men 1:1")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Song of the Three Young Men 1:1 RSV",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
                    {
                        Title = "The Prayer of Azariah in the Furnace",
                        PsalmTitle = "",
                        Text = "<**1**> And they walked about in the midst of the flames, singing hymns to God and blessing the Lord.",
                        Reference = new Reference
                        {
                            Book = "Song of the Three Young Men",
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 1,
                            Version = _defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = false,
                            IsDEU = true,
                            AsString = "Song of the Three Young Men 1:1"
                        }
                    }
                ],
                Culture = "en-US",
                CultureFooter = $"BibleBot {Utils.Version} by Kerygma Digital"
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task ShouldProcessVerseWithActualDataNameSGTHREE()
        {
            Version testVersion = await _versionService.Get("WYC") ?? await _versionService.Create(new MockWYC());

            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Song of the Three Young Men 1:1 WYC")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Song of the Three Young Men 1:1 WYC",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**1**> And they walked in the midst of the flame, and praised God, and blessed the Lord. [And they walked in the middle of the flame, praising God, and blessing the Lord.]",
                        Reference = new Reference
                        {
                            Book = "Song of the Three Young Men",
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 1,
                            Version = testVersion,
                            IsOT = false,
                            IsNT = false,
                            IsDEU = true,
                            AsString = "Song of the Three Young Men 1:1"
                        }
                    }
                ],
                Culture = "en-US",
                CultureFooter = $"BibleBot {Utils.Version} by Kerygma Digital"
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task ShouldProcessVerseWithDataNamePRAZ()
        {
            Version testVersion = await _versionService.Get("NRSVA") ?? await _versionService.Create(new MockNRSVA());

            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Prayer of Azariah 1:1 NRSVA")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Prayer of Azariah 1:1 NRSVA",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
                    {
                        Title = "The Prayer of Azariah in the Furnace",
                        PsalmTitle = "(Additions to Daniel, inserted between 3.23 and 3.24)",
                        Text = "<**1**> They walked around in the midst of the flames, singing hymns to God and blessing the Lord.",
                        Reference = new Reference
                        {
                            Book = "Prayer of Azariah",
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 1,
                            Version = testVersion,
                            IsOT = false,
                            IsNT = false,
                            IsDEU = true,
                            AsString = "Prayer of Azariah 1:1"
                        }
                    }
                ],
                Culture = "en-US",
                CultureFooter = $"BibleBot {Utils.Version} by Kerygma Digital"
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task ShouldProcessVerseWithActualDataNamePRAZAR()
        {
            Version testVersion = await _versionService.Get("CEB") ?? await _versionService.Create(new MockCEB());

            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Prayer of Azariah 1:1 CEB")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Prayer of Azariah 1:1 CEB",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
                    {
                        Title = "Azariah's prayer for reconciliation",
                        PsalmTitle = "",
                        Text = "<**1**> Shadrach, Meshach, and Abednego walked around in the flames, singing hymns to God, blessing the Lord.",
                        Reference = new Reference
                        {
                            Book = "Prayer of Azariah",
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 1,
                            Version = testVersion,
                            IsOT = false,
                            IsNT = false,
                            IsDEU = true,
                            AsString = "Prayer of Azariah 1:1"
                        }
                    }
                ],
                Culture = "en-US",
                CultureFooter = $"BibleBot {Utils.Version} by Kerygma Digital"
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task ShouldProcessVerseWithDataNameADDESTH()
        {
            Version testVersion = await _versionService.Get("WYC") ?? await _versionService.Create(new MockWYC());

            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Additions to Esther 10:4 WYC")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Additions to Esther 10:4 WYC",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**4**> And Mordecai said, These things be done of (or by) God. [And Mordecai said, Of God these things be done.]",
                        Reference = new Reference
                        {
                            Book = "Additions to Esther",
                            StartingChapter = 10,
                            StartingVerse = 4,
                            EndingChapter = 10,
                            EndingVerse = 4,
                            Version = testVersion,
                            IsOT = false,
                            IsNT = false,
                            IsDEU = true,
                            AsString = "Additions to Esther 10:4"
                        }
                    }
                ],
                Culture = "en-US",
                CultureFooter = $"BibleBot {Utils.Version} by Kerygma Digital"
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }
    }
}