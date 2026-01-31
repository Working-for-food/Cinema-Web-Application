using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class MovieTmdb : Migration
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

            migrationBuilder.DropIndex(
                name: "IX_MovieGenres_MovieId_GenreId",
                table: "MovieGenres");

            migrationBuilder.DropIndex(
                name: "IX_MovieDirectors_MovieId_DirectorId",
                table: "MovieDirectors");

            migrationBuilder.DropIndex(
                name: "IX_MovieActors_MovieId_ActorId",
                table: "MovieActors");

            migrationBuilder.DropColumn(
                name: "ProductionCountryCode",
                table: "Movies");

            migrationBuilder.RenameColumn(
                name: "OriginalName",
                table: "Movies",
                newName: "OriginalTitle");

            migrationBuilder.RenameColumn(
                name: "DirectorId",
                table: "Movies",
                newName: "TmdbId");

            migrationBuilder.AddColumn<string>(
                name: "BackdropPath",
                table: "Movies",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "Movies",
                type: "nchar(2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PersonId",
                table: "Movies",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PosterPath",
                table: "Movies",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Genres",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<int>(
                name: "TmdbId",
                table: "Genres",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Movies_CountryCode",
                table: "Movies",
                column: "CountryCode");

            migrationBuilder.CreateIndex(
                name: "IX_Movies_PersonId",
                table: "Movies",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Movies_TmdbId",
                table: "Movies",
                column: "TmdbId",
                unique: true,
                filter: "[TmdbId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Genres_TmdbId",
                table: "Genres",
                column: "TmdbId",
                unique: true,
                filter: "[TmdbId] IS NOT NULL");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Movies_Countries_CountryCode",
                table: "Movies");

            migrationBuilder.DropForeignKey(
                name: "FK_Movies_People_PersonId",
                table: "Movies");

            migrationBuilder.DropIndex(
                name: "IX_Movies_CountryCode",
                table: "Movies");

            migrationBuilder.DropIndex(
                name: "IX_Movies_PersonId",
                table: "Movies");

            migrationBuilder.DropIndex(
                name: "IX_Movies_TmdbId",
                table: "Movies");

            migrationBuilder.DropIndex(
                name: "IX_Genres_TmdbId",
                table: "Genres");

            migrationBuilder.DropColumn(
                name: "BackdropPath",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "PersonId",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "PosterPath",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "TmdbId",
                table: "Genres");

            migrationBuilder.RenameColumn(
                name: "TmdbId",
                table: "Movies",
                newName: "DirectorId");

            migrationBuilder.RenameColumn(
                name: "OriginalTitle",
                table: "Movies",
                newName: "OriginalName");

            migrationBuilder.AddColumn<string>(
                name: "ProductionCountryCode",
                table: "Movies",
                type: "nchar(2)",
                fixedLength: true,
                maxLength: 2,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Genres",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(120)",
                oldMaxLength: 120);

            migrationBuilder.CreateIndex(
                name: "IX_Movies_DirectorId",
                table: "Movies",
                column: "DirectorId");

            migrationBuilder.CreateIndex(
                name: "IX_Movies_ProductionCountryCode",
                table: "Movies",
                column: "ProductionCountryCode");

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

            migrationBuilder.CreateIndex(
                name: "IX_MovieActors_MovieId_ActorId",
                table: "MovieActors",
                columns: new[] { "MovieId", "ActorId" },
                unique: true);

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
