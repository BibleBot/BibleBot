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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Version = BibleBot.Models.Version;

namespace BibleBot.Backend.Services
{
    public class PostgresService(PgContext pgContext)
    {
        public async Task<List<T>> Get<T>(bool includeRelated = true) where T : class
        {
            if (includeRelated && typeof(Version).IsAssignableFrom(typeof(T)))
            {
                return await pgContext.Versions.AsNoTracking()
                    .Include(v => v.Books)
                    .ThenInclude(b => b.Chapters)
                    .AsSplitQuery()
                    .ToListAsync() as List<T>;
            }

            return await pgContext.GetDbSet<T>().AsNoTracking().ToListAsync() ?? [];
        }

        public async Task<T> Get<T>(string query, bool includeRelated = true) where T : class
        {
            if (includeRelated && typeof(Version).IsAssignableFrom(typeof(T)))
            {
                return await pgContext.Versions.AsNoTracking()
                    .Include(v => v.Books)
                    .ThenInclude(b => b.Chapters)
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(v => v.Id.Equals(query, StringComparison.OrdinalIgnoreCase)) as T;
            }

            return await pgContext.GetDbSet<T>().AsNoTracking().FirstOrDefaultAsync(e => EF.Property<string>(e, "Id") == query);
        }

        public async Task<T> Get<T>(long query) where T : class
        {
            if (typeof(User).IsAssignableFrom(typeof(T)))
            {
                return await pgContext.Users.AsNoTracking()
                    .Include(u => u.OptOutEntry)
                    .FirstOrDefaultAsync(u => u.Id == query) as T;
            }

            return await pgContext.GetDbSet<T>().AsNoTracking().FirstOrDefaultAsync(e => EF.Property<long>(e, "Id") == query);
        }

        public async Task<int> GetCount<T>() where T : class
        {
            return await pgContext.GetDbSet<T>().CountAsync();
        }

        public async Task<T> Create<T>(T t) where T : class
        {
            await pgContext.GetDbSet<T>().AddAsync(t);
            await pgContext.SaveChangesAsync();

            return t;
        }

        public async Task Remove<T>(T t) where T : class
        {
            pgContext.GetDbSet<T>().Remove(t);
            await pgContext.SaveChangesAsync();
        }

        public async Task Update<T>(long snowflake, UpdateDef<T> setters) where T : class, IPreference
        {
            await pgContext.GetDbSet<T>().Where(e => e.Id == snowflake).ExecuteUpdateAsync(setters);

            EntityEntry<T> entry = pgContext.ChangeTracker.Entries<T>().FirstOrDefault(e => e.Entity.Id == snowflake);
            if (entry != null) await entry.ReloadAsync();
        }

        public async Task Update(string abbreviation, UpdateDef<Version> updateDef)
        {
            await pgContext.Versions.Where(v => v.Id.Equals(abbreviation, StringComparison.OrdinalIgnoreCase)).ExecuteUpdateAsync(updateDef);

            EntityEntry<Version> entry = pgContext.ChangeTracker.Entries<Version>()
                .FirstOrDefault(e => e.Entity.Id.Equals(abbreviation, StringComparison.OrdinalIgnoreCase));
            if (entry != null) await entry.ReloadAsync();
        }

        public async Task Update(FrontendStats frontendStats, UpdateDef<FrontendStats> updateDef)
        {
            await pgContext.FrontendStats.Where(f => f.Id == frontendStats.Id).ExecuteUpdateAsync(updateDef);

            EntityEntry<FrontendStats> entry = pgContext.ChangeTracker.Entries<FrontendStats>().FirstOrDefault(e => e.Entity.Id == frontendStats.Id);
            if (entry != null) await entry.ReloadAsync();
        }

        public async Task Update(string experimentId, UpdateDef<Experiment> updateDef)
        {
            await pgContext.Experiments.Where(e => e.Id.Equals(experimentId, StringComparison.OrdinalIgnoreCase)).ExecuteUpdateAsync(updateDef);

            EntityEntry<Experiment> entry = pgContext.ChangeTracker.Entries<Experiment>().FirstOrDefault(e => e.Entity.Id == experimentId);
            if (entry != null) await entry.ReloadAsync();
        }
    }
}
