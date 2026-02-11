using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SyncMigration2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AlterColumn<DateTime>(
            //    name: "CreatedAtUtc",
            //    table: "SavedMovies",
            //    type: "datetime2",
            //    nullable: false,
            //    defaultValueSql: "SYSUTCDATETIME()",
            //    oldClrType: typeof(DateTime),
            //    oldType: "datetime2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AlterColumn<DateTime>(
            //    name: "CreatedAtUtc",
            //    table: "SavedMovies",
            //    type: "datetime2",
            //    nullable: false,
            //    oldClrType: typeof(DateTime),
            //    oldType: "datetime2",
            //    oldDefaultValueSql: "SYSUTCDATETIME()");
        }
    }
}
