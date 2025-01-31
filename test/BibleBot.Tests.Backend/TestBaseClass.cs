/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using BibleBot.Backend.Controllers;
using BibleBot.Backend.Services;
using BibleBot.Backend.Services.Providers;
using BibleBot.Tests.Backend.Mocks;
using BibleBot.Models;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Serilog.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BibleBot.Tests.Backend
{
    [SetUpFixture]
    public class TestBaseClass
    {
        public CommandsController _commandsController;
        public VersesController _versesController;

        private MongoService _mongoService;
        public VersionService _versionService;
        public LanguageService _languageService;

        public Mock<UserService> _userServiceMock;
        public Mock<GuildService> _guildServiceMock;
        public Mock<ResourceService> _resourceServiceMock;
        public Mock<ParsingService> _parsingServiceMock;
        public Mock<FrontendStatsService> _frontendStatsServiceMock;
        public Mock<NameFetchingService> _nameFetchingServiceMock;

        public Mock<SpecialVerseProvider> _spProviderMock;
        public Mock<BibleGatewayProvider> _bgProviderMock;
        public Mock<APIBibleProvider> _abProviderMock;

        private IDatabaseSettings _databaseSettings;

        public Version _defaultBibleGatewayVersion;
        public Version _defaultAPIBibleVersion;

        public IStringLocalizerFactory _localizerFactory = new ResourceManagerStringLocalizerFactory(Options.Create(new LocalizationOptions { ResourcesPath = "Resources" }), new SerilogLoggerFactory());

        [OneTimeSetUp]
        public async Task RunBeforeAnyTests()
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
            _languageService = new LanguageService(_mongoService);
            _resourceServiceMock = new Mock<ResourceService>();
            _parsingServiceMock = new Mock<ParsingService>();
            _frontendStatsServiceMock = new Mock<FrontendStatsService>(_mongoService);
            _nameFetchingServiceMock = new Mock<NameFetchingService>();

            _spProviderMock = new Mock<SpecialVerseProvider>();
            _bgProviderMock = new Mock<BibleGatewayProvider>();
            _abProviderMock = new Mock<APIBibleProvider>();

            _defaultBibleGatewayVersion = await _versionService.Get("RSV") ?? await _versionService.Create(new MockRSV());
            _defaultAPIBibleVersion = await _versionService.Get("KJVA") ?? await _versionService.Create(new MockKJVA());

            _commandsController = new CommandsController(_userServiceMock.Object, _guildServiceMock.Object,
                                                    _versionService, _resourceServiceMock.Object,
                                                    _frontendStatsServiceMock.Object, _languageService, _nameFetchingServiceMock.Object,
                                                    _spProviderMock.Object, _bgProviderMock.Object, _abProviderMock.Object, _localizerFactory);

            _versesController = new VersesController(_userServiceMock.Object, _guildServiceMock.Object,
                                                    _parsingServiceMock.Object, _versionService, _languageService,
                                                    _nameFetchingServiceMock.Object, _bgProviderMock.Object,
                                                    _abProviderMock.Object);

            return;
        }
    }
}