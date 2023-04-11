using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    public partial class fixed_names : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rentals_Profiles_ProfilLibraryCardNumber",
                table: "Rentals");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Profiles_ProfilLibraryCardNumber",
                table: "Reservations");

            migrationBuilder.RenameColumn(
                name: "ProfilLibraryCardNumber",
                table: "Reservations",
                newName: "ProfileLibraryCardNumber");

            migrationBuilder.RenameIndex(
                name: "IX_Reservations_ProfilLibraryCardNumber",
                table: "Reservations",
                newName: "IX_Reservations_ProfileLibraryCardNumber");

            migrationBuilder.RenameColumn(
                name: "ProfilLibraryCardNumber",
                table: "Rentals",
                newName: "ProfileLibraryCardNumber");

            migrationBuilder.RenameIndex(
                name: "IX_Rentals_ProfilLibraryCardNumber",
                table: "Rentals",
                newName: "IX_Rentals_ProfileLibraryCardNumber");

            migrationBuilder.AddForeignKey(
                name: "FK_Rentals_Profiles_ProfileLibraryCardNumber",
                table: "Rentals",
                column: "ProfileLibraryCardNumber",
                principalTable: "Profiles",
                principalColumn: "LibraryCardNumber",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Profiles_ProfileLibraryCardNumber",
                table: "Reservations",
                column: "ProfileLibraryCardNumber",
                principalTable: "Profiles",
                principalColumn: "LibraryCardNumber",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rentals_Profiles_ProfileLibraryCardNumber",
                table: "Rentals");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Profiles_ProfileLibraryCardNumber",
                table: "Reservations");

            migrationBuilder.RenameColumn(
                name: "ProfileLibraryCardNumber",
                table: "Reservations",
                newName: "ProfilLibraryCardNumber");

            migrationBuilder.RenameIndex(
                name: "IX_Reservations_ProfileLibraryCardNumber",
                table: "Reservations",
                newName: "IX_Reservations_ProfilLibraryCardNumber");

            migrationBuilder.RenameColumn(
                name: "ProfileLibraryCardNumber",
                table: "Rentals",
                newName: "ProfilLibraryCardNumber");

            migrationBuilder.RenameIndex(
                name: "IX_Rentals_ProfileLibraryCardNumber",
                table: "Rentals",
                newName: "IX_Rentals_ProfilLibraryCardNumber");

            migrationBuilder.AddForeignKey(
                name: "FK_Rentals_Profiles_ProfilLibraryCardNumber",
                table: "Rentals",
                column: "ProfilLibraryCardNumber",
                principalTable: "Profiles",
                principalColumn: "LibraryCardNumber",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Profiles_ProfilLibraryCardNumber",
                table: "Reservations",
                column: "ProfilLibraryCardNumber",
                principalTable: "Profiles",
                principalColumn: "LibraryCardNumber",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
