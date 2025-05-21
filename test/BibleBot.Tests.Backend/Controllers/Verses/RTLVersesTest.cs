// Copyright (C) 2016-2025 Kerygma Digital Co.
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this file,
// You can obtain one at https://mozilla.org/MPL/2.0/.

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
    public class RTLVersesTest : TestBaseClass
    {
        [Test]
        public async Task ShouldProcessGenesisInHebrew()
        {
            Version testVersion = await _versionService.Get("WLC") ?? await _versionService.Create(new MockWLC());

            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Genesis 1:1-2 WLC")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result!.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Genesis 1:1-2 WLC",
                DisplayStyle = "embed",
                Verses =
                [
                    new VerseResult
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**1**> בְּרֵאשִׁ֖ית בָּרָ֣א אֱלֹהִ֑ים אֵ֥ת הַשָּׁמַ֖יִם וְאֵ֥ת הָאָֽרֶץ׃ <**2**> וְהָאָ֗רֶץ הָיְתָ֥ה תֹ֙הוּ֙ וָבֹ֔הוּ וְחֹ֖שֶׁךְ עַל־פְּנֵ֣י תְה֑וֹם וְר֣וּחַ אֱלֹהִ֔ים מְרַחֶ֖פֶת עַל־פְּנֵ֥י הַמָּֽיִם׃",
                        Reference = new Reference
                        {
                            Book = new Book {
                                ProperName = "Genesis"
                            },
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 2,
                            Version = testVersion,
                            IsOT = false,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "Genesis 1:1-2"
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
        public async Task ShouldProcessJohnInArabic()
        {
            Version testVersion = await _versionService.Get("ERV-AR") ?? await _versionService.Create(new MockERVAR());

            ObjectResult result = _versesController.ProcessMessage(new MockRequest("John 1:1-2 ERV-AR")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result!.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "John 1:1-2 ERV-AR",
                DisplayStyle = "embed",
                Verses =
                [
                    new VerseResult
                    {
                        Title = "يَسُوعُ المَسِيحُ كَلِمَةُ الله",
                        PsalmTitle = "",
                        Text = "<**1**> فِي البَدْءِ كَانَ الكَلِمَةُ مَوْجُودًا، وَكَانَ الكَلِمَةُ مَعَ اللهِ، وَكَانَ الكَلِمَةُ هُوَ اللهَ. <**2**> كَانَ الكَلِمَةُ مَعَ اللهِ فِي البَدْءِ.",
                        Reference = new Reference
                        {
                            Book = new Book {
                                ProperName = "John"
                            },
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 2,
                            Version = testVersion,
                            IsOT = false,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "John 1:1-2"
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
