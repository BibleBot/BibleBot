/*
* Copyright (C) 2016-2026 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BibleBot.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using Version = BibleBot.Models.Version;

namespace BibleBot.Backend.Services
{
    public class VersionService(IServiceScopeFactory scopeFactory)
    {
        private List<Version> _versions = [];
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public async Task<List<Version>> GetVersions(bool forcePull = false)
        {
            if (!forcePull && _versions.Count != 0)
            {
                return [.. _versions];
            }

            await _semaphore.WaitAsync();
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                PostgresService postgresService = scope.ServiceProvider.GetRequiredService<PostgresService>();
                _versions = await postgresService.Get<Version>();
            }
            finally
            {
                _semaphore.Release();
            }

            return [.. _versions];
        }

        public async Task<List<Version>> Get() => await GetVersions();
        public async Task<Version> Get(string abbreviation) => (await GetVersions()).FirstOrDefault(version => string.Equals(version.Id, abbreviation, StringComparison.OrdinalIgnoreCase));
        public async Task<long> GetCount()
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            PostgresService postgresService = scope.ServiceProvider.GetRequiredService<PostgresService>();
            return await postgresService.GetCount<Version>();
        }

        public async Task<Version> GetPreferenceOrDefault(User idealUser, Guild idealGuild, bool isBot)
        {
            Version idealVersion = await Get("RSV");

            if (idealUser != null && !isBot)
            {
                idealVersion = await Get(idealUser.Version);
            }
            else if (idealGuild != null)
            {
                idealVersion = await Get(idealGuild.Version);
            }

            return idealVersion ?? await Get("RSV");
        }

        public async Task<Version> GetPreferenceOrDefault(User idealUser, bool isBot)
        {
            Version idealVersion = await Get("RSV");

            if (idealUser != null && !isBot)
            {
                idealVersion = await Get(idealUser.Version);
            }

            return idealVersion ?? await Get("RSV");
        }

        public async Task<Version> GetPreferenceOrDefault(Guild idealGuild, bool isBot)
        {
            Version idealVersion = await Get("RSV");

            if (idealGuild != null)
            {
                idealVersion = await Get(idealGuild.Version);
            }

            return idealVersion ?? await Get("RSV");
        }

        public async Task<Version> Create(Version version)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            PostgresService postgresService = scope.ServiceProvider.GetRequiredService<PostgresService>();
            Version createdVersion = await postgresService.Create(version);
            await GetVersions(true);

            return createdVersion;
        }

        public async Task Update(string abbreviation, UpdateDef<Version> updateDef)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            PostgresService postgresService = scope.ServiceProvider.GetRequiredService<PostgresService>();

            Version beforeVersion = await Get(abbreviation);
            await postgresService.Update(abbreviation, updateDef);

            Version afterVersion = await postgresService.Get<Version>(abbreviation);

            await _semaphore.WaitAsync();
            try
            {
                _versions.Remove(beforeVersion);
                _versions.Add(afterVersion);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task Remove(Version idealVersion)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            PostgresService postgresService = scope.ServiceProvider.GetRequiredService<PostgresService>();

            await postgresService.Remove(idealVersion);
            await GetVersions(true);
        }
    }
}
