using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOneToManyRelationBack : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Movies_Countries_CountryCode",
                table: "Movies");

            migrationBuilder.DropForeignKey(
                name: "FK_Movies_People_PersonId",
                table: "Movies");

            migrationBuilder.RenameColumn(
                name: "PersonId",
                table: "Movies",
                newName: "DirectorId");

            migrationBuilder.RenameColumn(
                name: "CountryCode",
                table: "Movies",
                newName: "ProductionCountryCode");

            migrationBuilder.RenameIndex(
                name: "IX_Movies_PersonId",
                table: "Movies",
                newName: "IX_Movies_DirectorId");

            migrationBuilder.RenameIndex(
                name: "IX_Movies_CountryCode",
                table: "Movies",
                newName: "IX_Movies_ProductionCountryCode");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Movies_Countries_ProductionCountryCode",
                table: "Movies");

            migrationBuilder.DropForeignKey(
                name: "FK_Movies_People_DirectorId",
                table: "Movies");

            migrationBuilder.RenameColumn(
                name: "ProductionCountryCode",
                table: "Movies",
                newName: "CountryCode");

            migrationBuilder.RenameColumn(
                name: "DirectorId",
                table: "Movies",
                newName: "PersonId");

            migrationBuilder.RenameIndex(
                name: "IX_Movies_ProductionCountryCode",
                table: "Movies",
                newName: "IX_Movies_CountryCode");

            migrationBuilder.RenameIndex(
                name: "IX_Movies_DirectorId",
                table: "Movies",
                newName: "IX_Movies_PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_MovieGenres_MovieId_GenreId",
                table: "MovieGenres",
                columns: new[] { "MovieId", "GenreId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovieDirectors_MovieId_DirectorId",
                table: "MovieDirectors",
                columns: new[] { "MovieId", "DirectorId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Movies_Countries_CountryCode",
                table: "Movies",
                column: "CountryCode",
                principalTable: "Countries",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_Movies_People_PersonId",
                table: "Movies",
                column: "PersonId",
                principalTable: "People",
                principalColumn: "Id");
        }
    }
}
