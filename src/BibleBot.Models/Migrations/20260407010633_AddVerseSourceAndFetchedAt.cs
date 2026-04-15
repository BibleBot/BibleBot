using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BibleBot.Models.Migrations
{
    /// <inheritdoc />
    public partial class AddVerseSourceAndFetchedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "fetched_at",
                table: "verses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "source",
                table: "verses",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fetched_at",
                table: "verses");

            migrationBuilder.DropColumn(
                name: "source",
                table: "verses");
        }
    }
}
