using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    public partial class add_missing_relation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfileHistoryId",
                table: "Profiles",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_ProfileHistoryId",
                table: "Profiles",
                column: "ProfileHistoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Profiles_ProfilesHistories_ProfileHistoryId",
                table: "Profiles",
                column: "ProfileHistoryId",
                principalTable: "ProfilesHistories",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Profiles_ProfilesHistories_ProfileHistoryId",
                table: "Profiles");

            migrationBuilder.DropIndex(
                name: "IX_Profiles_ProfileHistoryId",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "ProfileHistoryId",
                table: "Profiles");
        }
    }
}
