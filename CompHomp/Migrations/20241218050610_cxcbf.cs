using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CompHomp.Migrations
{
    /// <inheritdoc />
    public partial class cxcbf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Builds",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2024, 12, 16, 10, 16, 39, 314, DateTimeKind.Local).AddTicks(3390));

            migrationBuilder.UpdateData(
                table: "Builds",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2024, 12, 16, 10, 16, 39, 314, DateTimeKind.Local).AddTicks(3396));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Password", "RegistrationDate" },
                values: new object[] { "$2a$11$Bn.bfqEh67wDzK3Wo72ONeF/HMp.e1w2a9h1oKokL8bdsvKPWBeoi", new DateTime(2024, 12, 16, 10, 16, 39, 170, DateTimeKind.Local).AddTicks(9831) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Password", "RegistrationDate" },
                values: new object[] { "$2a$11$EIBHHfE/XGtLFCJMuQ8mb.lJAasoQpXhMW0ItrmR0d/zpS1GT8qsS", new DateTime(2024, 12, 16, 10, 16, 39, 314, DateTimeKind.Local).AddTicks(2384) });
        }
    }
}
