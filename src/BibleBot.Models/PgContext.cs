/*
 * Copyright (C) 2016-2026 Kerygma Digital Co.
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using Microsoft.EntityFrameworkCore;

namespace BibleBot.Models
{
    /// <summary>
    /// The unified database context
    /// </summary>
    /// <param name="options">The options to be used for the context.</param>
    public class PgContext(DbContextOptions<PgContext> options) : DbContext(options)
    {
        /// <summary>
        /// The users table.
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// The guilds table.
        /// </summary>
        public DbSet<Guild> Guilds { get; set; }

        /// <summary>
        /// The verse metrics table.
        /// </summary>
        public DbSet<VerseMetric> VerseMetrics { get; set; }

        /// <summary>
        /// The appended verses table.
        /// </summary>
        public DbSet<AppendedVerse> AppendedVerses { get; set; }

        /// <summary>
        /// The frontend statistics table.
        /// </summary>
        public DbSet<FrontendStats> FrontendStats { get; set; }

        /// <summary>
        /// The languages table.
        /// </summary>
        public DbSet<Language> Languages { get; set; }

        /// <summary>
        /// The Bible versions table.
        /// </summary>
        public DbSet<Version> Versions { get; set; }

        /// <summary>
        /// The Bible books table.
        /// </summary>
        public DbSet<Book> Books { get; set; }

        /// <summary>
        /// The Bible chapters table.
        /// </summary>
        public DbSet<Chapter> Chapters { get; set; }

        /// <summary>
        /// The Bible verses table.
        /// </summary>
        public DbSet<Verse> Verses { get; set; }

        /// <summary>
        /// The experiments table.
        /// </summary>
        public DbSet<Experiment> Experiments { get; set; }

        /// <summary>
        /// The opt-out users table.
        /// </summary>
        public DbSet<OptOutUser> OptOutUsers { get; set; }

        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Id).HasColumnName("id").ValueGeneratedNever();
                entity.Property(u => u.Version).HasColumnName("version");
                entity.Property(u => u.Language).HasColumnName("language");
                entity.Property(u => u.DisplayStyle).HasColumnName("display_style");
                entity.Property(u => u.InputMethod).HasColumnName("input_method");
                entity.Property(u => u.TitlesEnabled).HasColumnName("titles_enabled").IsRequired().HasDefaultValue(true);
                entity.Property(u => u.VerseNumbersEnabled).HasColumnName("verse_numbers_enabled").IsRequired().HasDefaultValue(true);
                entity.Property(u => u.PaginationEnabled).HasColumnName("pagination_enabled").IsRequired().HasDefaultValue(false);
                entity.Ignore(u => u.IsOptOut);
                entity.HasOne(u => u.OptOutEntry).WithOne().HasForeignKey<OptOutUser>(o => o.Id);
                entity.HasIndex(u => u.Version).HasDatabaseName("idx_users_version");
                entity.HasIndex(u => u.Language).HasDatabaseName("idx_users_language");
            });

            modelBuilder.Entity<Guild>(entity =>
            {
                entity.ToTable("guilds");
                entity.HasKey(g => g.Id);
                entity.Property(g => g.Id).HasColumnName("id").ValueGeneratedNever();
                entity.Property(g => g.Version).HasColumnName("version");
                entity.Property(g => g.Language).HasColumnName("language");
                entity.Property(g => g.DisplayStyle).HasColumnName("display_style");
                entity.Property(g => g.IgnoringBrackets).HasColumnName("ignoring_brackets");
                entity.Property(g => g.DailyVerseChannelId).HasColumnName("dv_channel_id");
                entity.Property(g => g.DailyVerseWebhook).HasColumnName("dv_webhook");
                entity.Property(g => g.DailyVerseTime).HasColumnName("dv_time");
                entity.Property(g => g.DailyVerseTimeZone).HasColumnName("dv_timezone");
                entity.Property(g => g.DailyVerseLastSentDate).HasColumnName("dv_last_sent");
                entity.Property(g => g.DailyVerseRoleId).HasColumnName("dv_role_id");
                entity.Property(g => g.DailyVerseIsThread).HasColumnName("dv_is_thread");
                entity.Property(g => g.DailyVerseLastStatusCode).HasColumnName("dv_last_status");
                entity.Property(g => g.IsDM).HasColumnName("is_dm").IsRequired();

                entity.HasIndex(g => new { g.DailyVerseTime, g.DailyVerseTimeZone })
                    .HasDatabaseName("idx_guilds_daily_verse")
                    .HasFilter("\"dv_webhook\" IS NOT NULL");
                entity.HasIndex(g => g.DailyVerseLastSentDate)
                    .HasDatabaseName("idx_guilds_dv_last_sent");
            });

            modelBuilder.Entity<VerseMetric>(entity =>
            {
                entity.ToTable("verse_metrics");
                entity.HasKey(v => v.Id);
                entity.Property(v => v.Id).HasColumnName("id");
                entity.Property(v => v.TimeGenerated).HasColumnName("time_generated").HasDefaultValueSql("now()");
                entity.Property(v => v.UserId).HasColumnName("user_id").IsRequired();
                entity.Property(v => v.GuildId).HasColumnName("guild_id").IsRequired();
                entity.Property(v => v.Book).HasColumnName("book").IsRequired();
                entity.Property(v => v.Chapter).HasColumnName("chapter").IsRequired();
                entity.Property(v => v.VerseRange).HasColumnName("verse_range").HasColumnType("int4range").IsRequired();
                entity.Property(v => v.Version).HasColumnName("version").IsRequired();
                entity.Property(v => v.Publisher).HasColumnName("publisher");
                entity.Property(v => v.IsOT).HasColumnName("is_ot").IsRequired();
                entity.Property(v => v.IsNT).HasColumnName("is_nt").IsRequired();
                entity.Property(v => v.IsDEU).HasColumnName("is_deu").IsRequired();

                entity.HasMany(v => v.AppendedVerses)
                    .WithOne(a => a.VerseMetric)
                    .HasForeignKey(a => a.VerseMetricId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(v => new { v.GuildId, v.Book, v.Chapter })
                    .HasDatabaseName("idx_verse_metrics_guild_book_chapter");
                entity.HasIndex(v => new { v.UserId, v.Book, v.Chapter })
                    .HasDatabaseName("idx_verse_metrics_user_book_chapter");
                entity.HasIndex(v => v.TimeGenerated)
                    .HasDatabaseName("idx_verse_metrics_time_generated")
                    .IsDescending();
            });

            modelBuilder.Entity<AppendedVerse>(entity =>
            {
                entity.ToTable("appended_verses");
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Id).HasColumnName("id");
                entity.Property(a => a.VerseMetricId).HasColumnName("verse_metric_id");
                entity.Property(a => a.VerseRange).HasColumnName("verse_range").HasColumnType("int4range");

                entity.HasIndex(a => a.VerseMetricId)
                    .HasDatabaseName("idx_appended_verse_metric_id");
            });

            modelBuilder.Entity<FrontendStats>(entity =>
            {
                entity.ToTable("frontend_stats");
                entity.HasKey(f => f.Id);
                entity.Property(f => f.Id).HasColumnName("id");
                entity.Property(f => f.FrontendRepoCommitHash).HasColumnName("commit_hash");
                entity.Property(f => f.ShardCount).HasColumnName("shard_count");
                entity.Property(f => f.ServerCount).HasColumnName("server_count");
                entity.Property(f => f.UserCount).HasColumnName("user_count");
                entity.Property(f => f.ChannelCount).HasColumnName("channel_count");
                entity.Property(f => f.UserInstallCount).HasColumnName("user_install_count");
            });

            modelBuilder.Entity<Language>(entity =>
            {
                entity.ToTable("languages");
                entity.HasKey(l => l.Id);
                entity.Property(l => l.Id).HasColumnName("id");
                entity.Property(l => l.Name).HasColumnName("name");
                entity.Property(l => l.DefaultVersion).HasColumnName("default_version");
            });

            modelBuilder.Entity<Version>(entity =>
            {
                entity.ToTable("versions");
                entity.HasKey(v => v.Id);
                entity.Property(v => v.Id).HasColumnName("id");
                entity.Property(v => v.IsActive).HasColumnName("is_active");
                entity.Property(v => v.AliasOfId).HasColumnName("alias_of_id");
                entity.Property(v => v.Name).HasColumnName("name");
                entity.Property(v => v.Source).HasColumnName("source");
                entity.Property(v => v.Publisher).HasColumnName("publisher");
                entity.Property(v => v.Locale).HasColumnName("locale");
                entity.Property(v => v.InternalId).HasColumnName("internal_id");
                entity.Property(v => v.SupportsOldTestament).HasColumnName("supports_ot");
                entity.Property(v => v.SupportsNewTestament).HasColumnName("supports_nt");
                entity.Property(v => v.SupportsDeuterocanon).HasColumnName("supports_deu");
                entity.Property(v => v.FollowsSeptuagintNumbering).HasColumnName("follows_septuagint");
            });

            modelBuilder.Entity<Book>(entity =>
            {
                entity.ToTable("books");
                entity.HasKey(v => v.Id);
                entity.Property(v => v.Id).HasColumnName("id");
                entity.Property(b => b.Name).HasColumnName("name");
                entity.Property(b => b.ProperName).HasColumnName("proper_name");
                entity.Property(b => b.InternalName).HasColumnName("internal_name");
                entity.Property(b => b.PreferredName).HasColumnName("preferred_name");
                entity.Property(b => b.VersionId).HasColumnName("version_id");
                entity.HasOne<Version>()
                    .WithMany(v => v.Books)
                    .HasForeignKey(b => b.VersionId);
                entity.HasIndex(b => new { b.VersionId, b.Name }).IsUnique();
            });

            modelBuilder.Entity<Chapter>(entity =>
            {
                entity.ToTable("chapters");
                entity.HasKey(v => v.Id);
                entity.Property(v => v.Id).HasColumnName("id");
                entity.Property(c => c.Number).HasColumnName("number");
                entity.Property(c => c.Titles).HasColumnName("titles").HasColumnType("jsonb");
                entity.Property(c => c.BookId).HasColumnName("book_id");
                entity.HasOne<Book>()
                    .WithMany(b => b.Chapters)
                    .HasForeignKey(c => c.BookId);
                entity.HasIndex(c => new { c.BookId, c.Number }).IsUnique();
            });

            modelBuilder.Entity<Verse>(entity =>
            {
                entity.ToTable("verses");
                entity.HasKey(v => v.Id);
                entity.Property(v => v.Id).HasColumnName("id");
                entity.Property(v => v.Number).HasColumnName("number");
                entity.Property(v => v.Content).HasColumnName("content");
                entity.Property(v => v.ChapterId).HasColumnName("chapter_id");
                entity.HasOne<Chapter>()
                    .WithMany(c => c.Verses)
                    .HasForeignKey(v => v.ChapterId);
            });

            modelBuilder.Entity<Experiment>(entity =>
            {
                entity.ToTable("experiments");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.Type).HasColumnName("type");
                entity.Property(e => e.Sphere).HasColumnName("sphere");
                entity.Property(e => e.Variants).HasColumnName("variants").HasColumnType("jsonb");
                entity.Property(e => e.Weights).HasColumnName("weights").HasColumnType("jsonb");
                entity.Property(e => e.Feedback).HasColumnName("feedback").HasColumnType("jsonb");
            });

            modelBuilder.Entity<OptOutUser>(entity =>
            {
                entity.ToTable("opt_out_users");
                entity.HasKey(o => o.Id);
                entity.Property(o => o.Id).HasColumnName("id").ValueGeneratedNever();
            });
        }

        /// <summary>
        /// Gets the DbSet for a given type.
        /// </summary>
        public DbSet<T> GetDbSet<T>() where T : class
        {
            Type type = typeof(T);
            return type.Name switch
            {
                nameof(User) => this.Users as DbSet<T>,
                nameof(Guild) => this.Guilds as DbSet<T>,
                nameof(Book) => this.Books as DbSet<T>,
                nameof(Chapter) => this.Chapters as DbSet<T>,
                nameof(Verse) => this.Verses as DbSet<T>,
                nameof(VerseMetric) => this.VerseMetrics as DbSet<T>,
                nameof(Models.FrontendStats) => this.FrontendStats as DbSet<T>,
                nameof(Language) => this.Languages as DbSet<T>,
                nameof(Version) => this.Versions as DbSet<T>,
                nameof(Experiment) => this.Experiments as DbSet<T>,
                _ => throw new ArgumentException($"Unknown DbSet type: {type.Name}")
            };
        }
    }
}
