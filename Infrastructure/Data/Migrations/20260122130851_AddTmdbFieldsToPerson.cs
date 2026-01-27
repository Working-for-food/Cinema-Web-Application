using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTmdbFieldsToPerson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "People",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TmdbId",
                table: "People",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "TmdbLastSyncAt",
                table: "People",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_People_TmdbId",
                table: "People",
                column: "TmdbId",
                unique: true,
                filter: "[TmdbId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_People_TmdbId",
                table: "People");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "People");

            migrationBuilder.DropColumn(
                name: "TmdbId",
                table: "People");

            migrationBuilder.DropColumn(
                name: "TmdbLastSyncAt",
                table: "People");
        }
    }
}
