/*
* Copyright (C) 2016-2026 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using BibleBot.AutomaticServices.Services;
using BibleBot.Backend.Services;
using BibleBot.Backend.Services.Providers;
using BibleBot.Backend.Services.Providers.Content;
using BibleBot.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Npgsql;
using Serilog;

namespace BibleBot.AutomaticServices
{
    public class Startup(IConfiguration configuration)
    {
        private IConfiguration Configuration { get; } = configuration;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = "127.0.0.1:6379";
            });

            string connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONN");
            NpgsqlDataSourceBuilder dataSourceBuilder = new(connectionString);
            dataSourceBuilder.UseNodaTime();
            dataSourceBuilder.EnableDynamicJson();
            NpgsqlDataSource dataSource = dataSourceBuilder.Build();

            // Instantiate the various services.
            services.AddSingleton(dataSource);
            services.AddDbContextPool<PgContext>(options =>
                options.UseNpgsql(dataSource,
                    o => o.SetPostgresVersion(18, 0).UseNodaTime()
                )
            );

            services.AddScoped<PostgresService>();
            services.AddScoped<PreferenceService>();
            services.AddScoped<UserService>();
            services.AddScoped<GuildService>();
            services.AddSingleton<VersionService>();
            services.AddSingleton<LanguageService>();
            services.AddSingleton(sp => new ParsingService(true));

            services.AddSingleton(sp => new MetadataFetchingService(sp.GetRequiredService<VersionService>(), true));

            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.Configure<RequestLocalizationOptions>(options =>
            {
                string[] supportedCultures = ["en-GB", "en-US", "eo", "pl-PL"];
                options.SetDefaultCulture(supportedCultures[0])
                       .AddSupportedCultures(supportedCultures)
                       .AddSupportedUICultures(supportedCultures);
            });

            // Instantiate the various providers, which are just services.
            services.AddSingleton<SpecialVerseProvider>();
            services.AddSingleton<BibleGatewayProvider>();
            services.AddSingleton<APIBibleProvider>();
            services.AddSingleton<NLTAPIProvider>();

            // Register the list of content providers
            services.AddSingleton<List<IContentProvider>>(sp => [
                sp.GetRequiredService<BibleGatewayProvider>(),
                sp.GetRequiredService<APIBibleProvider>(),
                sp.GetRequiredService<NLTAPIProvider>()
            ]);

            // Register the special verse processing service
            services.AddScoped<SpecialVerseProcessingService>();

            // Add background services.
            services.AddHostedService<AutomaticDailyVerseService>();
            services.AddHostedService<VersionStatsService>();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                services.AddHostedService<SystemdWatchdogService>();
            }

            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v9", new OpenApiInfo
                {
                    Title = "BibleBot.AutomaticServices",
                    Version = "9",
                    Description = "The AutomaticServices of BibleBot",
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
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BibleBot.AutomaticServices"));
            }
            else
            {
                app.UseExceptionHandler("/");
            }

            app.UseSerilogRequestLogging();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            IOptions<RequestLocalizationOptions> localizationOption = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(localizationOption.Value);

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            Log.Information("AutomaticServices is ready.");
        }
    }
}
