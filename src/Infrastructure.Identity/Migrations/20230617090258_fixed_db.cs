using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Identity.Migrations
{
    /// <inheritdoc />
    public partial class fixed_db : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "36764551-535a-4191-b24c-9c30573d0789");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "912a8366-ba9f-44ac-9946-7d58952adc4f");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "9d97b81e-474a-4a85-b254-a8fadfd9186b");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "051a944a-9862-4fc6-a2e5-714c13585122", "5ddf24b8-bf01-4dee-8f4f-da9bee5cc813" });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "5ddf24b8-bf01-4dee-8f4f-da9bee5cc813");

            migrationBuilder.AddColumn<string>(
                name: "ProfileCardNumber",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "051a944a-9862-4fc6-a2e5-714c13585122",
                column: "ConcurrencyStamp",
                value: null);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "310f5096-7d9b-481e-bd2a-35dd81f3dad1", null, "basic", "BASIC" },
                    { "4fdddc9d-b8ae-41be-885a-3562d22c9686", null, "worker", "WORKER" },
                    { "d5d73e32-e188-4b87-a033-bab580c12092", null, "moderator", "MODERATOR" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Created", "CreatedBy", "Email", "EmailConfirmed", "FirstName", "LastModified", "LastModifiedBy", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "ProfileCardNumber", "RefreshTokenId", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "6f4e62ab-e678-4956-a173-c581d447f1e9", 0, "e99561de-0831-41b9-bf8e-2906bddcf47a", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "defaultadmin@gmail.com", true, "Admin", null, null, "Admin", false, null, "DEFAULTADMIN@GMAIL.COM", "ADMIN", "AQAAAAIAAYagAAAAEAS5HuLAu2g6vWf3rMD2//OAfv3M2DqoPDSp7vH9iEYDBTM9f0IPRMhjypTBB8Mwsw==", null, false, null, null, "f7ccacea-4756-403f-8523-a848c0580f60", false, "Admin" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "051a944a-9862-4fc6-a2e5-714c13585122", "6f4e62ab-e678-4956-a173-c581d447f1e9" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "310f5096-7d9b-481e-bd2a-35dd81f3dad1");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "4fdddc9d-b8ae-41be-885a-3562d22c9686");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d5d73e32-e188-4b87-a033-bab580c12092");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "051a944a-9862-4fc6-a2e5-714c13585122", "6f4e62ab-e678-4956-a173-c581d447f1e9" });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "6f4e62ab-e678-4956-a173-c581d447f1e9");

            migrationBuilder.DropColumn(
                name: "ProfileCardNumber",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "051a944a-9862-4fc6-a2e5-714c13585122",
                column: "ConcurrencyStamp",
                value: "6d7af46e-cf3a-42b6-b0a8-b15e757e01de");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "36764551-535a-4191-b24c-9c30573d0789", "527cf310-6fdc-4859-8190-d92105d96df8", "worker", "WORKER" },
                    { "912a8366-ba9f-44ac-9946-7d58952adc4f", "492f8306-28e7-4470-8e65-d5e877e92c9c", "moderator", "MODERATOR" },
                    { "9d97b81e-474a-4a85-b254-a8fadfd9186b", "bb06cd17-4de3-4694-9691-10faab9e44d5", "basic", "BASIC" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Created", "CreatedBy", "Email", "EmailConfirmed", "FirstName", "LastModified", "LastModifiedBy", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "RefreshTokenId", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "5ddf24b8-bf01-4dee-8f4f-da9bee5cc813", 0, "de34f790-8602-473a-baae-6539f6eb291d", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "defaultadmin@gmail.com", true, "Admin", null, null, "Admin", false, null, "DEFAULTADMIN@GMAIL.COM", "ADMIN", "AQAAAAEAACcQAAAAEMbp6sTHfcsj0qHUxDxHLTF8pX6Vo5kXwSK4JxNV4XpaBRZYF22a4+EVfVDiQWvbRA==", null, false, null, "eca0327e-6d30-4d43-a47c-96b163600f51", false, "Admin" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "051a944a-9862-4fc6-a2e5-714c13585122", "5ddf24b8-bf01-4dee-8f4f-da9bee5cc813" });
        }
    }
}
