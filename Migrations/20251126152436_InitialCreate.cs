using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GermanVerbTester.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TestResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CorrectAnswers = table.Column<int>(type: "int", nullable: false),
                    TotalQuestions = table.Column<int>(type: "int", nullable: false),
                    SubmissionDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Verbs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    German = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    English = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Verbs", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Verbs",
                columns: new[] { "Id", "English", "German" },
                values: new object[,]
                {
                    { 1, "to be", "sein" },
                    { 2, "to have", "haben" },
                    { 3, "to become", "werden" },
                    { 4, "to be able to", "können" },
                    { 5, "to have to", "müssen" },
                    { 6, "to say", "sagen" },
                    { 7, "to do", "machen" },
                    { 8, "to give", "geben" },
                    { 9, "to come", "kommen" },
                    { 10, "should", "sollen" },
                    { 11, "to want", "wollen" },
                    { 12, "to go", "gehen" },
                    { 13, "to know", "wissen" },
                    { 14, "to see", "sehen" },
                    { 15, "to let", "lassen" },
                    { 16, "to stand", "stehen" },
                    { 17, "to find", "finden" },
                    { 18, "to stay", "bleiben" },
                    { 19, "to lie", "liegen" },
                    { 20, "to be called", "heißen" },
                    { 21, "to think", "denken" },
                    { 22, "to take", "nehmen" },
                    { 23, "to do", "tun" },
                    { 24, "to be allowed to", "dürfen" },
                    { 25, "to believe", "glauben" },
                    { 26, "to hold", "halten" },
                    { 27, "to name", "nennen" },
                    { 28, "to like", "mögen" },
                    { 29, "to show", "zeigen" },
                    { 30, "to lead", "führen" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestResults");

            migrationBuilder.DropTable(
                name: "Verbs");
        }
    }
}
