using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CompHomp.Migrations
{
    /// <inheritdoc />
    public partial class asdzxcbvc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EventType",
                table: "AuditLogs",
                newName: "Action");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "AuditLogs",
                newName: "Details");

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Builds",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 3, 16, 0, 18, 29, 56, DateTimeKind.Local).AddTicks(4736));

            migrationBuilder.UpdateData(
                table: "Builds",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 3, 16, 0, 18, 29, 56, DateTimeKind.Local).AddTicks(4744));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Password", "RegistrationDate" },
                values: new object[] { "$2a$11$3HXQKveXmj3T/fllqBGQRO33kiTnii.VfLlUqFtzd3YwH2K/EReBK", new DateTime(2025, 3, 16, 0, 18, 28, 928, DateTimeKind.Local).AddTicks(3017) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Password", "RegistrationDate" },
                values: new object[] { "$2a$11$VNSdWqoeTb9fX8ShqeXU/OMuFCsY8gIfK.u3CpaSgpluHw7QNaeZC", new DateTime(2025, 3, 16, 0, 18, 29, 54, DateTimeKind.Local).AddTicks(3450) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Username",
                table: "AuditLogs");

            migrationBuilder.RenameColumn(
                name: "Details",
                table: "AuditLogs",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "Action",
                table: "AuditLogs",
                newName: "EventType");

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
    }
}
