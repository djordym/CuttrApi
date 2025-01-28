using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cuttr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class somechanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Users_UserId1",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Users_UserId2",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Matches_MatchEFMatchId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_MatchEFMatchId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Matches_UserId1",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "MatchEFMatchId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Matches");

            migrationBuilder.RenameColumn(
                name: "UserId2",
                table: "Matches",
                newName: "ConnectionId");

            migrationBuilder.RenameIndex(
                name: "IX_Matches_UserId2",
                table: "Matches",
                newName: "IX_Matches_ConnectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Connections_ConnectionId",
                table: "Matches",
                column: "ConnectionId",
                principalTable: "Connections",
                principalColumn: "ConnectionId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Connections_ConnectionId",
                table: "Matches");

            migrationBuilder.RenameColumn(
                name: "ConnectionId",
                table: "Matches",
                newName: "UserId2");

            migrationBuilder.RenameIndex(
                name: "IX_Matches_ConnectionId",
                table: "Matches",
                newName: "IX_Matches_UserId2");

            migrationBuilder.AddColumn<int>(
                name: "MatchEFMatchId",
                table: "Messages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "Matches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_MatchEFMatchId",
                table: "Messages",
                column: "MatchEFMatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_UserId1",
                table: "Matches",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Users_UserId1",
                table: "Matches",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Users_UserId2",
                table: "Matches",
                column: "UserId2",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Matches_MatchEFMatchId",
                table: "Messages",
                column: "MatchEFMatchId",
                principalTable: "Matches",
                principalColumn: "MatchId");
        }
    }
}
