using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CandidateManagementSystem.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckConstraintsForNonEmptyFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Skills_Name",
                table: "Skills",
                sql: "btrim(\"Name\") <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Candidates_ContactNumber",
                table: "Candidates",
                sql: "btrim(\"ContactNumber\") <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Candidates_Email",
                table: "Candidates",
                sql: "btrim(\"Email\") <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Candidates_FullName",
                table: "Candidates",
                sql: "btrim(\"FullName\") <> ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Skills_Name",
                table: "Skills");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Candidates_ContactNumber",
                table: "Candidates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Candidates_Email",
                table: "Candidates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Candidates_FullName",
                table: "Candidates");
        }
    }
}
