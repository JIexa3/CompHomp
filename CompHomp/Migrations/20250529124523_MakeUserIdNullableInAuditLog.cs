using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CompHomp.Migrations
{
    /// <inheritdoc />
    public partial class MakeUserIdNullableInAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "AuditLogs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "AuditLogs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Builds",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 3, 17, 12, 33, 44, 890, DateTimeKind.Local).AddTicks(2830));

            migrationBuilder.UpdateData(
                table: "Builds",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 3, 17, 12, 33, 44, 890, DateTimeKind.Local).AddTicks(2842));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Password", "RegistrationDate" },
                values: new object[] { "$2a$11$UqHsN/y936dGXx5m9UFKbul2lIsXTsmD20G20iU6xKlEpe1zusYFq", new DateTime(2025, 3, 17, 12, 33, 44, 759, DateTimeKind.Local).AddTicks(9802) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Password", "RegistrationDate" },
                values: new object[] { "$2a$11$RYIamSuujUJpKHQDbzQdFOmi7InQNH1chSojctkyXJL/.ldYv4OY.", new DateTime(2025, 3, 17, 12, 33, 44, 890, DateTimeKind.Local).AddTicks(1364) });
        }
    }
}
