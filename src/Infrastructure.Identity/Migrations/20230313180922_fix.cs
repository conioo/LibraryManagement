using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Identity.Migrations
{
    public partial class fix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1badf17a-9eba-46e9-ba4f-34e2051916a0");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "7400581f-c5aa-4ef2-85ce-9f95412d2204");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b0438d03-8ff2-4cb9-b079-46b54502020f");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "d62426d6-427f-4710-aa43-a8328965ee0e");

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

        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "051a944a-9862-4fc6-a2e5-714c13585122",
                column: "ConcurrencyStamp",
                value: "cd7678d4-de32-49b2-8281-c5339edc227d");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "1badf17a-9eba-46e9-ba4f-34e2051916a0", "996b5ebf-519a-4a49-ac32-e15b692c96ac", "worker", "WORKER" },
                    { "7400581f-c5aa-4ef2-85ce-9f95412d2204", "9a8fa676-09ae-4285-8bfe-e9ba913b35fd", "moderator", "MODERATOR" },
                    { "b0438d03-8ff2-4cb9-b079-46b54502020f", "40f357a0-9194-417a-8e37-92b30a379d19", "basic", "BASIC" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Created", "CreatedBy", "Email", "EmailConfirmed", "FirstName", "LastModified", "LastModifiedBy", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "RefreshTokenId", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "d62426d6-427f-4710-aa43-a8328965ee0e", 0, "505637ca-19ba-40b4-a477-7ae594f9664e", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "defaultadmin@gmail.com", true, "Admin", null, null, "Admin", false, null, "DEFAULTADMIN@GMAIL.COM", "ADMIN", "AQAAAAEAACcQAAAAEEPmnq8zheRhEpOOBlCNF8rC/my6UfaNmYzv4A4iD2pXkSZ74V3AVCIvjkwCekuDZg==", null, false, null, "bab35243-50b0-42b4-9527-601e903b9ccd", false, "Admin" });
        }
    }
}
