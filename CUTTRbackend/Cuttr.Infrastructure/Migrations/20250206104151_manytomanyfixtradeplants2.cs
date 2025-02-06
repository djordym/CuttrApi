using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cuttr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class manytomanyfixtradeplants2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlantIdsProposedByUser1",
                table: "TradeProposals");

            migrationBuilder.DropColumn(
                name: "PlantIdsProposedByUser2",
                table: "TradeProposals");

            migrationBuilder.DropColumn(
                name: "MatchId",
                table: "Messages");

            migrationBuilder.CreateTable(
                name: "TradeProposalPlants",
                columns: table => new
                {
                    TradeProposalId = table.Column<int>(type: "int", nullable: false),
                    PlantId = table.Column<int>(type: "int", nullable: false),
                    IsProposedByUser1 = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradeProposalPlants", x => new { x.TradeProposalId, x.PlantId });
                    table.ForeignKey(
                        name: "FK_TradeProposalPlants_Plants_PlantId",
                        column: x => x.PlantId,
                        principalTable: "Plants",
                        principalColumn: "PlantId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TradeProposalPlants_TradeProposals_TradeProposalId",
                        column: x => x.TradeProposalId,
                        principalTable: "TradeProposals",
                        principalColumn: "TradeProposalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TradeProposalPlants_PlantId",
                table: "TradeProposalPlants",
                column: "PlantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TradeProposalPlants");

            migrationBuilder.AddColumn<string>(
                name: "PlantIdsProposedByUser1",
                table: "TradeProposals",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PlantIdsProposedByUser2",
                table: "TradeProposals",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MatchId",
                table: "Messages",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
