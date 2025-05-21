/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
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
using MongoDB.Driver;
using Version = BibleBot.Models.Version;

namespace BibleBot.Backend.Services
{
    public class VersionService(MongoService mongoService)
    {
        private List<Version> _versions = [];

        private async Task<List<Version>> GetVersions(bool forcePull = false)
        {
            if (forcePull || _versions.Count == 0)
            {
                _versions = await mongoService.Get<Version>();
            }

            return _versions;
        }

        public async Task<List<Version>> Get() => await GetVersions();
        public async Task<Version> Get(string abbreviation) => (await GetVersions()).FirstOrDefault(version => string.Equals(version.Abbreviation, abbreviation, StringComparison.OrdinalIgnoreCase));
        public async Task<int> GetCount() => (await GetVersions()).Count;

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
            Version createdVersion = await mongoService.Create(version);
            await GetVersions(true);

            return createdVersion;
        }

        public async Task Update(string abbreviation, UpdateDefinition<Version> updateDefinition)
        {
            Version beforeVersion = await Get(abbreviation);
            await mongoService.Update(abbreviation, updateDefinition);

            Version afterVersion = await mongoService.Get<Version>(abbreviation);

            _versions.Remove(beforeVersion);
            _versions.Add(afterVersion);
        }
        public async Task Remove(Version idealVersion)
        {
            await mongoService.Remove(idealVersion);
            await GetVersions(true);
        }
    }
}
