using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cuttr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class uitbreidingtradeproposals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "OwnerCompletionConfirmed",
                table: "TradeProposals",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ProposalOwnerUserId",
                table: "TradeProposals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ResponderCompletionConfirmed",
                table: "TradeProposals",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerCompletionConfirmed",
                table: "TradeProposals");

            migrationBuilder.DropColumn(
                name: "ProposalOwnerUserId",
                table: "TradeProposals");

            migrationBuilder.DropColumn(
                name: "ResponderCompletionConfirmed",
                table: "TradeProposals");
        }
    }
}
