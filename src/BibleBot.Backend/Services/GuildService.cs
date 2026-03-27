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
using System.Threading;
using System.Threading.Tasks;
using BibleBot.Models;
using Serilog;

namespace BibleBot.Backend.Services
{
    public class GuildService(PreferenceService preferenceService)
    {
        public async Task<List<Guild>> Get() => await preferenceService.Get<Guild>();
        public async Task<Guild> Get(long guildId) => await preferenceService.Get<Guild>(guildId);
        public async Task<int> GetCount() => await preferenceService.GetCount<Guild>();
        public async Task<Guild> Create(Guild guild) => await preferenceService.Create(guild);
        public async Task Update(long guildId, UpdateDef<Guild> updateDef) => await preferenceService.Update(guildId, updateDef);

        public async Task Remove(Guild idealGuild) => await preferenceService.Remove(idealGuild);
    }
}
