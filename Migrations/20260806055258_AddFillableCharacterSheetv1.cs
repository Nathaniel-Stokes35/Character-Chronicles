using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CharacterChronicles.Migrations
{
    /// <inheritdoc />
    public partial class AddFillableCharacterSheetv1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Alignment",
                table: "Characters",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ArmorClass",
                table: "Characters",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Background",
                table: "Characters",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentHitPoints",
                table: "Characters",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Equipment",
                table: "Characters",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FeaturesAndTraits",
                table: "Characters",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HitPointMaximum",
                table: "Characters",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Alignment",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "ArmorClass",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "Background",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "CurrentHitPoints",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "Equipment",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "FeaturesAndTraits",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "HitPointMaximum",
                table: "Characters");
        }
    }
}
