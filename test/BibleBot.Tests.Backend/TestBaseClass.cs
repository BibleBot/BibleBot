/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Threading.Tasks;
using BibleBot.Backend.Controllers;
using BibleBot.Backend.InternalModels;
using BibleBot.Backend.Services;
using BibleBot.Backend.Services.Providers;
using BibleBot.Backend.Services.Providers.Content;
using BibleBot.Models;
using BibleBot.Tests.Backend.Mocks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Serilog.Extensions.Logging;

namespace BibleBot.Tests.Backend
{
    [SetUpFixture]
    public class TestBaseClass
    {
        protected CommandsController _commandsController;
        protected VersesController _versesController;

        private MongoService _mongoService;
        private IDistributedCache _cache;
        private PreferenceService _preferenceService;
        protected VersionService _versionService;
        private LanguageService _languageService;

        private Mock<UserService> _userServiceMock;
        private Mock<OptOutService> _optOutServiceMock;
        private Mock<GuildService> _guildServiceMock;
        private Mock<ResourceService> _resourceServiceMock;
        private Mock<ParsingService> _parsingServiceMock;
        private Mock<FrontendStatsService> _frontendStatsServiceMock;
        private Mock<MetadataFetchingService> _metadataFetchingServiceMock;

        private Mock<SpecialVerseProvider> _spProviderMock;
        private Mock<BibleGatewayProvider> _bgProviderMock;
        private Mock<APIBibleProvider> _abProviderMock;
        private Mock<NLTAPIProvider> _nltProviderMock;

        private IDatabaseSettings _databaseSettings;

        protected Version _defaultBibleGatewayVersion;
        protected Version _defaultAPIBibleVersion;

        private readonly IStringLocalizerFactory _localizerFactory = new ResourceManagerStringLocalizerFactory(Options.Create(new LocalizationOptions { ResourcesPath = "Resources" }), new SerilogLoggerFactory());

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
                OptOutUserCollectionName = "OptOutUsers",
                DatabaseName = "BibleBotBackend"
            };

            _cache = new RedisCache(Options.Create(new RedisCacheOptions()
            {
                Configuration = "127.0.0.1:6379"
            }));

            _mongoService = new MongoService(_databaseSettings);
            _preferenceService = new PreferenceService(_cache, _mongoService);
            _optOutServiceMock = new Mock<OptOutService>(_mongoService);
            _userServiceMock = new Mock<UserService>(_preferenceService);
            _guildServiceMock = new Mock<GuildService>(_preferenceService);
            _versionService = new VersionService(_mongoService);
            _languageService = new LanguageService(_mongoService);
            _resourceServiceMock = new Mock<ResourceService>();
            _parsingServiceMock = new Mock<ParsingService>();
            _frontendStatsServiceMock = new Mock<FrontendStatsService>(_mongoService);
            _metadataFetchingServiceMock = new Mock<MetadataFetchingService>(_versionService, false);

            _spProviderMock = new Mock<SpecialVerseProvider>();
            _bgProviderMock = new Mock<BibleGatewayProvider>();
            _abProviderMock = new Mock<APIBibleProvider>();
            _nltProviderMock = new Mock<NLTAPIProvider>();

            _defaultBibleGatewayVersion = await _versionService.Get("RSV") ?? await _versionService.Create(new MockRSV());
            _defaultAPIBibleVersion = await _versionService.Get("KJV") ?? await _versionService.Create(new MockKJV());

            _commandsController = new CommandsController(_userServiceMock.Object, _optOutServiceMock.Object, _guildServiceMock.Object,
                                                    _versionService, _resourceServiceMock.Object,
                                                    _frontendStatsServiceMock.Object, _languageService, _metadataFetchingServiceMock.Object,
                                                    _spProviderMock.Object, _bgProviderMock.Object, _abProviderMock.Object, _nltProviderMock.Object, _localizerFactory);

            _versesController = new VersesController(_userServiceMock.Object, _guildServiceMock.Object,
                                                    _parsingServiceMock.Object, _versionService, _languageService,
                                                    _metadataFetchingServiceMock.Object, _bgProviderMock.Object,
                                                    _abProviderMock.Object, _nltProviderMock.Object, _optOutServiceMock.Object, new StringLocalizer<VersesController>(_localizerFactory), new StringLocalizer<SharedResource>(_localizerFactory));
        }
    }
}
