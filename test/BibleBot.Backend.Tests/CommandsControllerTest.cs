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
    [TestFixture, Category("CommandsController")]
    public class CommandsControllerTest
    {
        private CommandsController _commandsController;

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

            _userServiceMock = new Mock<UserService>(_databaseSettings);
            _guildServiceMock = new Mock<GuildService>(_databaseSettings);
            _versionService = new VersionService(_databaseSettings);
            _resourceServiceMock = new Mock<ResourceService>();
            _frontendStatsServiceMock = new Mock<FrontendStatsService>(_databaseSettings);
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

            ActionResult<IResponse> result = await _commandsController.ProcessMessage(req);
            CommandResponse resp = (result.Result as ObjectResult).Value as CommandResponse;

            CommandResponse expected = new()
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
            ActionResult<IResponse> result = await _commandsController.ProcessMessage(new MockRequest());
            CommandResponse resp = (result.Result as ObjectResult).Value as CommandResponse;

            CommandResponse expected = new()
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
            ActionResult<IResponse> result = await _commandsController.ProcessMessage(new MockRequest("+search faith"));
            CommandResponse resp = (result.Result as ObjectResult).Value as CommandResponse;

            resp.OK.Should().BeTrue();
            resp.LogStatement.Should().NotBeNullOrEmpty();
            resp.Pages.Should().NotBeNullOrEmpty();
            resp.Pages[0].Description.Should().Contain("Page 1 of"); // Ensure we're sending back the correct order.
        }
    }
}