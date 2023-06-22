/*
* Copyright (C) 2016-2023 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BibleBot.Backend.Controllers;
using BibleBot.Backend.Models;
using BibleBot.Backend.Services;
using BibleBot.Backend.Services.Providers;
using BibleBot.Backend.Tests.Mocks;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace BibleBot.Backend.Tests
{
    [TestFixture, Category("CommandsController")]
    public class CommandsControllerTest
    {
        private CommandsController commandsController;

        private VersionService versionService;

        private Mock<UserService> userServiceMock;
        private Mock<GuildService> guildServiceMock;
        private Mock<ResourceService> resourceServiceMock;
        private Mock<FrontendStatsService> frontendStatsServiceMock;

        private Mock<SpecialVerseProvider> spProviderMock;
        private Mock<BibleGatewayProvider> bgProviderMock;
        private Mock<APIBibleProvider> abProviderMock;

        private IDatabaseSettings databaseSettings;

        private Models.Version defaultBibleGatewayVersion;
        private Models.Version defaultAPIBibleVersion;

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
            resourceServiceMock = new Mock<ResourceService>();
            frontendStatsServiceMock = new Mock<FrontendStatsService>(databaseSettings);

            spProviderMock = new Mock<SpecialVerseProvider>();
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

            commandsController = new CommandsController(userServiceMock.Object, guildServiceMock.Object,
                                                    versionService, resourceServiceMock.Object,
                                                    frontendStatsServiceMock.Object, spProviderMock.Object,
                                                    bgProviderMock.Object, abProviderMock.Object);
        }

        [Test]
        public async Task ShouldFailWhenTokenIsInvalid()
        {
            var req = new MockRequest();
            req.Token = "meowmix";

            var resp = await commandsController.ProcessMessage(req);

            var expected = new CommandResponse
            {
                OK = false,
                LogStatement = null,
                Pages = null,
                CreateWebhook = false,
                RemoveWebhook = false,
                SendAnnouncement = false
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task ShouldFailWhenBodyIsEmpty()
        {
            var resp = await commandsController.ProcessMessage(new MockRequest());

            var expected = new CommandResponse
            {
                OK = false,
                LogStatement = null,
                Pages = null,
                CreateWebhook = false,
                RemoveWebhook = false,
                SendAnnouncement = false
            };

            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task ShouldProcessSearchQueryWithLargeResults()
        {
            var resp = await commandsController.ProcessMessage(new MockRequest("+search faith")) as CommandResponse;

            resp.OK.Should().BeTrue();
            resp.LogStatement.Should().NotBeNullOrEmpty();
            resp.Pages.Should().NotBeNullOrEmpty();
            resp.Pages[0].Description.Should().Contain("Page 1 of"); // Ensure we're sending back the correct order.
        }
    }
}