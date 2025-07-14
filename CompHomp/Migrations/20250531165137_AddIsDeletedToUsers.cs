using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CompHomp.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDeletedToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 999999);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Builds",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 5, 31, 20, 51, 37, 328, DateTimeKind.Local).AddTicks(1948));

            migrationBuilder.UpdateData(
                table: "Builds",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 5, 31, 20, 51, 37, 328, DateTimeKind.Local).AddTicks(1957));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "IsDeleted", "Password", "RegistrationDate" },
                values: new object[] { false, "$2a$11$4v3luqnMzmQgb14q83FgYu.z542vvrJ/wN2G31dw1fRq0GXdnZGry", new DateTime(2025, 5, 31, 20, 51, 37, 205, DateTimeKind.Local).AddTicks(703) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "IsDeleted", "Password", "RegistrationDate" },
                values: new object[] { false, "$2a$11$M37ZjvYoOH7QPgg97tUXtuK/5qf3PGSkgCmEzjz/FcAEM/utzBPIa", new DateTime(2025, 5, 31, 20, 51, 37, 328, DateTimeKind.Local).AddTicks(89) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Builds",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 5, 29, 17, 9, 22, 367, DateTimeKind.Local).AddTicks(1045));

            migrationBuilder.UpdateData(
                table: "Builds",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 5, 29, 17, 9, 22, 367, DateTimeKind.Local).AddTicks(1053));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Password", "RegistrationDate" },
                values: new object[] { "$2a$11$bUlMi57GlD/PUeRughmNCeCKwMcOBzkS/0YqfieNKyJPAl9eejJya", new DateTime(2025, 5, 29, 17, 9, 22, 242, DateTimeKind.Local).AddTicks(9488) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Password", "RegistrationDate" },
                values: new object[] { "$2a$11$AVb8HaXyea6D2oUPSgPsNOU8t7mGRw.oPAIQTHQc23xqVvUVcwkjC", new DateTime(2025, 5, 29, 17, 9, 22, 366, DateTimeKind.Local).AddTicks(9091) });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "IsActive", "IsBlocked", "LastLoginAttempt", "Login", "LoginAttempts", "Password", "RegistrationDate", "Role" },
                values: new object[] { 999999, "deleted@user", false, true, null, "DELETED_USER", 0, "$2a$11$R54hp/LlxPsC1Qxj9tai3eKFdSHM9G8/U.t1//clJ7QBy9D5wRwyu", new DateTime(2025, 5, 29, 17, 9, 22, 118, DateTimeKind.Local).AddTicks(5938), 0 });
        }
    }
}
