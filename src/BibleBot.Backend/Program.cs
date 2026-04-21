/*
* Copyright (C) 2016-2026 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BibleBot.Backend.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace BibleBot.Backend
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Level:w4}] {Message:lj}{NewLine}{Exception}", theme: AnsiConsoleTheme.Code)
                .CreateBootstrapLogger();

            Log.Information($"BibleBot {Utils.Version} (Backend) by Kerygma Digital");

            // Handle CLI commands before starting the web server
            if (args.Contains("--import-usx"))
            {
                await RunUsxImport(args);
                return;
            }

            CreateHostBuilder(args).Build().Run();
        }

        private static async Task RunUsxImport(string[] args)
        {
            string path = GetArgValue(args, "--path");
            string versionId = GetArgValue(args, "--version");

            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(versionId))
            {
                Log.Error("Usage: dotnet run -- --import-usx --path <directory> --version <version-id>");
                Log.Error("  --path      Path to directory containing .usx files (absolute, or relative to repo root)");
                Log.Error("  --version   Version abbreviation (e.g., NIV)");
                return;
            }

            // Resolve relative paths against the repository root.
            // dotnet run --project changes the CWD to the project directory,
            // so relative paths from the command line won't resolve correctly.
            path = ResolvePathFromRepoRoot(path);
            Log.Information("Resolved import path: {Path}", path);

            // Build a minimal host with just DB access and logging
            IHost host = Host.CreateDefaultBuilder(args)
                .UseSerilog((context, services, configuration) => configuration
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .WriteTo.Console(outputTemplate: "[{Level:w4}] {Message:lj}{NewLine}{Exception}", theme: AnsiConsoleTheme.Code))
                .ConfigureServices((context, services) =>
                {
                    string connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONN");
                    Npgsql.NpgsqlDataSourceBuilder dataSourceBuilder = new(connectionString);
                    dataSourceBuilder.EnableDynamicJson();
                    Npgsql.NpgsqlDataSource dataSource = dataSourceBuilder.Build();

                    services.AddSingleton(dataSource);
                    services.AddDbContextPool<BibleBot.Models.PgContext>(options =>
                        options.UseNpgsql(dataSource, o => o.UseNodaTime())
                    );
                    services.AddSingleton<UsxParserService>();
                })
                .Build();

            UsxParserService parser = host.Services.GetRequiredService<UsxParserService>();
            UsxImportSummary summary = await parser.ImportDirectory(path, versionId);

            Log.Information("=== Import Summary ===");
            Log.Information("Books processed: {BooksProcessed}", summary.BooksProcessed);
            Log.Information("Verses inserted: {VersesInserted}", summary.VersesInserted);
            Log.Information("Verses updated:  {VersesUpdated}", summary.VersesUpdated);
            Log.Information("Titles updated:  {TitlesUpdated}", summary.TitlesUpdated);

            if (summary.BooksSkipped.Count > 0)
            {
                Log.Warning("Books skipped: {BooksSkipped}", string.Join(", ", summary.BooksSkipped));
            }

            if (summary.Errors.Count > 0)
            {
                Log.Error("Errors:");
                foreach (string error in summary.Errors)
                {
                    Log.Error("  {Error}", error);
                }
            }
        }

        private static string GetArgValue(string[] args, string key)
        {
            int index = Array.IndexOf(args, key);
            if (index >= 0 && index < args.Length - 1)
            {
                return args[index + 1];
            }
            return null;
        }

        /// <summary>
        /// Resolves a path that may be relative to the repository root.
        /// <c>dotnet run --project</c> changes the CWD to the project directory,
        /// so relative paths from the command line won't resolve correctly.
        /// This method walks up from the assembly location to find the repo root (.git dir).
        /// </summary>
        private static string ResolvePathFromRepoRoot(string path)
        {
            // If the path is already absolute and exists, use it directly
            if (Path.IsPathRooted(path))
            {
                return path;
            }

            // If it happens to exist relative to current directory, use it
            string fullPath = Path.GetFullPath(path);
            if (Directory.Exists(fullPath))
            {
                return fullPath;
            }

            // Walk up from the assembly location to find the repo root
            string assemblyDir = Path.GetDirectoryName(AppContext.BaseDirectory);
            DirectoryInfo dir = new(assemblyDir);
            while (dir != null)
            {
                if (Directory.Exists(Path.Combine(dir.FullName, ".git")))
                {
                    string repoRelativePath = Path.Combine(dir.FullName, path);
                    if (Directory.Exists(repoRelativePath))
                    {
                        return repoRelativePath;
                    }

                    // Found .git but the path still doesn't exist — return
                    // the repo-relative path so the error message is clearer
                    return repoRelativePath;
                }

                dir = dir.Parent;
            }

            // Fallback: return as-is, ImportDirectory will report the error
            return path;
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
