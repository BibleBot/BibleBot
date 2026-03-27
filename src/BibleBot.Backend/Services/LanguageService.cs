/*
* Copyright (C) 2016-2026 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BibleBot.Models;

using Microsoft.Extensions.DependencyInjection;
using System.Threading;

namespace BibleBot.Backend.Services
{
    public class LanguageService(IServiceScopeFactory scopeFactory)
    {
        private List<Language> _languages = [];
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public async Task<List<Language>> GetLanguages(bool forcePull = false)
        {
            if (!forcePull && _languages.Count != 0)
            {
                return [.. _languages];
            }

            await _semaphore.WaitAsync();
            try
            {
                if (!forcePull && _languages.Count != 0)
                {
                    return [.. _languages];
                }

                using IServiceScope scope = scopeFactory.CreateScope();
                PostgresService postgresService = scope.ServiceProvider.GetRequiredService<PostgresService>();
                _languages = await postgresService.Get<Language>();
            }
            finally
            {
                _semaphore.Release();
            }

            return [.. _languages];
        }

        public async Task<List<Language>> Get() => await GetLanguages();
        public async Task<Language> Get(string culture) => (await GetLanguages()).FirstOrDefault(language => language.Id == culture);

        public async Task<Language> GetPreferenceOrDefault(User idealUser, Guild idealGuild, bool isBot)
        {
            Language idealLanguage = await Get("en-US");

            if (idealUser != null && !isBot)
            {
                idealLanguage = await Get(idealUser.Language);
            }
            else if (idealGuild != null)
            {
                idealLanguage = await Get(idealGuild.Language);
            }

            return idealLanguage ?? await Get("en-US");
        }

        public async Task<Language> Create(Language language)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            PostgresService postgresService = scope.ServiceProvider.GetRequiredService<PostgresService>();
            Language createdLanguage = await postgresService.Create(language);
            await GetLanguages(true);

            return createdLanguage;
        }
    }
}
