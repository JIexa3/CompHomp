using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CompHomp.Migrations
{
    /// <inheritdoc />
    public partial class AddDeletedUserRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 999999);

            migrationBuilder.UpdateData(
                table: "Builds",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 5, 29, 16, 45, 23, 68, DateTimeKind.Local).AddTicks(5026));

            migrationBuilder.UpdateData(
                table: "Builds",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 5, 29, 16, 45, 23, 68, DateTimeKind.Local).AddTicks(5034));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Password", "RegistrationDate" },
                values: new object[] { "$2a$11$1Oy8P1jsO3GpLi6omSFgcuE6OaXVH3jV.wTAq9nK05GrCOhkNVOz.", new DateTime(2025, 5, 29, 16, 45, 22, 944, DateTimeKind.Local).AddTicks(5263) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Password", "RegistrationDate" },
                values: new object[] { "$2a$11$LGEgQFhVYVlbLkijXQqJeunswKEQB1fo7YobEOgZUPLc7/6IPyCWS", new DateTime(2025, 5, 29, 16, 45, 23, 68, DateTimeKind.Local).AddTicks(3935) });
        }
    }
}
