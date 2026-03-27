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
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BibleBot.Models;

using Microsoft.Extensions.DependencyInjection;
using System.Threading;

namespace BibleBot.Backend.Services
{
    public class ExperimentService(IServiceScopeFactory scopeFactory)
    {
        private List<Experiment> _experiments = [];
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public async Task<List<Experiment>> GetExperiments(bool forcePull = false)
        {
            if (!forcePull && _experiments.Count != 0)
            {
                return [.. _experiments];
            }

            await _semaphore.WaitAsync();
            try
            {
                if (!forcePull && _experiments.Count != 0)
                {
                    return [.. _experiments];
                }

                using IServiceScope scope = scopeFactory.CreateScope();
                PostgresService postgresService = scope.ServiceProvider.GetRequiredService<PostgresService>();
                _experiments = await postgresService.Get<Experiment>();
            }
            finally
            {
                _semaphore.Release();
            }

            return [.. _experiments];
        }

        public async Task<List<Experiment>> Get() => await GetExperiments();
        public async Task<Experiment> Get(string id) => (await GetExperiments()).FirstOrDefault(experiment => string.Equals(experiment.Id, id, StringComparison.OrdinalIgnoreCase));

        public async Task<Experiment> Create(Experiment experiment)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            PostgresService postgresService = scope.ServiceProvider.GetRequiredService<PostgresService>();
            Experiment createdExperiment = await postgresService.Create(experiment);
            await GetExperiments(true);

            return createdExperiment;
        }

        public async Task Update(string name, UpdateDef<Experiment> updateDef)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            PostgresService postgresService = scope.ServiceProvider.GetRequiredService<PostgresService>();

            Experiment beforeExperiment = await Get(name);
            await postgresService.Update(name, updateDef);

            Experiment afterExperiment = await postgresService.Get<Experiment>(name);

            await _semaphore.WaitAsync();
            try
            {
                _experiments.Remove(beforeExperiment);
                _experiments.Add(afterExperiment);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task Helps(string experimentId, long userId, bool helped)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            PostgresService postgresService = scope.ServiceProvider.GetRequiredService<PostgresService>();

            Experiment exp = (await GetExperiments()).FirstOrDefault(experiment =>
                string.Equals(experiment.Id, experimentId, StringComparison.OrdinalIgnoreCase));

            if (exp != null)
            {
                if (helped)
                {
                    exp.Feedback.Helped.Add(userId);
                }
                else
                {
                    exp.Feedback.DidNotHelp.Add(userId);
                }

                await this.Update(experimentId, UpdateDef<Experiment>.Set(experiment => experiment.Feedback, exp.Feedback));
            }
        }

        public async Task Helped(string experimentId, long userId) => await Helps(experimentId, userId, true);

        public async Task DidNotHelp(string experimentId, long userId) => await Helps(experimentId, userId, false);

        public async Task Remove(Experiment experiment)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            PostgresService postgresService = scope.ServiceProvider.GetRequiredService<PostgresService>();

            await postgresService.Remove(experiment);
            await GetExperiments(true);
        }

        public async Task<Dictionary<Experiment, string>> GetFrontendExperimentVariantsForUser(long userId) => await GetExperimentVariantsForId(userId, isUser: true, frontendOnly: true);

        public async Task<Dictionary<Experiment, string>> GetFrontendExperimentVariantsForGuild(long guildId) => await GetExperimentVariantsForId(guildId, isUser: false, frontendOnly: true);

        public async Task<Dictionary<Experiment, string>> GetAutoServExperimentVariantsForGuild(long guildId) => await GetExperimentVariantsForId(guildId, isUser: false, autoServOnly: true);

        public async Task<Dictionary<Experiment, string>> GetExperimentVariantsForUser(long userId) => await GetExperimentVariantsForId(userId, isUser: true);

        public async Task<Dictionary<Experiment, string>> GetExperimentVariantsForGuild(long guildId) => await GetExperimentVariantsForId(guildId, isUser: false);

        private async Task<Dictionary<Experiment, string>> GetExperimentVariantsForId(long id, bool isUser, bool frontendOnly = false, bool backendOnly = false, bool autoServOnly = false)
        {
            Dictionary<Experiment, string> variants = [];
            List<Experiment> experiments = await GetExperiments();

            foreach (Experiment experiment in experiments)
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

                byte[] hashInput = Encoding.UTF8.GetBytes($"{id}:{experiment.Id}");
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
