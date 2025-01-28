using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cuttr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class version21 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Matches_MatchId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_MatchId",
                table: "Messages");

            migrationBuilder.AddColumn<bool>(
                name: "IsTraded",
                table: "Plants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ConnectionId",
                table: "Messages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MatchEFMatchId",
                table: "Messages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Connections",
                columns: table => new
                {
                    ConnectionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId1 = table.Column<int>(type: "int", nullable: false),
                    UserId2 = table.Column<int>(type: "int", nullable: false),
                    isActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Connections", x => x.ConnectionId);
                    table.ForeignKey(
                        name: "FK_Connections_Users_UserId1",
                        column: x => x.UserId1,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Connections_Users_UserId2",
                        column: x => x.UserId2,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TradeProposals",
                columns: table => new
                {
                    TradeProposalId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConnectionId = table.Column<int>(type: "int", nullable: false),
                    PlantIdsProposedByUser1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlantIdsProposedByUser2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TradeProposalStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeclinedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradeProposals", x => x.TradeProposalId);
                    table.ForeignKey(
                        name: "FK_TradeProposals_Connections_ConnectionId",
                        column: x => x.ConnectionId,
                        principalTable: "Connections",
                        principalColumn: "ConnectionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ConnectionId",
                table: "Messages",
                column: "ConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_MatchEFMatchId",
                table: "Messages",
                column: "MatchEFMatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Connections_UserId1",
                table: "Connections",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_Connections_UserId2",
                table: "Connections",
                column: "UserId2");

            migrationBuilder.CreateIndex(
                name: "IX_TradeProposals_ConnectionId",
                table: "TradeProposals",
                column: "ConnectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Connections_ConnectionId",
                table: "Messages",
                column: "ConnectionId",
                principalTable: "Connections",
                principalColumn: "ConnectionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Matches_MatchEFMatchId",
                table: "Messages",
                column: "MatchEFMatchId",
                principalTable: "Matches",
                principalColumn: "MatchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Connections_ConnectionId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Matches_MatchEFMatchId",
                table: "Messages");

            migrationBuilder.DropTable(
                name: "TradeProposals");

            migrationBuilder.DropTable(
                name: "Connections");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ConnectionId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_MatchEFMatchId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "IsTraded",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "ConnectionId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "MatchEFMatchId",
                table: "Messages");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_MatchId",
                table: "Messages",
                column: "MatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Matches_MatchId",
                table: "Messages",
                column: "MatchId",
                principalTable: "Matches",
                principalColumn: "MatchId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
