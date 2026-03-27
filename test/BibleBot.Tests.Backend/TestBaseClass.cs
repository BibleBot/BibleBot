/*
* Copyright (C) 2016-2026 Kerygma Digital Co.
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
using BibleBot.Backend.Services.Providers.Content;
using BibleBot.Models;
using BibleBot.Tests.Backend.Mocks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Moq;
using Npgsql;
using NUnit.Framework;
using Serilog.Extensions.Logging;

using Version = BibleBot.Models.Version;

namespace BibleBot.Tests.Backend
{
    [SetUpFixture]
    public class TestBaseClass
    {
        protected CommandsController _commandsController;
        protected VersesController _versesController;

        private Mock<IServiceProvider> _serviceProviderMock;
        private Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private Mock<IServiceScope> _serviceScopeMock;

        private PostgresService _postgresService;
        private IDistributedCache _cache;
        private PreferenceService _preferenceService;
        protected VersionService _versionService;
        private LanguageService _languageService;
        private ExperimentService _experimentService;
        protected VerseMetricsService _verseMetricsService;

        private Mock<UserService> _userServiceMock;
        private Mock<GuildService> _guildServiceMock;
        private Mock<ResourceService> _resourceServiceMock;
        private Mock<ParsingService> _parsingServiceMock;
        private Mock<FrontendStatsService> _frontendStatsServiceMock;
        private Mock<MetadataFetchingService> _metadataFetchingServiceMock;

        private Mock<SpecialVerseProvider> _spProviderMock;
        private Mock<BibleGatewayProvider> _bgProviderMock;
        private Mock<APIBibleProvider> _abProviderMock;
        private Mock<NLTAPIProvider> _nltProviderMock;

        protected Language _defaultLanguage;
        protected Version _defaultBibleGatewayVersion;
        protected Version _defaultAPIBibleVersion;

        private readonly IStringLocalizerFactory _localizerFactory = new ResourceManagerStringLocalizerFactory(Options.Create(new LocalizationOptions { ResourcesPath = "Resources" }), new SerilogLoggerFactory());

        [OneTimeSetUp]
        public async Task RunBeforeAnyTests()
        {
            _cache = new RedisCache(Options.Create(new RedisCacheOptions()
            {
                Configuration = "127.0.0.1:6379"
            }));

            string connectionString = System.Environment.GetEnvironmentVariable("POSTGRES_CONN");
            NpgsqlDataSourceBuilder dataSourceBuilder = new(connectionString);
            dataSourceBuilder.UseNodaTime();
            dataSourceBuilder.EnableDynamicJson();
            NpgsqlDataSource dataSource = dataSourceBuilder.Build();

            DbContextOptions<PgContext> pgOptions = new DbContextOptionsBuilder<PgContext>()
                .UseNpgsql(dataSource,
                    o => o.SetPostgresVersion(18, 0).UseNodaTime()
                )
                .Options;

            PgContext pgContext = new(pgOptions);
            await pgContext.Database.MigrateAsync();
            _postgresService = new PostgresService(pgContext);

            _serviceProviderMock = new Mock<IServiceProvider>();
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(PostgresService))).Returns(_postgresService);

            _serviceScopeMock = new Mock<IServiceScope>();
            _serviceScopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);

            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _serviceScopeFactoryMock.Setup(sf => sf.CreateScope()).Returns(_serviceScopeMock.Object);

            _preferenceService = new PreferenceService(_cache, _postgresService);
            _userServiceMock = new Mock<UserService>(_preferenceService);
            _guildServiceMock = new Mock<GuildService>(_preferenceService);
            _versionService = new VersionService(_serviceScopeFactoryMock.Object);
            _languageService = new LanguageService(_serviceScopeFactoryMock.Object);
            _experimentService = new ExperimentService(_serviceScopeFactoryMock.Object);
            _verseMetricsService = new VerseMetricsService(pgContext);
            _resourceServiceMock = new Mock<ResourceService>();
            _parsingServiceMock = new Mock<ParsingService>(false);
            _frontendStatsServiceMock = new Mock<FrontendStatsService>(_postgresService);
            _metadataFetchingServiceMock = new Mock<MetadataFetchingService>(_versionService, false);

            _spProviderMock = new Mock<SpecialVerseProvider>();
            _bgProviderMock = new Mock<BibleGatewayProvider>();
            _abProviderMock = new Mock<APIBibleProvider>();
            _nltProviderMock = new Mock<NLTAPIProvider>();

            _defaultLanguage = await _languageService.Get("en-US") ?? await _languageService.Create(new MockEnglish());
            _defaultBibleGatewayVersion = await _versionService.Get("RSV") ?? await _versionService.Create(new MockRSV());
            _defaultAPIBibleVersion = await _versionService.Get("KJV") ?? await _versionService.Create(new MockKJV());

            var bibleProviders = new List<IContentProvider> { _bgProviderMock.Object, _abProviderMock.Object, _nltProviderMock.Object };
            var specialVerseProcessingService = new SpecialVerseProcessingService(_parsingServiceMock.Object, _metadataFetchingServiceMock.Object, _versionService, _spProviderMock.Object, bibleProviders);

            _commandsController = new CommandsController(_userServiceMock.Object, _guildServiceMock.Object,
                                                    _versionService, _resourceServiceMock.Object,
                                                    _frontendStatsServiceMock.Object, _languageService, _metadataFetchingServiceMock.Object,
                                                    specialVerseProcessingService, _experimentService, bibleProviders, _localizerFactory);

            _versesController = new VersesController(_userServiceMock.Object, _guildServiceMock.Object,
                                                    _parsingServiceMock.Object, _verseMetricsService, _versionService, _languageService,
                                                    _metadataFetchingServiceMock.Object, _experimentService, bibleProviders, new StringLocalizer<VersesController>(_localizerFactory), new StringLocalizer<SharedResource>(_localizerFactory));
        }
    }
}
