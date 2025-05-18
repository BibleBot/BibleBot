/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using BibleBot.Backend.Middleware;
using BibleBot.Backend.Services;
using BibleBot.Backend.Services.Providers;
using BibleBot.Backend.Services.Providers.Content;
using BibleBot.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Serilog;

namespace BibleBot.Backend
{
    public class Startup(IConfiguration configuration)
    {
        public IConfiguration Configuration { get; } = configuration;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Link settings in appsettings.json to a database settings model.
            services.Configure<DatabaseSettings>(Configuration.GetSection(nameof(DatabaseSettings)));
            services.AddSingleton<IDatabaseSettings>(sp => sp.GetRequiredService<IOptions<DatabaseSettings>>().Value);

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = "127.0.0.1:6379";
            });

            // Instantiate the various services.
            services.AddSingleton(sp => new MongoService(Configuration.GetSection(nameof(DatabaseSettings)).Get<DatabaseSettings>()));

            services.AddSingleton(sp => new UserService(sp.GetRequiredService<IDistributedCache>(), sp.GetRequiredService<MongoService>()));
            services.AddSingleton(sp => new GuildService(sp.GetRequiredService<IDistributedCache>(), sp.GetRequiredService<MongoService>()));
            services.AddSingleton(sp => new VersionService(sp.GetRequiredService<MongoService>()));
            services.AddSingleton(sp => new LanguageService(sp.GetRequiredService<MongoService>()));

            services.AddSingleton<ParsingService>();
            services.AddSingleton<ResourceService>();
            services.AddSingleton<FrontendStatsService>();
            services.AddSingleton<OptOutService>();

            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.AddOptions<RequestLocalizationOptions>().Configure<UserService, GuildService, LanguageService>((options, userService, guildService, languageService) =>
            {
                string[] supportedCultures = ["en-GB", "en-US", "eo", "pl-PL"];
                options.SetDefaultCulture(supportedCultures[0])
                       .AddSupportedCultures(supportedCultures)
                       .AddSupportedUICultures(supportedCultures);

                options.AddInitialRequestCultureProvider(new PreferenceRequestCultureProvider(userService, guildService, languageService));
            });

            // Instantiate the various providers, which are just services.
            services.AddSingleton<SpecialVerseProvider>();
            services.AddSingleton<BibleGatewayProvider>();
            services.AddSingleton<APIBibleProvider>();
            services.AddSingleton<NLTAPIProvider>();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                services.AddHostedService<SystemdWatchdogService>();
            }

            services.AddSingleton(sp => new MetadataFetchingService(sp.GetRequiredService<VersionService>(), false));

            services.AddResponseCaching();
            services.AddResponseCompression();
            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v9", new OpenApiInfo
                {
                    Title = "BibleBot.Backend",
                    Version = "9",
                    Description = "The Backend of BibleBot",
                    Contact = new OpenApiContact
                    {
                        Name = "Seraphim R. Pardee",
                        Email = "srp@kerygma.digital"
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Mozilla Public License 2.0",
                        Url = new Uri("https://www.mozilla.org/en-US/MPL/2.0/")
                    }
                });

                // Utilize XML comments in Swagger documentation.
                string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, MetadataFetchingService metadataFetchingService, VersionService versionService)
        {
            versionService.Get().GetAwaiter().GetResult();
            metadataFetchingService.FetchMetadata(Configuration.GetSection("BibleBotBackend").GetValue<bool>("MetadataFetchDryRun")).GetAwaiter().GetResult();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v9/swagger.json", "BibleBot.Backend"));
            }
            else
            {
                app.UseExceptionHandler(c => c.Run(async context =>
                {
                    Exception exception = context.Features.Get<IExceptionHandlerPathFeature>().Error;
                    await context.Response.WriteAsJsonAsync(new { OK = false, logStatement = exception.Message });
                }));
            }

            app.UseSerilogRequestLogging();

            app.UseResponseCaching();
            app.UseResponseCompression();
            app.UseDefaultFiles();
            app.UseStaticFiles();

            IOptions<RequestLocalizationOptions> localizationOption = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(localizationOption.Value);

            app.UseHouseAuthorization();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            Log.Information("Backend is ready.");
        }
    }
}
