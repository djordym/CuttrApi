using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cuttr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class plantconstraintremoved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_MatchEF_PlantIdOrder",
                table: "Matches");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_MatchEF_PlantIdOrder",
                table: "Matches",
                sql: "[PlantId1] < [PlantId2]");
        }
    }
}
