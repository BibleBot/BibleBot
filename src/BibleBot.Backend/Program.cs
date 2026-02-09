/*
* Copyright (C) 2016-2026 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace BibleBot.Backend
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Level:w4}] {Message:lj}{NewLine}{Exception}", theme: AnsiConsoleTheme.Code)
                .CreateBootstrapLogger();

            Log.Information($"BibleBot {Utils.Version} (Backend) by Kerygma Digital");
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args)
                .UseSerilog((context, services, configuration) => configuration
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .WriteTo.Console(outputTemplate: "[{Level:w4}] {Message:lj}{NewLine}{Exception}", theme: AnsiConsoleTheme.Code))
                    .UseSystemd();

            return hostBuilder.ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseSentry(options =>
                {
                    options.Dsn = Environment.GetEnvironmentVariable("SENTRY_DSN");
                    options.MaxRequestBodySize = Sentry.Extensibility.RequestSize.None;
                    options.CaptureBlockingCalls = true;

                    string version = Utils.GetVersion();
                    if (version != "undefined")
                    {
                        options.Release = version;
                    }
                    options.Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                }).UseStartup<Startup>();
            });
        }
    }
}
