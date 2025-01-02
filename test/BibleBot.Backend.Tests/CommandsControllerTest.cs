/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
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
    [TestFixture, Category("CommandsController")]
    public class CommandsControllerTest
    {
        private CommandsController _commandsController;

        private MongoService _mongoService;
        private VersionService _versionService;

        private Mock<UserService> _userServiceMock;
        private Mock<GuildService> _guildServiceMock;
        private Mock<ResourceService> _resourceServiceMock;
        private Mock<FrontendStatsService> _frontendStatsServiceMock;
        private Mock<NameFetchingService> _nameFetchingServiceMock;

        private Mock<SpecialVerseProvider> _spProviderMock;
        private Mock<BibleGatewayProvider> _bgProviderMock;
        private Mock<APIBibleProvider> _abProviderMock;

        private IDatabaseSettings _databaseSettings;

        // private Version _defaultBibleGatewayVersion;
        // private Version _defaultAPIBibleVersion;

        [OneTimeSetUp]
        public void OneTimeSetup()
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

            _mongoService = new MongoService(_databaseSettings);
            _userServiceMock = new Mock<UserService>(_mongoService);
            _guildServiceMock = new Mock<GuildService>(_mongoService);
            _versionService = new VersionService(_mongoService);
            _resourceServiceMock = new Mock<ResourceService>();
            _frontendStatsServiceMock = new Mock<FrontendStatsService>(_mongoService);
            _nameFetchingServiceMock = new Mock<NameFetchingService>();

            _spProviderMock = new Mock<SpecialVerseProvider>();
            _bgProviderMock = new Mock<BibleGatewayProvider>();
            _abProviderMock = new Mock<APIBibleProvider>();

            // _defaultBibleGatewayVersion = await _versionService.Get("RSV") ?? await _versionService.Create(new MockRSV());
            // _defaultAPIBibleVersion = await _versionService.Get("KJVA") ?? await _versionService.Create(new MockKJVA());

            _commandsController = new CommandsController(_userServiceMock.Object, _guildServiceMock.Object,
                                                    _versionService, _resourceServiceMock.Object,
                                                    _frontendStatsServiceMock.Object, _nameFetchingServiceMock.Object,
                                                    _spProviderMock.Object, _bgProviderMock.Object, _abProviderMock.Object);
        }

        [Test]
        public async Task ShouldFailWhenTokenIsInvalid()
        {
            MockRequest req = new()
            {
                Token = "meowmix"
            };

            ObjectResult result = (await _commandsController.ProcessMessage(req)).Result as ObjectResult;
            CommandResponse resp = result.Value as CommandResponse;

            CommandResponse expected = new()
            {
                OK = false,
                LogStatement = null,
                Pages = null,
                CreateWebhook = false,
                RemoveWebhook = false,
                SendAnnouncement = false
            };

            result.StatusCode.Should().Be(403);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task ShouldFailWhenBodyIsEmpty()
        {
            ObjectResult result = (await _commandsController.ProcessMessage(new MockRequest())).Result as ObjectResult;
            CommandResponse resp = result.Value as CommandResponse;

            CommandResponse expected = new()
            {
                OK = false,
                LogStatement = null,
                Pages = null,
                CreateWebhook = false,
                RemoveWebhook = false,
                SendAnnouncement = false
            };

            result.StatusCode.Should().Be(400);
            resp.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task ShouldProcessSearchQueryWithLargeResults()
        {
            ObjectResult result = (await _commandsController.ProcessMessage(new MockRequest("+search faith"))).Result as ObjectResult;
            CommandResponse resp = result.Value as CommandResponse;

            result.StatusCode.Should().Be(200);
            resp.OK.Should().BeTrue();
            resp.LogStatement.Should().NotBeNullOrEmpty();
            resp.Pages.Should().NotBeNullOrEmpty();
            resp.Pages[0].Description.Should().Contain("Page 1 of"); // Ensure we're sending back the correct order.
        }
    }
}