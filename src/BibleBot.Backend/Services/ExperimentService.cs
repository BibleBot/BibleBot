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
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BibleBot.Models;
using MongoDB.Driver;

namespace BibleBot.Backend.Services
{
    public class ExperimentService(MongoService mongoService)
    {
        private List<Experiment> _experiments = [];

        public async Task<List<Experiment>> GetExperiments(bool forcePull = false)
        {
            if (forcePull || _experiments.Count == 0)
            {
                _experiments = await mongoService.Get<Experiment>();
            }

            return _experiments;
        }

        public async Task<List<Experiment>> Get() => await GetExperiments();
        public async Task<Experiment> Get(string name) => (await GetExperiments()).FirstOrDefault(experiment => string.Equals(experiment.Name, name, StringComparison.OrdinalIgnoreCase));

        public async Task<Experiment> Create(Experiment experiment)
        {
            Experiment createdExperiment = await mongoService.Create(experiment);
            await GetExperiments(true);

            return createdExperiment;
        }

        public async Task Update(string name, UpdateDefinition<Experiment> updateDefinition)
        {
            Experiment beforeExperiment = await Get(name);
            await mongoService.Update(name, updateDefinition);

            Experiment afterExperiment = await mongoService.Get<Experiment>(name);

            _experiments.Remove(beforeExperiment);
            _experiments.Add(afterExperiment);
        }

        public async Task Remove(Experiment experiment)
        {
            await mongoService.Remove(experiment);
            await GetExperiments(true);
        }

        public async Task<Dictionary<Experiment, string>> GetFrontendExperimentVariantsForUser(string userId) => await GetExperimentVariantsForId(userId, isUser: true, frontendOnly: true);

        public async Task<Dictionary<Experiment, string>> GetFrontendExperimentVariantsForGuild(string guildId) => await GetExperimentVariantsForId(guildId, isUser: false, frontendOnly: true);

        public async Task<Dictionary<Experiment, string>> GetAutoServExperimentVariantsForGuild(string guildId) => await GetExperimentVariantsForId(guildId, isUser: false, autoServOnly: true);

        public async Task<Dictionary<Experiment, string>> GetExperimentVariantsForUser(string userId) => await GetExperimentVariantsForId(userId, isUser: true);

        public async Task<Dictionary<Experiment, string>> GetExperimentVariantsForGuild(string guildId) => await GetExperimentVariantsForId(guildId, isUser: false);

        public async Task<Dictionary<Experiment, string>> GetExperimentVariantsForId(string id, bool isUser, bool frontendOnly = false, bool backendOnly = false, bool autoServOnly = false) => !ulong.TryParse(id, out ulong idInt) ? [] : await GetExperimentVariantsForId(idInt, isUser, frontendOnly, backendOnly, autoServOnly);

        public async Task<Dictionary<Experiment, string>> GetExperimentVariantsForId(ulong id, bool isUser, bool frontendOnly = false, bool backendOnly = false, bool autoServOnly = false)
        {
            Dictionary<Experiment, string> variants = [];

            foreach (Experiment experiment in await GetExperiments())
            {
                if (frontendOnly && experiment.Type is not "Frontend" and not "Universal")
                {
                    continue;
                }

                if (backendOnly && experiment.Type is not "Backend" and not "Universal")
                {
                    continue;
                }

                if (autoServOnly && experiment.Type is not "AutoServ" and not "Universal")
                {
                    continue;
                }

                if (isUser && experiment.Sphere is not "User" and not "Universal")
                {
                    continue;
                }

                byte[] hashInput = Encoding.UTF8.GetBytes($"{id}:{experiment.Name}");
                byte[] hashBytes = MD5.HashData(hashInput);

                // Python's int(hexdigest, 16) treats the hash as a big-endian number.
                // C#'s BigInteger(byte[]) expects a little-endian number.
                // So we reverse the bytes to match the interpretation.
                Array.Reverse(hashBytes);

                // Append a 0 byte to the end (which is the MSB in little-endian) to ensure the number is positive.
                // BigInteger is signed, and we want to treat the hash as unsigned.
                byte[] positiveHashBytes = new byte[hashBytes.Length + 1];
                Array.Copy(hashBytes, positiveHashBytes, hashBytes.Length);

                BigInteger hashVal = new(positiveHashBytes);
                BigInteger normalizedVal = hashVal % 100;

                int currentWeight = 0;
                string assignedVariant = experiment.Variants[^1]; // Fallback to last variant

                if (experiment.Weights != null && experiment.Variants != null && experiment.Weights.Count == experiment.Variants.Count)
                {
                    for (int i = 0; i < experiment.Weights.Count; i++)
                    {
                        currentWeight += experiment.Weights[i];
                        if (normalizedVal < currentWeight)
                        {
                            assignedVariant = experiment.Variants[i];
                            break;
                        }
                    }
                }

                variants[experiment] = assignedVariant;
            }

            return variants;
        }
    }
}
