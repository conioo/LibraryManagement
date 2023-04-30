using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    public partial class repair_v1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Copies_History_HistoryId",
                table: "Copies");

            migrationBuilder.DropForeignKey(
                name: "FK_Profiles_History_HistoryId",
                table: "Profiles");

            migrationBuilder.DropForeignKey(
                name: "FK_Rentals_History_HistoryId",
                table: "Rentals");

            migrationBuilder.DropForeignKey(
                name: "FK_Rentals_Profiles_ProfileLibraryCardNumber",
                table: "Rentals");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_History_HistoryId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Profiles_ProfileLibraryCardNumber",
                table: "Reservations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_History",
                table: "History");

            migrationBuilder.RenameTable(
                name: "History",
                newName: "Histories");

            migrationBuilder.AlterColumn<string>(
                name: "ProfileLibraryCardNumber",
                table: "Reservations",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ProfileLibraryCardNumber",
                table: "Rentals",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "HistoryId",
                table: "Profiles",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Histories",
                table: "Histories",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Copies_Histories_HistoryId",
                table: "Copies",
                column: "HistoryId",
                principalTable: "Histories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Profiles_Histories_HistoryId",
                table: "Profiles",
                column: "HistoryId",
                principalTable: "Histories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rentals_Histories_HistoryId",
                table: "Rentals",
                column: "HistoryId",
                principalTable: "Histories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Rentals_Profiles_ProfileLibraryCardNumber",
                table: "Rentals",
                column: "ProfileLibraryCardNumber",
                principalTable: "Profiles",
                principalColumn: "LibraryCardNumber");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Histories_HistoryId",
                table: "Reservations",
                column: "HistoryId",
                principalTable: "Histories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Profiles_ProfileLibraryCardNumber",
                table: "Reservations",
                column: "ProfileLibraryCardNumber",
                principalTable: "Profiles",
                principalColumn: "LibraryCardNumber");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Copies_Histories_HistoryId",
                table: "Copies");

            migrationBuilder.DropForeignKey(
                name: "FK_Profiles_Histories_HistoryId",
                table: "Profiles");

            migrationBuilder.DropForeignKey(
                name: "FK_Rentals_Histories_HistoryId",
                table: "Rentals");

            migrationBuilder.DropForeignKey(
                name: "FK_Rentals_Profiles_ProfileLibraryCardNumber",
                table: "Rentals");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Histories_HistoryId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Profiles_ProfileLibraryCardNumber",
                table: "Reservations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Histories",
                table: "Histories");

            migrationBuilder.RenameTable(
                name: "Histories",
                newName: "History");

            migrationBuilder.AlterColumn<string>(
                name: "ProfileLibraryCardNumber",
                table: "Reservations",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProfileLibraryCardNumber",
                table: "Rentals",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "HistoryId",
                table: "Profiles",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_History",
                table: "History",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Copies_History_HistoryId",
                table: "Copies",
                column: "HistoryId",
                principalTable: "History",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Profiles_History_HistoryId",
                table: "Profiles",
                column: "HistoryId",
                principalTable: "History",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Rentals_History_HistoryId",
                table: "Rentals",
                column: "HistoryId",
                principalTable: "History",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Rentals_Profiles_ProfileLibraryCardNumber",
                table: "Rentals",
                column: "ProfileLibraryCardNumber",
                principalTable: "Profiles",
                principalColumn: "LibraryCardNumber",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_History_HistoryId",
                table: "Reservations",
                column: "HistoryId",
                principalTable: "History",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Profiles_ProfileLibraryCardNumber",
                table: "Reservations",
                column: "ProfileLibraryCardNumber",
                principalTable: "Profiles",
                principalColumn: "LibraryCardNumber",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
