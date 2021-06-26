using System;
using System.Collections.Generic;
using BibleBot.Backend.Controllers;
using BibleBot.Backend.Models;
using BibleBot.Backend.Services;
using BibleBot.Backend.Services.Providers;
using BibleBot.Lib;
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

            versesController = new VersesController(userServiceMock.Object, guildServiceMock.Object,
                                                    parsingServiceMock.Object, versionService,
                                                    nameFetchingServiceMock.Object, bgProviderMock.Object,
                                                    abProviderMock.Object);
        }

        [Test]
        public void BibleGatewayVerses()
        {
            var resp = versesController.ProcessMessage(new Request
            {
                UserId = "000000",
                UserPermissions = 8589934591,
                GuildId = "000000",
                IsDM = false,
                Token = Environment.GetEnvironmentVariable("ENDPOINT_TOKEN"),
                Body = "Genesis 1:1 RSV"
            });

            var expectedResult = new VerseResponse
            {
                OK = true,
                LogStatement = "Genesis 1:1",
                DisplayStyle = "embed",
                Verses = new List<Verse>
                {
                    new Verse
                    {
                        Title = "Six Days of Creation and the Sabbath",
                        PsalmTitle = "",
                        Text = "<**1**> In the beginning God created the heavens and the earth.",
                        Reference = new Reference
                        {
                            Book = "Genesis",
                            StartingChapter = 1,
                            StartingVerse = 1,
                            EndingChapter = 1,
                            EndingVerse = 1,
                            Version = new Lib.Version
                            {
                                Name = "Revised Standard Version (RSV)",
                                Abbreviation = "RSV",
                                Source = "bg",
                                SupportsOldTestament = true,
                                SupportsNewTestament = true,
                                SupportsDeuterocanon = true
                            }
                        }
                    }
                }
            };

            Assert.AreEqual(expectedResult, resp);
        }
    }
}