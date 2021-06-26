/*
* Copyright (C) 2016-2021 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using BibleBot.Backend.Controllers;
using BibleBot.Backend.Models;
using BibleBot.Backend.Services;
using BibleBot.Backend.Services.Providers;
using BibleBot.Backend.Tests.Mocks;
using BibleBot.Lib;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace BibleBot.Backend.Tests
{
    [TestFixture, Category("VersesController")]
    public class VersesControllerTest
    {
        private VersesController versesController;

        private VersionService versionService;

        private Mock<UserService> userServiceMock;
        private Mock<GuildService> guildServiceMock;
        private Mock<ParsingService> parsingServiceMock;
        private Mock<NameFetchingService> nameFetchingServiceMock;

        private Mock<BibleGatewayProvider> bgProviderMock;
        private Mock<APIBibleProvider> abProviderMock;

        private IDatabaseSettings databaseSettings;

        private Lib.Version defaultBibleGatewayVersion;
        private Lib.Version defaultAPIBibleVersion;

        [SetUp]
        public void Setup()
        {
            databaseSettings = new DatabaseSettings
            {
                UserCollectionName = "Users",
                GuildCollectionName = "Guilds",
                VersionCollectionName = "Versions",
                LanguageCollectionName = "Languages",
                FrontendStatsCollectionName = "FrontendStats",
                DatabaseName = "BibleBotBackend"
            };

            userServiceMock = new Mock<UserService>(databaseSettings);
            guildServiceMock = new Mock<GuildService>(databaseSettings);
            versionService = new VersionService(databaseSettings);
            parsingServiceMock = new Mock<ParsingService>(versionService);
            nameFetchingServiceMock = new Mock<NameFetchingService>();

            bgProviderMock = new Mock<BibleGatewayProvider>();
            abProviderMock = new Mock<APIBibleProvider>();

            defaultBibleGatewayVersion = versionService.Get("RSV");
            if (defaultBibleGatewayVersion == null)
            {
                defaultBibleGatewayVersion = versionService.Create(new MockRSV());
            }

            defaultAPIBibleVersion = versionService.Get("KJVA");
            if (defaultAPIBibleVersion == null)
            {
                defaultAPIBibleVersion = versionService.Create(new MockKJVA());
            }

            versesController = new VersesController(userServiceMock.Object, guildServiceMock.Object,
                                                    parsingServiceMock.Object, versionService,
                                                    nameFetchingServiceMock.Object, bgProviderMock.Object,
                                                    abProviderMock.Object);
        }

        [Test]
        public void ShouldFailWhenTokenIsInvalid()
        {
            var req = new MockRequest();
            req.Token = "meowmix";

            var resp = versesController.ProcessMessage(req).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = false,
                LogStatement = null
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldFailWhenBodyIsEmpty()
        {
            var resp = versesController.ProcessMessage(new MockRequest()).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = false,
                LogStatement = null
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessBibleGatewayReference()
        {
            var resp = versesController.ProcessMessage(new MockRequest("Matthew 1:1")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = true,
                LogStatement = "Matthew 1:1",
                DisplayStyle = "embed",
                Verses = new List<Verse>
                {
                    new Verse
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
                            Version = defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "Matthew 1:1"
                        }
                    }
                }
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessAPIBibleReference()
        {
            var resp = versesController.ProcessMessage(new MockRequest("Genesis 1:1 KJVA")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = true,
                LogStatement = "Genesis 1:1",
                DisplayStyle = "embed",
                Verses = new List<Verse>
                {
                    new Verse
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
                            Version = defaultAPIBibleVersion,
                            IsOT = true,
                            IsNT = false,
                            IsDEU = false,
                            AsString = "Genesis 1:1"
                        }
                    }
                }
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldFailWhenReferencingDeuterocanonInProtestantBible()
        {
            var testVersion = versionService.Get("NTE");

            if (testVersion == null)
            {
                testVersion = versionService.Create(new MockNTE());
            }

            var resp = versesController.ProcessMessage(new MockRequest("Sirach 1:1 NTE")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = false,
                LogStatement = "New Testament for Everyone (NTE) does not support the Apocrypha/Deuterocanon."
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldFailWhenReferencingOldTestamentInNewTestamentOnlyBible()
        {
            var testVersion = versionService.Get("NTE");

            if (testVersion == null)
            {
                testVersion = versionService.Create(new MockNTE());
            }

            var resp = versesController.ProcessMessage(new MockRequest("Genesis 1:1 NTE")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = false,
                LogStatement = "New Testament for Everyone (NTE) does not support the Old Testament."
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldIgnoreVerseReferenceInBrackets()
        {
            var resp = versesController.ProcessMessage(new MockRequest("lorem < Matthew 1:1 > ipsum John 1:1 dolor < Genesis 1:1 > sit")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = true,
                LogStatement = "John 1:1",
                DisplayStyle = "embed",
                Verses = new List<Verse>
                {
                    new Verse
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
                            Version = defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "John 1:1"
                        }
                    }
                }
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldIgnoreVerseReferenceWithVersionInBrackets()
        {
            var resp = versesController.ProcessMessage(new MockRequest("lorem < Matthew 1:1 NTE > ipsum John 1:1 dolor < Genesis 1:1 NTE > sit")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = true,
                LogStatement = "John 1:1",
                DisplayStyle = "embed",
                Verses = new List<Verse>
                {
                    new Verse
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
                            Version = defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "John 1:1"
                        }
                    }
                }
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldIgnoreMultipleVerseReferencesInBrackets()
        {
            var resp = versesController.ProcessMessage(new MockRequest("lorem < Genesis 1:1 / Matthew 1:1 / Acts 1:1 > ipsum John 1:1 dolor < Genesis 1:1 > sit")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = true,
                LogStatement = "John 1:1",
                DisplayStyle = "embed",
                Verses = new List<Verse>
                {
                    new Verse
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
                            Version = defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "John 1:1"
                        }
                    }
                }
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldIgnoreMultipleVerseReferencesWithVersionInBrackets()
        {
            var resp = versesController.ProcessMessage(new MockRequest("lorem < Genesis 1:1 NTE / Matthew 1:1 NTE / Acts 1:1 NTE > ipsum John 1:1 dolor < Genesis 1:1 NTE > sit")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = true,
                LogStatement = "John 1:1",
                DisplayStyle = "embed",
                Verses = new List<Verse>
                {
                    new Verse
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
                            Version = defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "John 1:1"
                        }
                    }
                }
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessBibleGatewaySpannedReference()
        {
            var resp = versesController.ProcessMessage(new MockRequest("Matthew 1:1-2")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = true,
                LogStatement = "Matthew 1:1-2",
                DisplayStyle = "embed",
                Verses = new List<Verse>
                {
                    new Verse
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
                            Version = defaultBibleGatewayVersion,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "Matthew 1:1-2"
                        }
                    }
                }
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessAPIBibleSpannedReference()
        {
            var resp = versesController.ProcessMessage(new MockRequest("Genesis 1:1-2 KJVA")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = true,
                LogStatement = "Genesis 1:1-2",
                DisplayStyle = "embed",
                Verses = new List<Verse>
                {
                    new Verse
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
                            Version = defaultAPIBibleVersion,
                            IsOT = true,
                            IsNT = false,
                            IsDEU = false,
                            AsString = "Genesis 1:1-2"
                        }
                    }
                }
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessBibleGatewaySpannedChapterReference()
        {
            var resp = versesController.ProcessMessage(new MockRequest("Matthew 1:25-2:1")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = true,
                LogStatement = "Matthew 1:25-2:1",
                DisplayStyle = "embed",
                Verses = new List<Verse>
                {
                    new Verse
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
                            Version = defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "Matthew 1:25-2:1"
                        }
                    }
                }
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessAPIBibleSpannedChapterReference()
        {
            var resp = versesController.ProcessMessage(new MockRequest("Genesis 1:31-2:1 KJVA")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = true,
                LogStatement = "Genesis 1:31-2:1",
                DisplayStyle = "embed",
                Verses = new List<Verse>
                {
                    new Verse
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
                            Version = defaultAPIBibleVersion,
                            IsOT = true,
                            IsNT = false,
                            IsDEU = false,
                            AsString = "Genesis 1:31-2:1"
                        }
                    }
                }
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldProcessBibleGatewayExpandedReference()
        {
            var resp = versesController.ProcessMessage(new MockRequest("Matthew 1:24-")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = true,
                LogStatement = "Matthew 1:24-",
                DisplayStyle = "embed",
                Verses = new List<Verse>
                {
                    new Verse
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
                            Version = defaultBibleGatewayVersion,
                            IsOT = false,
                            IsNT = true,
                            IsDEU = false,
                            AsString = "Matthew 1:24-25"
                        }
                    }
                }
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldIgnoreAPIBibleExpandedReference()
        {
            var resp = versesController.ProcessMessage(new MockRequest("Matthew 1:24- KJVA")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = false,
                LogStatement = null
            };

            resp.Should().BeEquivalentTo(expected);
        }
    }
}