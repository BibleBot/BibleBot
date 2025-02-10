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
using BibleBot.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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

            // Instantiate the various services.
            MongoService mongoService = new(Configuration.GetSection(nameof(DatabaseSettings)).Get<DatabaseSettings>());
            services.AddSingleton(mongoService);

            UserService userService = new(mongoService);
            services.AddSingleton(userService);

            GuildService guildService = new(mongoService);
            services.AddSingleton(guildService);

            services.AddSingleton<ParsingService>();
            services.AddSingleton<VersionService>();

            LanguageService languageService = new(mongoService);
            services.AddSingleton(languageService);

            services.AddSingleton<ResourceService>();
            services.AddSingleton<FrontendStatsService>();
            services.AddSingleton<OptOutService>();

            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.Configure<RequestLocalizationOptions>(options =>
            {
                string[] supportedCultures = ["en-US", "eo"];
                options.SetDefaultCulture(supportedCultures[0])
                       .AddSupportedCultures(supportedCultures)
                       .AddSupportedUICultures(supportedCultures);

                options.AddInitialRequestCultureProvider(new PreferenceRequestCultureProvider(userService, guildService, languageService));
            });

            // Instantiate the various providers, which are just services.
            services.AddSingleton<SpecialVerseProvider>();
            services.AddSingleton<BibleGatewayProvider>();
            services.AddSingleton<APIBibleProvider>();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                services.AddHostedService<SystemdWatchdogService>();
            }

            // Add the name fetching service with a predefined instance, since we'll use it later in this function.
            NameFetchingService nameFetchingService = new();
            services.AddSingleton(nameFetchingService);

            services.AddResponseCaching();
            services.AddResponseCompression();
            services.AddControllers();

            // Run the NameFetchingService on startup without async.
            nameFetchingService.FetchBookNames(Configuration.GetSection("BibleBotBackend").GetValue<bool>("NameFetchDryRun")).GetAwaiter().GetResult();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "BibleBot.Backend",
                    Version = "1",
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
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BibleBot.Backend"));
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
