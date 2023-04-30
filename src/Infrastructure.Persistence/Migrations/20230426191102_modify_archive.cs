using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    public partial class modify_archive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Copies_Histories_HistoryId",
                table: "Copies");

            migrationBuilder.DropForeignKey(
                name: "FK_Copies_Rentals_LastRentalId",
                table: "Copies");

            migrationBuilder.DropForeignKey(
                name: "FK_Copies_Reservations_LastReservationId",
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

            migrationBuilder.DropTable(
                name: "Histories");

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
                name: "IsReturned",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "HistoryId",
                table: "Profiles");

            migrationBuilder.RenameColumn(
                name: "EndTime",
                table: "Reservations",
                newName: "EndDate");

            migrationBuilder.RenameColumn(
                name: "BeginTime",
                table: "Reservations",
                newName: "BeginDate");

            migrationBuilder.RenameColumn(
                name: "EndTime",
                table: "Rentals",
                newName: "EndDate");

            migrationBuilder.RenameColumn(
                name: "BeginTime",
                table: "Rentals",
                newName: "BeginDate");

            migrationBuilder.RenameColumn(
                name: "LastReservationId",
                table: "Copies",
                newName: "CurrentReservationId");

            migrationBuilder.RenameColumn(
                name: "LastRentalId",
                table: "Copies",
                newName: "CurrentRentalId");

            migrationBuilder.RenameColumn(
                name: "HistoryId",
                table: "Copies",
                newName: "CopyHistoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Copies_HistoryId",
                table: "Copies",
                newName: "IX_Copies_CopyHistoryId");

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

            migrationBuilder.CreateTable(
                name: "CopyHistories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CopyHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProfilesHistories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfilesHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArchivalRentals",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BeginDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReturnedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PenaltyCharge = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ProfileLibraryCardNumber = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ProfileHistoryId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CopyInventoryNumber = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CopyHistoryId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivalRentals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivalRentals_Copies_CopyInventoryNumber",
                        column: x => x.CopyInventoryNumber,
                        principalTable: "Copies",
                        principalColumn: "InventoryNumber");
                    table.ForeignKey(
                        name: "FK_ArchivalRentals_CopyHistories_CopyHistoryId",
                        column: x => x.CopyHistoryId,
                        principalTable: "CopyHistories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ArchivalRentals_Profiles_ProfileLibraryCardNumber",
                        column: x => x.ProfileLibraryCardNumber,
                        principalTable: "Profiles",
                        principalColumn: "LibraryCardNumber");
                    table.ForeignKey(
                        name: "FK_ArchivalRentals_ProfilesHistories_ProfileHistoryId",
                        column: x => x.ProfileHistoryId,
                        principalTable: "ProfilesHistories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ArchivalReservations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BeginDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CollectionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProfileLibraryCardNumber = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ProfileHistoryId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CopyInventoryNumber = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CopyHistoryId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivalReservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivalReservations_Copies_CopyInventoryNumber",
                        column: x => x.CopyInventoryNumber,
                        principalTable: "Copies",
                        principalColumn: "InventoryNumber");
                    table.ForeignKey(
                        name: "FK_ArchivalReservations_CopyHistories_CopyHistoryId",
                        column: x => x.CopyHistoryId,
                        principalTable: "CopyHistories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ArchivalReservations_Profiles_ProfileLibraryCardNumber",
                        column: x => x.ProfileLibraryCardNumber,
                        principalTable: "Profiles",
                        principalColumn: "LibraryCardNumber");
                    table.ForeignKey(
                        name: "FK_ArchivalReservations_ProfilesHistories_ProfileHistoryId",
                        column: x => x.ProfileHistoryId,
                        principalTable: "ProfilesHistories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Copies_CurrentRentalId",
                table: "Copies",
                column: "CurrentRentalId",
                unique: true,
                filter: "[CurrentRentalId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Copies_CurrentReservationId",
                table: "Copies",
                column: "CurrentReservationId",
                unique: true,
                filter: "[CurrentReservationId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivalRentals_CopyHistoryId",
                table: "ArchivalRentals",
                column: "CopyHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivalRentals_CopyInventoryNumber",
                table: "ArchivalRentals",
                column: "CopyInventoryNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivalRentals_ProfileHistoryId",
                table: "ArchivalRentals",
                column: "ProfileHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivalRentals_ProfileLibraryCardNumber",
                table: "ArchivalRentals",
                column: "ProfileLibraryCardNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivalReservations_CopyHistoryId",
                table: "ArchivalReservations",
                column: "CopyHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivalReservations_CopyInventoryNumber",
                table: "ArchivalReservations",
                column: "CopyInventoryNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivalReservations_ProfileHistoryId",
                table: "ArchivalReservations",
                column: "ProfileHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivalReservations_ProfileLibraryCardNumber",
                table: "ArchivalReservations",
                column: "ProfileLibraryCardNumber");

            migrationBuilder.AddForeignKey(
                name: "FK_Copies_CopyHistories_CopyHistoryId",
                table: "Copies",
                column: "CopyHistoryId",
                principalTable: "CopyHistories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Copies_Rentals_CurrentRentalId",
                table: "Copies",
                column: "CurrentRentalId",
                principalTable: "Rentals",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Copies_Reservations_CurrentReservationId",
                table: "Copies",
                column: "CurrentReservationId",
                principalTable: "Reservations",
                principalColumn: "Id");

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
                name: "FK_Copies_CopyHistories_CopyHistoryId",
                table: "Copies");

            migrationBuilder.DropForeignKey(
                name: "FK_Copies_Rentals_CurrentRentalId",
                table: "Copies");

            migrationBuilder.DropForeignKey(
                name: "FK_Copies_Reservations_CurrentReservationId",
                table: "Copies");

            migrationBuilder.DropForeignKey(
                name: "FK_Rentals_Profiles_ProfileLibraryCardNumber",
                table: "Rentals");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Profiles_ProfileLibraryCardNumber",
                table: "Reservations");

            migrationBuilder.DropTable(
                name: "ArchivalRentals");

            migrationBuilder.DropTable(
                name: "ArchivalReservations");

            migrationBuilder.DropTable(
                name: "CopyHistories");

            migrationBuilder.DropTable(
                name: "ProfilesHistories");

            migrationBuilder.DropIndex(
                name: "IX_Copies_CurrentRentalId",
                table: "Copies");

            migrationBuilder.DropIndex(
                name: "IX_Copies_CurrentReservationId",
                table: "Copies");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "Reservations",
                newName: "EndTime");

            migrationBuilder.RenameColumn(
                name: "BeginDate",
                table: "Reservations",
                newName: "BeginTime");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "Rentals",
                newName: "EndTime");

            migrationBuilder.RenameColumn(
                name: "BeginDate",
                table: "Rentals",
                newName: "BeginTime");

            migrationBuilder.RenameColumn(
                name: "CurrentReservationId",
                table: "Copies",
                newName: "LastReservationId");

            migrationBuilder.RenameColumn(
                name: "CurrentRentalId",
                table: "Copies",
                newName: "LastRentalId");

            migrationBuilder.RenameColumn(
                name: "CopyHistoryId",
                table: "Copies",
                newName: "HistoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Copies_CopyHistoryId",
                table: "Copies",
                newName: "IX_Copies_HistoryId");

            migrationBuilder.AlterColumn<string>(
                name: "ProfileLibraryCardNumber",
                table: "Reservations",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

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
                name: "ProfileLibraryCardNumber",
                table: "Rentals",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "HistoryId",
                table: "Rentals",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsReturned",
                table: "Rentals",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "HistoryId",
                table: "Profiles",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Histories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Histories", x => x.Id);
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
                name: "FK_Copies_Histories_HistoryId",
                table: "Copies",
                column: "HistoryId",
                principalTable: "Histories",
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
    }
}
