/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
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
    public class LanguageService(MongoService mongoService)
    {
        private readonly MongoService _mongoService = mongoService;

        public async Task<List<Language>> Get() => await _mongoService.Get<Language>();
        public async Task<Language> Get(string culture) => await _mongoService.Get<Language>(culture);

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
    }
}
