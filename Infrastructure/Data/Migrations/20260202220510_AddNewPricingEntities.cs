using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNewPricingEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PricingTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    HallId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SessionCategoryMultipliers",
                columns: table => new
                {
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Multiplier = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionCategoryMultipliers", x => new { x.SessionId, x.Category });
                    table.ForeignKey(
                        name: "FK_SessionCategoryMultipliers_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SessionRowPrices",
                columns: table => new
                {
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    Row = table.Column<int>(type: "int", nullable: false),
                    BasePrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionRowPrices", x => new { x.SessionId, x.Row });
                    table.ForeignKey(
                        name: "FK_SessionRowPrices_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PricingTemplateCategoryMultipliers",
                columns: table => new
                {
                    PricingTemplateId = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Multiplier = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingTemplateCategoryMultipliers", x => new { x.PricingTemplateId, x.Category });
                    table.ForeignKey(
                        name: "FK_PricingTemplateCategoryMultipliers_PricingTemplates_PricingTemplateId",
                        column: x => x.PricingTemplateId,
                        principalTable: "PricingTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PricingTemplateRowPrices",
                columns: table => new
                {
                    PricingTemplateId = table.Column<int>(type: "int", nullable: false),
                    Row = table.Column<int>(type: "int", nullable: false),
                    BasePrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingTemplateRowPrices", x => new { x.PricingTemplateId, x.Row });
                    table.ForeignKey(
                        name: "FK_PricingTemplateRowPrices_PricingTemplates_PricingTemplateId",
                        column: x => x.PricingTemplateId,
                        principalTable: "PricingTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PricingTemplateCategoryMultipliers");

            migrationBuilder.DropTable(
                name: "PricingTemplateRowPrices");

            migrationBuilder.DropTable(
                name: "SessionCategoryMultipliers");

            migrationBuilder.DropTable(
                name: "SessionRowPrices");

            migrationBuilder.DropTable(
                name: "PricingTemplates");
        }
    }
}
