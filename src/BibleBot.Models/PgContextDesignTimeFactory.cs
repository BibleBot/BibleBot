/*
 * Copyright (C) 2016-2026 Kerygma Digital Co.
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql;

namespace BibleBot.Models
{
    /// <summary>
    /// Design-time factory for <see cref="PgContext"/>, used by EF Core tooling (migrations, etc.).
    /// Reads the connection string from the POSTGRES_CONN environment variable.
    /// </summary>
    public class PgContextDesignTimeFactory : IDesignTimeDbContextFactory<PgContext>
    {
        /// <inheritdoc/>
        public PgContext CreateDbContext(string[] args)
        {
            string connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONN")
                ?? "Host=localhost;Database=biblebot;Username=postgres;Password=postgres";

            NpgsqlDataSourceBuilder dataSourceBuilder = new(connectionString);
            dataSourceBuilder.UseNodaTime();
            dataSourceBuilder.EnableDynamicJson();
            NpgsqlDataSource dataSource = dataSourceBuilder.Build();

            DbContextOptionsBuilder<PgContext> optionsBuilder = new();
            optionsBuilder.UseNpgsql(dataSource, o => o.UseNodaTime());

            return new PgContext(optionsBuilder.Options);
        }
    }
}
