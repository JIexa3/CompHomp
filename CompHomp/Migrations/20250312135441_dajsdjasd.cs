using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CompHomp.Migrations
{
    /// <inheritdoc />
    public partial class dajsdjasd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LoginAttempts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Login = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AttemptsCount = table.Column<int>(type: "int", nullable: false),
                    LastAttemptTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginAttempts", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Builds",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 3, 12, 17, 54, 40, 875, DateTimeKind.Local).AddTicks(2976));

            migrationBuilder.UpdateData(
                table: "Builds",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 3, 12, 17, 54, 40, 875, DateTimeKind.Local).AddTicks(2983));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Password", "RegistrationDate" },
                values: new object[] { "$2a$11$4ao92cFbfGEBFSVwDUMUxOJ39Tumj0e7R78pcI8jLVSRcBpTw6ZmG", new DateTime(2025, 3, 12, 17, 54, 40, 736, DateTimeKind.Local).AddTicks(2161) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Password", "RegistrationDate" },
                values: new object[] { "$2a$11$3dkxz0HFIee36VidFcORk.GIKgWbkOYyS0q3lFRWHdJS8FqmWm1G.", new DateTime(2025, 3, 12, 17, 54, 40, 875, DateTimeKind.Local).AddTicks(1436) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoginAttempts");

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
                columns: new[] { "Password", "RegistrationDate" },
                values: new object[] { "$2a$11$1dLEv8PbmR.j13vY9EW8fOmAX7BghvdugUFDuOkKr6rF2lYFDVbZW", new DateTime(2025, 3, 12, 17, 8, 47, 429, DateTimeKind.Local).AddTicks(7789) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Password", "RegistrationDate" },
                values: new object[] { "$2a$11$GxmJiKnYbpAFWbRtUQDS9O8Tb5i2m2u8ndc9O1cXRB2K93pcjqQh.", new DateTime(2025, 3, 12, 17, 8, 47, 560, DateTimeKind.Local).AddTicks(6466) });
        }
    }
}
