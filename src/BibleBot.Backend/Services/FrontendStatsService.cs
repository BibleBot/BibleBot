/*
* Copyright (C) 2016-2026 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;
using System.Threading.Tasks;
using BibleBot.Models;

namespace BibleBot.Backend.Services
{
    public class FrontendStatsService(PostgresService postgresService)
    {
        public async Task<List<FrontendStats>> Get() => await postgresService.Get<FrontendStats>();
        public async Task<FrontendStats> Create(FrontendStats frontendStats) => await postgresService.Create(frontendStats);
        public async Task Update(FrontendStats frontendStats, UpdateDef<FrontendStats> updateDef) => await postgresService.Update(frontendStats, updateDef);
    }
}
