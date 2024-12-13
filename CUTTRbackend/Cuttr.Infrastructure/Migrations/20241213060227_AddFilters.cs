using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cuttr.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFilters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_LocationLatitude_LocationLongitude",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LocationLatitude",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LocationLongitude",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Plants");

            migrationBuilder.RenameColumn(
                name: "PreferredCategories",
                table: "UserPreferences",
                newName: "PreferedWateringNeed");

            migrationBuilder.RenameColumn(
                name: "CareRequirements",
                table: "Plants",
                newName: "Extras");

            migrationBuilder.AlterColumn<int>(
                name: "SearchRadius",
                table: "UserPreferences",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AddColumn<string>(
                name: "PreferedExtras",
                table: "UserPreferences",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PreferedIndoorOutdoor",
                table: "UserPreferences",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PreferedLightRequirement",
                table: "UserPreferences",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PreferedPetFriendly",
                table: "UserPreferences",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PreferedPlantCategory",
                table: "UserPreferences",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PreferedPlantStage",
                table: "UserPreferences",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PreferedPropagationEase",
                table: "UserPreferences",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PreferedSize",
                table: "UserPreferences",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IndoorOutdoor",
                table: "Plants",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LightRequirement",
                table: "Plants",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PetFriendly",
                table: "Plants",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PlantCategory",
                table: "Plants",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PlantStage",
                table: "Plants",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PropagationEase",
                table: "Plants",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "Plants",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WateringNeed",
                table: "Plants",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreferedExtras",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "PreferedIndoorOutdoor",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "PreferedLightRequirement",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "PreferedPetFriendly",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "PreferedPlantCategory",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "PreferedPlantStage",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "PreferedPropagationEase",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "PreferedSize",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "IndoorOutdoor",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "LightRequirement",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "PetFriendly",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "PlantCategory",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "PlantStage",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "PropagationEase",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "WateringNeed",
                table: "Plants");

            migrationBuilder.RenameColumn(
                name: "PreferedWateringNeed",
                table: "UserPreferences",
                newName: "PreferredCategories");

            migrationBuilder.RenameColumn(
                name: "Extras",
                table: "Plants",
                newName: "CareRequirements");

            migrationBuilder.AddColumn<double>(
                name: "LocationLatitude",
                table: "Users",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LocationLongitude",
                table: "Users",
                type: "float",
                nullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "SearchRadius",
                table: "UserPreferences",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Plants",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Users_LocationLatitude_LocationLongitude",
                table: "Users",
                columns: new[] { "LocationLatitude", "LocationLongitude" });
        }
    }
}
