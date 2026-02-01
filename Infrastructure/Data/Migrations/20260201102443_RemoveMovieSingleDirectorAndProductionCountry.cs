using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMovieSingleDirectorAndProductionCountry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Movies_Countries_ProductionCountryCode",
                table: "Movies");

            migrationBuilder.DropForeignKey(
                name: "FK_Movies_People_DirectorId",
                table: "Movies");

            migrationBuilder.DropIndex(
                name: "IX_Movies_DirectorId",
                table: "Movies");

            migrationBuilder.DropIndex(
                name: "IX_Movies_ProductionCountryCode",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "DirectorId",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "ProductionCountryCode",
                table: "Movies");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DirectorId",
                table: "Movies",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductionCountryCode",
                table: "Movies",
                type: "nchar(2)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Movies_DirectorId",
                table: "Movies",
                column: "DirectorId");

            migrationBuilder.CreateIndex(
                name: "IX_Movies_ProductionCountryCode",
                table: "Movies",
                column: "ProductionCountryCode");

            migrationBuilder.AddForeignKey(
                name: "FK_Movies_Countries_ProductionCountryCode",
                table: "Movies",
                column: "ProductionCountryCode",
                principalTable: "Countries",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Movies_People_DirectorId",
                table: "Movies",
                column: "DirectorId",
                principalTable: "People",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
