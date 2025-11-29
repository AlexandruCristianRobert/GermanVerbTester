using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GermanVerbTester.Migrations
{
    /// <inheritdoc />
    public partial class AddHintToVerb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Hint",
                table: "Verbs",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Hint",
                table: "Verbs");
        }
    }
}
