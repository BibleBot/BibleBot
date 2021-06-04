using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;

using Serilog;
using Prometheus;

using BibleBot.Backend.Models;
using BibleBot.Backend.Services;
using BibleBot.Backend.Services.Providers;

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

            // Add background services.
            services.AddHostedService<AutomaticDailyVerseService>();

            // Instantiate the various services.
            services.AddSingleton<UserService>();
            services.AddSingleton<GuildService>();
            services.AddSingleton<ParsingService>();
            services.AddSingleton<VersionService>();
            services.AddSingleton<NameFetchingService>();
            services.AddSingleton<FrontendStatsService>();

            // Instantiate the various providers, which are just services.
            services.AddSingleton<BibleGatewayProvider>();

            services.AddControllers();

            if (!Configuration.GetSection("BibleBotBackend").GetValue<bool>("IsDevelopment"))
            {
                services.AddLettuceEncrypt();
            }

            // Run the NameFetchingService on startup without async.
            new NameFetchingService().FetchBookNames(Configuration.GetSection("BibleBotBackend").GetValue<bool>("NameFetchDryRun")).GetAwaiter().GetResult();
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "BibleBot.Backend",
                    Version = "v1",
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
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BibleBot.Backend v1"));
            }
            else
            {
                app.UseExceptionHandler("/");
                app.UseHsts();
            }

            app.UseSerilogRequestLogging();

            //app.UseHttpsRedirection();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseHttpMetrics();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapMetrics();
            });

            Log.Information("Ready at http://localhost:5000 and https://localhost:5001.");
        }
    }
}
