using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cuttr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class locationName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LocationName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocationName",
                table: "Users");
        }
    }
}
