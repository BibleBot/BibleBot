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

            if (versionService.Get("RSV") == null)
            {
                versionService.Create(new MockRSV());
            }

            versesController = new VersesController(userServiceMock.Object, guildServiceMock.Object,
                                                    parsingServiceMock.Object, versionService,
                                                    nameFetchingServiceMock.Object, bgProviderMock.Object,
                                                    abProviderMock.Object);
        }

        [Test]
        public void ShouldProcessBibleGatewayReference()
        {
            var testVersion = versionService.Get("NTE");

            if (testVersion == null)
            {
                testVersion = versionService.Create(new MockNTE());
            }

            var resp = versesController.ProcessMessage(new MockRequest("Matthew 1:1 NTE")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = true,
                LogStatement = "Matthew 1:1",
                DisplayStyle = "embed",
                Verses = new List<Verse>
                {
                    new Verse
                    {
                        Title = "Jesus’ Genealogy",
                        PsalmTitle = "",
                        Text = "<**1**> The book of the family tree of Jesus the Messiah, the son of David, the son of Abraham.",
                        Reference = new Reference
                        {
                            Book = "Matthew",
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 1,
                            Version = testVersion,
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
            var testVersion = versionService.Get("KJVA");

            if (testVersion == null)
            {
                testVersion = versionService.Create(new MockKJVA());
            }

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
                            Version = testVersion,
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
            var resp = versesController.ProcessMessage(new MockRequest("lorem < Genesis 1:1 > ipsum")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = false,
                LogStatement = null
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldIgnoreVerseReferenceWithVersionInBrackets()
        {
            var resp = versesController.ProcessMessage(new MockRequest("lorem < Genesis 1:1 NTE > ipsum")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = false,
                LogStatement = null
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldIgnoreMultipleVerseReferencesInBrackets()
        {
            var resp = versesController.ProcessMessage(new MockRequest("lorem < Genesis 1:1 / Matthew 1:1 / John 1:1 > ipsum")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = false,
                LogStatement = null
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldIgnoreMultipleVerseReferencesWithVersionInBrackets()
        {
            var resp = versesController.ProcessMessage(new MockRequest("lorem < Genesis 1:1 NTE / Matthew 1:1 NTE / John 1:1 NTE > ipsum")).GetAwaiter().GetResult();

            var expected = new VerseResponse
            {
                OK = false,
                LogStatement = null
            };

            resp.Should().BeEquivalentTo(expected);
        }
    }
}