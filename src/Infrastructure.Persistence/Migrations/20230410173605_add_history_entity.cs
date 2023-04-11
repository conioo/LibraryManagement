using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    public partial class add_history_entity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rentals_Copies_CopyInventoryNumber",
                table: "Rentals");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Copies_CopyInventoryNumber",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_CopyInventoryNumber",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Rentals_CopyInventoryNumber",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "CopyInventoryNumber",
                table: "Reservations");

            migrationBuilder.RenameColumn(
                name: "Returned",
                table: "Rentals",
                newName: "IsReturned");

            migrationBuilder.AddColumn<string>(
                name: "HistoryId",
                table: "Reservations",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Received",
                table: "Reservations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "CopyInventoryNumber",
                table: "Rentals",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "HistoryId",
                table: "Rentals",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PenaltyCharge",
                table: "Rentals",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HistoryId",
                table: "Profiles",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Profiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "HistoryId",
                table: "Copies",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastRentalId",
                table: "Copies",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastReservationId",
                table: "Copies",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "History",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_History", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_HistoryId",
                table: "Reservations",
                column: "HistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Rentals_HistoryId",
                table: "Rentals",
                column: "HistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_HistoryId",
                table: "Profiles",
                column: "HistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Copies_HistoryId",
                table: "Copies",
                column: "HistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Copies_LastRentalId",
                table: "Copies",
                column: "LastRentalId",
                unique: true,
                filter: "[LastRentalId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Copies_LastReservationId",
                table: "Copies",
                column: "LastReservationId",
                unique: true,
                filter: "[LastReservationId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Copies_History_HistoryId",
                table: "Copies",
                column: "HistoryId",
                principalTable: "History",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Copies_Rentals_LastRentalId",
                table: "Copies",
                column: "LastRentalId",
                principalTable: "Rentals",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Copies_Reservations_LastReservationId",
                table: "Copies",
                column: "LastReservationId",
                principalTable: "Reservations",
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
                name: "FK_Reservations_History_HistoryId",
                table: "Reservations",
                column: "HistoryId",
                principalTable: "History",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Copies_History_HistoryId",
                table: "Copies");

            migrationBuilder.DropForeignKey(
                name: "FK_Copies_Rentals_LastRentalId",
                table: "Copies");

            migrationBuilder.DropForeignKey(
                name: "FK_Copies_Reservations_LastReservationId",
                table: "Copies");

            migrationBuilder.DropForeignKey(
                name: "FK_Profiles_History_HistoryId",
                table: "Profiles");

            migrationBuilder.DropForeignKey(
                name: "FK_Rentals_History_HistoryId",
                table: "Rentals");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_History_HistoryId",
                table: "Reservations");

            migrationBuilder.DropTable(
                name: "History");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_HistoryId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Rentals_HistoryId",
                table: "Rentals");

            migrationBuilder.DropIndex(
                name: "IX_Profiles_HistoryId",
                table: "Profiles");

            migrationBuilder.DropIndex(
                name: "IX_Copies_HistoryId",
                table: "Copies");

            migrationBuilder.DropIndex(
                name: "IX_Copies_LastRentalId",
                table: "Copies");

            migrationBuilder.DropIndex(
                name: "IX_Copies_LastReservationId",
                table: "Copies");

            migrationBuilder.DropColumn(
                name: "HistoryId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "Received",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "HistoryId",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "PenaltyCharge",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "HistoryId",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "HistoryId",
                table: "Copies");

            migrationBuilder.DropColumn(
                name: "LastRentalId",
                table: "Copies");

            migrationBuilder.DropColumn(
                name: "LastReservationId",
                table: "Copies");

            migrationBuilder.RenameColumn(
                name: "IsReturned",
                table: "Rentals",
                newName: "Returned");

            migrationBuilder.AddColumn<string>(
                name: "CopyInventoryNumber",
                table: "Reservations",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "CopyInventoryNumber",
                table: "Rentals",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_CopyInventoryNumber",
                table: "Reservations",
                column: "CopyInventoryNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Rentals_CopyInventoryNumber",
                table: "Rentals",
                column: "CopyInventoryNumber");

            migrationBuilder.AddForeignKey(
                name: "FK_Rentals_Copies_CopyInventoryNumber",
                table: "Rentals",
                column: "CopyInventoryNumber",
                principalTable: "Copies",
                principalColumn: "InventoryNumber",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Copies_CopyInventoryNumber",
                table: "Reservations",
                column: "CopyInventoryNumber",
                principalTable: "Copies",
                principalColumn: "InventoryNumber",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
