using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CompHomp.Migrations
{
    /// <inheritdoc />
    public partial class AddLoginAttempts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginAttempt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LoginAttempts",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Builds",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 3, 12, 17, 8, 47, 560, DateTimeKind.Local).AddTicks(9016));

            migrationBuilder.UpdateData(
                table: "Builds",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 3, 12, 17, 8, 47, 560, DateTimeKind.Local).AddTicks(9029));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "LastLoginAttempt", "LoginAttempts", "Password", "RegistrationDate" },
                values: new object[] { null, 0, "$2a$11$1dLEv8PbmR.j13vY9EW8fOmAX7BghvdugUFDuOkKr6rF2lYFDVbZW", new DateTime(2025, 3, 12, 17, 8, 47, 429, DateTimeKind.Local).AddTicks(7789) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "LastLoginAttempt", "LoginAttempts", "Password", "RegistrationDate" },
                values: new object[] { null, 0, "$2a$11$GxmJiKnYbpAFWbRtUQDS9O8Tb5i2m2u8ndc9O1cXRB2K93pcjqQh.", new DateTime(2025, 3, 12, 17, 8, 47, 560, DateTimeKind.Local).AddTicks(6466) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastLoginAttempt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LoginAttempts",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Builds",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2024, 12, 18, 9, 6, 10, 632, DateTimeKind.Local).AddTicks(7071));

            migrationBuilder.UpdateData(
                table: "Builds",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2024, 12, 18, 9, 6, 10, 632, DateTimeKind.Local).AddTicks(7077));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Password", "RegistrationDate" },
                values: new object[] { "$2a$11$1EKhZVNxtvouxmTWITrITuoWF6TNyLwECC72.6o/iVntODU8TU4gS", new DateTime(2024, 12, 18, 9, 6, 10, 521, DateTimeKind.Local).AddTicks(3545) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Password", "RegistrationDate" },
                values: new object[] { "$2a$11$y8O0m9t.g8qkMUGsfEq0J.eQpzBwEh4/MoM2TyEG0QA0uG5dvnl4O", new DateTime(2024, 12, 18, 9, 6, 10, 632, DateTimeKind.Local).AddTicks(6295) });
        }
    }
}
