/*
* Copyright (C) 2016-2021 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BibleBot.AutomaticServices.Models;
using BibleBot.AutomaticServices.Services;
using BibleBot.AutomaticServices.Services.Providers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Prometheus;
using Serilog;

namespace BibleBot.AutomaticServices
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

            // Add background services.
            services.AddHostedService<AutomaticDailyVerseService>();
            services.AddHostedService<VersionStatsService>();

            // Instantiate the various services.
            services.AddSingleton<UserService>();
            services.AddSingleton<GuildService>();
            services.AddSingleton<VersionService>();

            // Instantiate the various providers, which are just services.
            services.AddSingleton<SpecialVerseProvider>();
            services.AddSingleton<BibleGatewayProvider>();
            services.AddSingleton<APIBibleProvider>();

            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "BibleBot.AutomaticServices",
                    Version = "1",
                    Description = "The AutomaticServices of BibleBot",
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
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BibleBot.AutomaticServices"));
            }
            else
            {
                app.UseExceptionHandler("/");
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

            Log.Information("AutomaticServices is ready.");
        }
    }
}
