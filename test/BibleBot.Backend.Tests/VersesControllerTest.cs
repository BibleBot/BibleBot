/*
* Copyright (C) 2016-2024 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Threading.Tasks;
using BibleBot.Backend.Controllers;
using BibleBot.Backend.Services;
using BibleBot.Backend.Services.Providers;
using BibleBot.Backend.Tests.Mocks;
using BibleBot.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace BibleBot.Backend.Tests
{
    [TestFixture, Category("VersesController")]
    public class VersesControllerTest
    {
        private VersesController _versesController;

        private VersionService _versionService;

        private Mock<UserService> _userServiceMock;
        private Mock<GuildService> _guildServiceMock;
        private Mock<ParsingService> _parsingServiceMock;
        private Mock<NameFetchingService> _nameFetchingServiceMock;

        private Mock<BibleGatewayProvider> _bgProviderMock;
        private Mock<APIBibleProvider> _abProviderMock;

        private IDatabaseSettings _databaseSettings;

        private Version _defaultBibleGatewayVersion;
        private Version _defaultAPIBibleVersion;

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            _databaseSettings = new DatabaseSettings
            {
                UserCollectionName = "Users",
                GuildCollectionName = "Guilds",
                VersionCollectionName = "Versions",
                LanguageCollectionName = "Languages",
                FrontendStatsCollectionName = "FrontendStats",
                DatabaseName = "BibleBotBackend"
            };

            _userServiceMock = new Mock<UserService>(_databaseSettings);
            _guildServiceMock = new Mock<GuildService>(_databaseSettings);
            _versionService = new VersionService(_databaseSettings);
            _parsingServiceMock = new Mock<ParsingService>();
            _nameFetchingServiceMock = new Mock<NameFetchingService>();

            _bgProviderMock = new Mock<BibleGatewayProvider>();
            _abProviderMock = new Mock<APIBibleProvider>();

            _defaultBibleGatewayVersion = await _versionService.Get("RSV") ?? await _versionService.Create(new MockRSV());
            _defaultAPIBibleVersion = await _versionService.Get("KJVA") ?? await _versionService.Create(new MockKJVA());

            _versesController = new VersesController(_userServiceMock.Object, _guildServiceMock.Object,
                                                    _parsingServiceMock.Object, _versionService,
                                                    _nameFetchingServiceMock.Object, _bgProviderMock.Object,
                                                    _abProviderMock.Object);
        }

        [Test]
        public void ShouldFailWhenTokenIsInvalid()
        {
            MockRequest req = new()
            {
                Token = "meowmix"
            };

            ObjectResult result = _versesController.ProcessMessage(req).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = false,
                LogStatement = null
            };

            result.StatusCode.Should().Be(403);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldFailWhenBodyIsEmpty()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest()).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = false,
                LogStatement = null
            };

            result.StatusCode.Should().Be(400);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessBibleGatewayReference()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Matthew 1:1")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Matthew 1:1 RSV",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
                    {
                        Title = "The Genealogy of Jesus the Messiah",
                        PsalmTitle = "",
                        Text = "<**1**> The book of the genealogy of Jesus Christ, the son of David, the son of Abraham.",
                        Reference = new Reference
                        {
                            Book = "Matthew",
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 1,
                            Version = _defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "Matthew 1:1"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessAPIBibleReference()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Genesis 1:1 KJVA")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Genesis 1:1 KJVA",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**1**> In the beginning God created the heaven and the earth.",
                        Reference = new Reference
                        {
                            Book = "Genesis",
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 1,
                            Version = _defaultAPIBibleVersion,
                            IsOT = true,
                            IsNT = false,
                            IsDEU = false,
                            AsString = "Genesis 1:1"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldFailWhenReferencingDeuterocanonInProtestantBible()
        {
            _ = _versionService.Get("NTFE") ?? _versionService.Create(new MockNTFE());
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Sirach 1:1 NTFE")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = false,
                LogStatement = "New Testament for Everyone (NTFE) does not support the Apocrypha/Deuterocanon."
            };

            result.StatusCode.Should().Be(400);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldFailWhenReferencingOldTestamentInNewTestamentOnlyBible()
        {
            _ = _versionService.Get("NTFE") ?? _versionService.Create(new MockNTFE());
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Genesis 1:1 NTFE")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = false,
                LogStatement = "New Testament for Everyone (NTFE) does not support the Old Testament."
            };

            result.StatusCode.Should().Be(400);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldIgnoreMultipleVerseReferencesInIgnoringBrackets()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("lorem < Genesis 1:1 NTFE / Matthew 1:1 NTFE / Acts 1:1 NTFE > ipsum John 1:1 dolor < Genesis 1:1 NTFE > sit")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "John 1:1 RSV",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
                    {
                        Title = "The Word Became Flesh",
                        PsalmTitle = "",
                        Text = "<**1**> In the beginning was the Word, and the Word was with God, and the Word was God.",
                        Reference = new Reference
                        {
                            Book = "John",
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 1,
                            Version = _defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "John 1:1"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessVerseInNonIgnoringBrackets()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("(John 1:1)")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "John 1:1 RSV",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
                    {
                        Title = "The Word Became Flesh",
                        PsalmTitle = "",
                        Text = "<**1**> In the beginning was the Word, and the Word was with God, and the Word was God.",
                        Reference = new Reference
                        {
                            Book = "John",
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 1,
                            Version = _defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "John 1:1"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessBibleGatewaySpannedReference()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Matthew 1:1-2")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Matthew 1:1-2 RSV",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
                    {
                        Title = "The Genealogy of Jesus the Messiah",
                        PsalmTitle = "",
                        Text = "<**1**> The book of the genealogy of Jesus Christ, the son of David, the son of Abraham. <**2**> Abraham was the father of Isaac, and Isaac the father of Jacob, and Jacob the father of Judah and his brothers,",
                        Reference = new Reference
                        {
                            Book = "Matthew",
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 2,
                            Version = _defaultBibleGatewayVersion,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "Matthew 1:1-2"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessAPIBibleSpannedReference()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Genesis 1:1-2 KJVA")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Genesis 1:1-2 KJVA",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**1**> In the beginning God created the heaven and the earth. <**2**> And the earth was without form, and void; and darkness was upon the face of the deep. And the Spirit of God moved upon the face of the waters.",
                        Reference = new Reference
                        {
                            Book = "Genesis",
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 2,
                            Version = _defaultAPIBibleVersion,
                            IsOT = true,
                            IsNT = false,
                            IsDEU = false,
                            AsString = "Genesis 1:1-2"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessBibleGatewaySpannedChapterReference()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Matthew 1:25-2:1")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Matthew 1:25-2:1 RSV",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
                    {
                        Title = "The Visit of the Wise Men",
                        PsalmTitle = "",
                        Text = "<**25**> but knew her not until she had borne a son; and he called his name Jesus. <**1**> Now when Jesus was born in Bethlehem of Judea in the days of Herod the king, behold, wise men from the East came to Jerusalem, saying,",
                        Reference = new Reference
                        {
                            Book = "Matthew",
                            StartingChapter = 1,
                            StartingVerse = 25,
                            EndingChapter = 2,
                            EndingVerse = 1,
                            Version = _defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "Matthew 1:25-2:1"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessAPIBibleSpannedChapterReference()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Genesis 1:31-2:1 KJVA")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Genesis 1:31-2:1 KJVA",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**31**> And God saw every thing that he had made, and, behold, it was very good. And the evening and the morning were the sixth day. <**1**> Thus the heavens and the earth were finished, and all the host of them.",
                        Reference = new Reference
                        {
                            Book = "Genesis",
                            StartingChapter = 1,
                            StartingVerse = 31,
                            EndingChapter = 2,
                            EndingVerse = 1,
                            Version = _defaultAPIBibleVersion,
                            IsOT = true,
                            IsNT = false,
                            IsDEU = false,
                            AsString = "Genesis 1:31-2:1"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessBibleGatewayExpandedReference()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Matthew 1:24-")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Matthew 1:24- RSV",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**24**> When Joseph woke from sleep, he did as the angel of the Lord commanded him; he took his wife, <**25**> but knew her not until she had borne a son; and he called his name Jesus.",
                        Reference = new Reference
                        {
                            Book = "Matthew",
                            StartingChapter = 1,
                            StartingVerse = 24,
                            EndingChapter = 1,
                            EndingVerse = 0,
                            Version = _defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "Matthew 1:24-25"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldIgnoreAPIBibleExpandedReference()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Matthew 1:24- KJVA")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = false,
                LogStatement = null
            };

            result.StatusCode.Should().Be(400);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldNotProcessReferenceStartingWithVerseZero()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Genesis 1:0")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = false,
                LogStatement = null
            };

            result.StatusCode.Should().Be(400);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldNotProcessReferenceWithSpaceBetweenColonAndVerseNumbers()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Genesis 1: 1-5")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = false,
                LogStatement = null
            };

            result.StatusCode.Should().Be(400);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessReferenceWithFullWidthColon()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Matthew 1：1-2")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Matthew 1:1-2 RSV",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
                    {
                        Title = "The Genealogy of Jesus the Messiah",
                        PsalmTitle = "",
                        Text = "<**1**> The book of the genealogy of Jesus Christ, the son of David, the son of Abraham. <**2**> Abraham was the father of Isaac, and Isaac the father of Jacob, and Jacob the father of Judah and his brothers,",
                        Reference = new Reference
                        {
                            Book = "Matthew",
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 2,
                            Version = _defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "Matthew 1:1-2"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldNotProcessSameVerseSpannedReferenceAsExpando()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Matthew 1:1-1")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Matthew 1:1 RSV",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
                    {
                        Title = "The Genealogy of Jesus the Messiah",
                        PsalmTitle = "",
                        Text = "<**1**> The book of the genealogy of Jesus Christ, the son of David, the son of Abraham.",
                        Reference = new Reference
                        {
                            Book = "Matthew",
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 1,
                            Version = _defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "Matthew 1:1"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task ShouldProcessDanielInEnglishSeptuagint()
        {
            Version testVersion = await _versionService.Get("ELXX") ?? await _versionService.Create(new MockELXX());

            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Daniel 1:1-2 ELXX")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Daniel 1:1-2 ELXX",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**1**> In the third year of the reign of Joakim king of Judah, came Nebuchadnezzar king of Babylon to Jerusalem, and besieged it. <**2**> And the Lord gave into his hand Joakim king of Judah, and part of the vessels of the house of God: and he brought them into the land of Shinar to the house of his god; and he brought the vessels into the treasure house of his god.",
                        Reference = new Reference
                        {
                            Book = "Daniel",
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 2,
                            Version = testVersion,
                            IsOT = true,
                            IsNT = false,
                            IsDEU = false,
                            AsString = "Daniel 1:1-2"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task ShouldProcessDanielInSeptuagint()
        {
            Version testVersion = await _versionService.Get("LXX") ?? await _versionService.Create(new MockLXX());

            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Daniel 1:1-2 LXX")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Daniel 1:1-2 LXX",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**1**> ἘΝ ἔτει τρίτῳ τῆς βασιλείας Ἰωακεὶμ βασιλέως Ἰούδα, ἦλθε Ναβουχοδονόσορ ὁ βασιλεὺς Βαβυλῶνος εἰς Ἱερουσαλὴμ, καὶ ἐπολιόρκει αὐτήν. <**2**> Καὶ ἔδωκε Κύριος ἐν χειρὶ αὐτοῦ τὸν Ἰωακεὶμ βασιλέα Ἰούδα, καὶ ἀπὸ μέρους τῶν σκευῶν οἴκου τοῦ Θεοῦ· καὶ ἤνεγκεν αὐτὰ εἰς γῆν Σενναὰρ οἴκου τοῦ θεοῦ αὐτοῦ, καὶ τὰ σκεύη εἰσήνεγκεν εἰς τὸν οἶκον θησαυροῦ τοῦ θεοῦ αὐτοῦ.",
                        Reference = new Reference
                        {
                            Book = "Daniel",
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 2,
                            Version = testVersion,
                            IsOT = true,
                            IsNT = false,
                            IsDEU = false,
                            AsString = "Daniel 1:1-2"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        // [Test]
        // public async Task ShouldProcessJohnInPatriarchalText()
        // {
        //     var testVersion = await versionService.Get("PAT1904");
        //     if (testVersion == null)
        //     {
        //         testVersion = await versionService.Create(new MockPAT1904());
        //     }
        //
        //     var resp = versesController.ProcessMessage(new MockRequest("John 1:1-2 PAT1904")).GetAwaiter().GetResult().Value as VerseResponse;
        //
        //     VerseResponse expected = new()
        //     {
        //         OK = true,
        //         LogStatement = "John 1:1-2 PAT1904",
        //         DisplayStyle = "embed",
        //         Verses = new List<Verse>
        //         {
        //             new()
        //             {
        //                 Title = "",
        //                 PsalmTitle = "",
        //                 Text = "<**1**> Ἐν ἀρχῇ ἦν ὁ Λόγος, καὶ ὁ Λόγος ἦν πρὸς τὸν Θεόν, καὶ Θεὸς ἦν ὁ Λόγος. <**2**> Οὗτος ἦν ἐν ἀρχῇ πρὸς τὸν Θεόν.",
        //                 Reference = new Reference
        //                 {
        //                     Book = "John",
        //                     StartingChapter = 1,
        //                     StartingVerse = 1,
        //                     EndingChapter = 1,
        //                     EndingVerse = 2,
        //                     Version = testVersion,
        //                     IsOT = false,
        //                     IsNT = true,
        //                     IsDEU = false,
        //                     AsString = "John 1:1-2"
        //                 }
        //             }
        //         }
        //     };
        //
        //     result.StatusCode.Should().Be(200);
        //     resp.Should().BeEquivalentTo(expected);
        // }

        [Test]
        public async Task ShouldNotMishandleMultipleSpansInProverbsInNIV()
        {
            Version testVersion = await _versionService.Get("NIV") ?? await _versionService.Create(new MockNIV());

            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Proverbs 25:1-12 NIV")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Proverbs 25:1-12 NIV",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
                    {
                        Title = "More Proverbs of Solomon",
                        PsalmTitle = "",
                        Text = "<**1**> These are more proverbs of Solomon, compiled by the men of Hezekiah king of Judah: <**2**> It is the glory of God to conceal a matter; to search out a matter is the glory of kings. <**3**> As the heavens are high and the earth is deep, so the hearts of kings are unsearchable. <**4**> Remove the dross from the silver, and a silversmith can produce a vessel; <**5**> remove wicked officials from the king's presence, and his throne will be established through righteousness. <**6**> Do not exalt yourself in the king's presence, and do not claim a place among his great men; <**7**> it is better for him to say to you, \"Come up here,\" than for him to humiliate you before his nobles. What you have seen with your eyes <**8**> do not bring hastily to court, for what will you do in the end if your neighbor puts you to shame? <**9**> If you take your neighbor to court, do not betray another's confidence, <**10**> or the one who hears it may shame you and the charge against you will stand. <**11**> Like apples of gold in settings of silver is a ruling rightly given. <**12**> Like an earring of gold or an ornament of fine gold is the rebuke of a wise judge to a listening ear.",
                        Reference = new Reference
                        {
                            Book = "Proverbs",
                            StartingChapter = 25,
                            StartingVerse = 1,
                            EndingChapter = 25,
                            EndingVerse = 12,
                            Version = testVersion,
                            IsOT = true,
                            IsNT = false,
                            IsDEU = false,
                            AsString = "Proverbs 25:1-12"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task ShouldWorkAroundHebrewVerseNumbersInExodusInISVPartOne()
        {
            Version testVersion = await _versionService.Get("ISV") ?? await _versionService.Create(new MockISV());

            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Exodus 20:1-7 ISV")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Exodus 20:1-7 ISV",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
                    {
                        Title = "The Ten Commandments",
                        PsalmTitle = "",
                        Text = "<**1**> Then God spoke all these words: <**2**> \"I am the Lord your God, who brought you out of the land of Egypt— from the house of slavery. <**3**> You are to have no other gods as a substitute for me. <**4**> \"You are not to craft for yourselves an idol or anything resembling what is in the skies above, or on earth beneath, or in the water sources under the earth. <**5**> You are not to bow down to them in worship or serve them, because I, the Lord your God, am a jealous God, visiting the guilt of parents on children, to the third and fourth generation of those who hate me, <**6**> but showing gracious love to the thousands of those who love me and keep my commandments. <**7**> \"You are not to misuse the name of the Lord your God, because the Lord will not leave unpunished the one who misuses his name.",
                        Reference = new Reference
                        {
                            Book = "Exodus",
                            StartingChapter = 20,
                            StartingVerse = 1,
                            EndingChapter = 20,
                            EndingVerse = 7,
                            Version = testVersion,
                            IsOT = true,
                            IsNT = false,
                            IsDEU = false,
                            AsString = "Exodus 20:1-7"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task ShouldWorkAroundHebrewVerseNumbersInExodusInISVPartTwo()
        {
            Version testVersion = await _versionService.Get("ISV") ?? await _versionService.Create(new MockISV());

            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Exodus 20:8-17 ISV")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Exodus 20:8-17 ISV",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**8**> \"Remember the Sabbath day, maintaining its holiness. <**9**> Six days you are to labor and do all your work, <**10**> but the seventh day is a Sabbath to the Lord your God. You are not to do any work—neither you, nor your son, nor your daughter, nor your male or female servant, nor your livestock, nor any foreigner who lives among you— <**11**> because the Lord made the heavens, the earth, the sea, and everything that is in them in six days. Then he rested on the seventh day. Therefore, the Lord blessed the Sabbath day and made it holy. <**12**> \"Honor your father and your mother, so that you may live long in the land that the Lord your God is giving you. <**13**> \"You are not to commit murder. <**14**> \"You are not to commit adultery. <**15**> \"You are not to steal. <**16**> \"You are not to give false testimony against your neighbor. <**17**> \"You are not to desire your neighbor's house, nor your neighbor's wife, his male or female servant, his ox, his donkey, nor anything else that pertains to your neighbor.\"",
                        Reference = new Reference
                        {
                            Book = "Exodus",
                            StartingChapter = 20,
                            StartingVerse = 8,
                            EndingChapter = 20,
                            EndingVerse = 17,
                            Version = testVersion,
                            IsOT = true,
                            IsNT = false,
                            IsDEU = false,
                            AsString = "Exodus 20:8-17"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldNotProcessEsdrasBooksAsEzra()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("1 Esdras 1:1")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "1 Esdras 1:1 RSV",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
                    {
                        Title = "Josiah Celebrates the Passover",
                        PsalmTitle = "",
                        Text = "<**1**> Josi'ah kept the passover to his Lord in Jerusalem; he killed the passover lamb on the fourteenth day of the first month,",
                        Reference = new Reference
                        {
                            Book = "1 Esdras",
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 1,
                            Version = _defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = false,
                            IsDEU = true,
                            AsString = "1 Esdras 1:1"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldNotProcessLetterOfJeremiahAsJeremiah()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Letter of Jeremiah 1:1")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Letter of Jeremiah 1:1 RSV",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**1**> A copy of a letter which Jeremiah sent to those who were to be taken to Babylon as captives by the king of the Babylonians, to give them the message which God had commanded him.",
                        Reference = new Reference
                        {
                            Book = "Letter of Jeremiah",
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 1,
                            Version = _defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = false,
                            IsDEU = true,
                            AsString = "Letter of Jeremiah 1"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessJeremiah()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Jeremiah 1:1")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Jeremiah 1:1 RSV",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**1**> The words of Jeremiah, the son of Hilki'ah, of the priests who were in An'athoth in the land of Benjamin,",
                        Reference = new Reference
                        {
                            Book = "Jeremiah",
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 1,
                            Version = _defaultBibleGatewayVersion,
                            IsOT = true,
                            IsNT = false,
                            IsDEU = false,
                            AsString = "Jeremiah 1:1"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        // TODO: test 1 Esdras + Ezra, Letter of Jeremiah + Jeremiah in same request

        [Test]
        public void ShouldProcessPsalm151Properly()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Psalm 151:1")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Psalm 151:1 RSV",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
                    {
                        Title = "",
                        PsalmTitle = "This psalm is ascribed to David as his own composition (though it is outside the number), after he had fought in single combat with Goliath.",
                        Text = "<**1**> I was small among my brothers, and youngest in my father's house; I tended my father's sheep.",
                        Reference = new Reference
                        {
                            Book = "Psalm 151",
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 1,
                            Version = _defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = false,
                            IsDEU = true,
                            AsString = "Psalm 151 1"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task ShouldThrowProviderNotFoundException()
        {
            Version testVersion = await _versionService.Create(new Version
            {
                Name = "A Test Version (TEST)",
                Abbreviation = "TEST",
                Source = "test",
                SupportsOldTestament = true,
                SupportsNewTestament = true,
                SupportsDeuterocanon = false
            });

            _ = _versesController
                .Invoking(c => c.ProcessMessage(new MockRequest("Genesis 1:1 TEST")).GetAwaiter().GetResult())
                .Should()
                .Throw<ProviderNotFoundException>();

            await _versionService.Remove(testVersion);
        }


        [Test]
        public void ShouldNotReturnDuplicates()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("John 1:1 / John 1:1")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "John 1:1 RSV",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
                    {
                        Title = "The Word Became Flesh",
                        PsalmTitle = "",
                        Text = "<**1**> In the beginning was the Word, and the Word was with God, and the Word was God.",
                        Reference = new Reference
                        {
                            Book = "John",
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 1,
                            Version = _defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "John 1:1"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task ShouldReturnDuplicatesWhenVersionsDiffer()
        {
            Version testVersion = await _versionService.Get("NTFE") ?? await _versionService.Create(new MockNTFE());

            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Philippians 1:6 / Philippians 1:6 NTFE")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Philippians 1:6 RSV / Philippians 1:6 NTFE",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**6**> And I am sure that he who began a good work in you will bring it to completion at the day of Jesus Christ.",
                        Reference = new Reference
                        {
                            Book = "Philippians",
                            StartingChapter = 1,
                            StartingVerse = 6,
                            EndingChapter = 1,
                            EndingVerse = 6,
                            Version = _defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "Philippians 1:6"
                        }
                    },
                    new()
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**6**> Of this I'm convinced: the one who began a good work in you will thoroughly complete it by the day of Messiah Jesus.",
                        Reference = new Reference
                        {
                            Book = "Philippians",
                            StartingChapter = 1,
                            StartingVerse = 6,
                            EndingChapter = 1,
                            EndingVerse = 6,
                            Version = testVersion,
                            IsOT = false,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "Philippians 1:6"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldNotProcessGreekEstherAsEsther()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Greek Esther 1:1")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Greek Esther 1:1 RSV",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
                    {
                        Title = "King Ahasu-erus Deposes Queen Vashti",
                        PsalmTitle = "",
                        Text = "<**1**> In the days of Ahasu-e'rus, the Ahasu-e'rus who reigned from India to Ethiopia over one hundred and twenty-seven provinces,",
                        Reference = new Reference
                        {
                            Book = "Greek Esther",
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 1,
                            Version = _defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = false,
                            IsDEU = true,
                            AsString = "Greek Esther 1:1"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

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
                ]
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
                ]
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
                ]
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
                ]
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
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task ShouldRemoveH4HeadingsInVerseContent()
        {
            Version testVersion = await _versionService.Get("NKJV") ?? await _versionService.Create(new MockNKJV());

            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Song of Songs 7:9-10 NKJV")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = true,
                LogStatement = "Song of Songs 7:9-10 NKJV",
                DisplayStyle = "embed",
                Verses =
                [
                    new()
                    {
                        Title = "",
                        PsalmTitle = "",
                        Text = "<**9**> And the roof of your mouth like the best wine. The wine goes down smoothly for my beloved, Moving gently the lips of sleepers. <**10**> I am my beloved's, And his desire is toward me.",
                        Reference = new Reference
                        {
                            Book = "Song of Songs",
                            StartingChapter = 7,
                            StartingVerse = 9,
                            EndingChapter = 7,
                            EndingVerse = 10,
                            Version = testVersion,
                            IsOT = true,
                            IsNT = false,
                            IsDEU = false,
                            AsString = "Song of Solomon 7:9-10"
                        }
                    }
                ]
            };

            result.StatusCode.Should().Be(200);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldNotProcessInvalidVerse()
        {
            ObjectResult result = _versesController.ProcessMessage(new MockRequest("Genesis 1:125")).GetAwaiter().GetResult().Result as ObjectResult;
            VerseResponse resp = result.Value as VerseResponse;

            VerseResponse expected = new()
            {
                OK = false,
                LogStatement = null
            };

            result.StatusCode.Should().Be(400);
            resp.Should().BeEquivalentTo(expected);
        }
    }
}