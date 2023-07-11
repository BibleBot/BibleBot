/*
* Copyright (C) 2016-2023 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.IO;
using System.Reflection;
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
using Prometheus;
using Serilog;

namespace BibleBot.Backend
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Link settings in appsettings.json to a database settings model.
            services.Configure<DatabaseSettings>(Configuration.GetSection(nameof(DatabaseSettings)));
            services.AddSingleton<IDatabaseSettings>(sp => sp.GetRequiredService<IOptions<DatabaseSettings>>().Value);

            // Instantiate the various services.
            services.AddSingleton<UserService>();
            services.AddSingleton<GuildService>();
            services.AddSingleton<ParsingService>();
            services.AddSingleton<VersionService>();
            services.AddSingleton<ResourceService>();
            services.AddSingleton<FrontendStatsService>();

            // Instantiate the various providers, which are just services.
            services.AddSingleton<SpecialVerseProvider>();
            services.AddSingleton<BibleGatewayProvider>();
            services.AddSingleton<APIBibleProvider>();

            // Add the name fetching service with a predefined instance, since we'll use it later in this function.
            var nameFetchingService = new NameFetchingService();
            services.AddSingleton<NameFetchingService>(nameFetchingService);

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
                        Name = "Seraphim R.P.",
                        Email = "srp@kerygma.digital"
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Mozilla Public License 2.0",
                        Url = new Uri("https://www.mozilla.org/en-US/MPL/2.0/")
                    }
                });

                // Utilize XML comments in Swagger documentation.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
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
                    var exception = context.Features.Get<IExceptionHandlerPathFeature>().Error;
                    await context.Response.WriteAsJsonAsync(new { error = exception.Message });
                }));
            }

            app.UseSerilogRequestLogging();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseHttpMetrics();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapMetrics();
            });

            Log.Information("Backend is ready.");
        }
    }
}
