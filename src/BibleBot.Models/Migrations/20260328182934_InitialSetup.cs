using System;
using System.Collections.Generic;
using BibleBot.Models;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using NpgsqlTypes;

#nullable disable

namespace BibleBot.Models.Migrations
{
    /// <inheritdoc />
    public partial class InitialSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "experiments",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    variants = table.Column<string>(type: "jsonb", nullable: true),
                    weights = table.Column<string>(type: "jsonb", nullable: true),
                    type = table.Column<string>(type: "text", nullable: true),
                    sphere = table.Column<string>(type: "text", nullable: true),
                    feedback = table.Column<ExperimentFeedback>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_experiments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "frontend_stats",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    shard_count = table.Column<int>(type: "integer", nullable: false),
                    server_count = table.Column<int>(type: "integer", nullable: false),
                    user_count = table.Column<int>(type: "integer", nullable: false),
                    channel_count = table.Column<int>(type: "integer", nullable: false),
                    user_install_count = table.Column<int>(type: "integer", nullable: false),
                    commit_hash = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_frontend_stats", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "guilds",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    language = table.Column<string>(type: "text", nullable: true),
                    version = table.Column<string>(type: "text", nullable: true),
                    display_style = table.Column<string>(type: "text", nullable: true),
                    ignoring_brackets = table.Column<string>(type: "text", nullable: true),
                    dv_channel_id = table.Column<long>(type: "bigint", nullable: false),
                    dv_webhook = table.Column<string>(type: "text", nullable: true),
                    dv_time = table.Column<string>(type: "text", nullable: true),
                    dv_timezone = table.Column<string>(type: "text", nullable: true),
                    dv_last_sent = table.Column<string>(type: "text", nullable: true),
                    dv_role_id = table.Column<long>(type: "bigint", nullable: false),
                    dv_is_thread = table.Column<bool>(type: "boolean", nullable: false),
                    dv_last_status = table.Column<int>(type: "integer", nullable: false),
                    is_dm = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guilds", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "languages",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    default_version = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_languages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    input_method = table.Column<string>(type: "text", nullable: true),
                    language = table.Column<string>(type: "text", nullable: true),
                    version = table.Column<string>(type: "text", nullable: true),
                    titles_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    verse_numbers_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    pagination_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    display_style = table.Column<string>(type: "text", nullable: true),
                    IsOptOut = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "verse_metrics",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    time_generated = table.Column<Instant>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    guild_id = table.Column<long>(type: "bigint", nullable: false),
                    book = table.Column<string>(type: "text", nullable: false),
                    chapter = table.Column<int>(type: "integer", nullable: false),
                    verse_range = table.Column<NpgsqlRange<int>>(type: "int4range", nullable: false),
                    version = table.Column<string>(type: "text", nullable: false),
                    publisher = table.Column<string>(type: "text", nullable: true),
                    is_ot = table.Column<bool>(type: "boolean", nullable: false),
                    is_nt = table.Column<bool>(type: "boolean", nullable: false),
                    is_deu = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_verse_metrics", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "versions",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    alias_of_id = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "text", nullable: true),
                    source = table.Column<string>(type: "text", nullable: true),
                    publisher = table.Column<string>(type: "text", nullable: true),
                    locale = table.Column<string>(type: "text", nullable: true),
                    internal_id = table.Column<string>(type: "text", nullable: true),
                    supports_ot = table.Column<bool>(type: "boolean", nullable: false),
                    supports_nt = table.Column<bool>(type: "boolean", nullable: false),
                    supports_deu = table.Column<bool>(type: "boolean", nullable: false),
                    follows_septuagint = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_versions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "appended_verses",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    verse_metric_id = table.Column<int>(type: "integer", nullable: false),
                    verse_range = table.Column<NpgsqlRange<int>>(type: "int4range", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appended_verses", x => x.id);
                    table.ForeignKey(
                        name: "FK_appended_verses_verse_metrics_verse_metric_id",
                        column: x => x.verse_metric_id,
                        principalTable: "verse_metrics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "books",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    version_id = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "text", nullable: true),
                    proper_name = table.Column<string>(type: "text", nullable: true),
                    internal_name = table.Column<string>(type: "text", nullable: true),
                    preferred_name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_books", x => x.id);
                    table.ForeignKey(
                        name: "FK_books_versions_version_id",
                        column: x => x.version_id,
                        principalTable: "versions",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "chapters",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    book_id = table.Column<int>(type: "integer", nullable: false),
                    number = table.Column<int>(type: "integer", nullable: false),
                    titles = table.Column<List<Tuple<int, int, string>>>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chapters", x => x.id);
                    table.ForeignKey(
                        name: "FK_chapters_books_book_id",
                        column: x => x.book_id,
                        principalTable: "books",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "verses",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    chapter_id = table.Column<int>(type: "integer", nullable: false),
                    number = table.Column<int>(type: "integer", nullable: false),
                    content = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_verses", x => x.id);
                    table.ForeignKey(
                        name: "FK_verses_chapters_chapter_id",
                        column: x => x.chapter_id,
                        principalTable: "chapters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_appended_verse_metric_id",
                table: "appended_verses",
                column: "verse_metric_id");

            migrationBuilder.CreateIndex(
                name: "IX_books_version_id_name",
                table: "books",
                columns: new[] { "version_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_chapters_book_id_number",
                table: "chapters",
                columns: new[] { "book_id", "number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_guilds_daily_verse",
                table: "guilds",
                columns: new[] { "dv_time", "dv_timezone" },
                filter: "\"dv_webhook\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "idx_guilds_dv_last_sent",
                table: "guilds",
                column: "dv_last_sent");

            migrationBuilder.CreateIndex(
                name: "idx_users_language",
                table: "users",
                column: "language");

            migrationBuilder.CreateIndex(
                name: "idx_users_version",
                table: "users",
                column: "version");

            migrationBuilder.CreateIndex(
                name: "idx_verse_metrics_guild_book_chapter",
                table: "verse_metrics",
                columns: new[] { "guild_id", "book", "chapter" });

            migrationBuilder.CreateIndex(
                name: "idx_verse_metrics_time_generated",
                table: "verse_metrics",
                column: "time_generated",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_verse_metrics_user_book_chapter",
                table: "verse_metrics",
                columns: new[] { "user_id", "book", "chapter" });

            migrationBuilder.CreateIndex(
                name: "IX_verses_chapter_id",
                table: "verses",
                column: "chapter_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "appended_verses");

            migrationBuilder.DropTable(
                name: "experiments");

            migrationBuilder.DropTable(
                name: "frontend_stats");

            migrationBuilder.DropTable(
                name: "guilds");

            migrationBuilder.DropTable(
                name: "languages");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "verses");

            migrationBuilder.DropTable(
                name: "verse_metrics");

            migrationBuilder.DropTable(
                name: "chapters");

            migrationBuilder.DropTable(
                name: "books");

            migrationBuilder.DropTable(
                name: "versions");
        }
    }
}
