using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CompHomp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBuildRatingsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuildComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuildId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuildComments_Builds_BuildId",
                        column: x => x.BuildId,
                        principalTable: "Builds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BuildComments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BuildRatings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuildId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    IsLike = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuildRatings_Builds_BuildId",
                        column: x => x.BuildId,
                        principalTable: "Builds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BuildRatings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_BuildComments_BuildId",
                table: "BuildComments",
                column: "BuildId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildComments_UserId",
                table: "BuildComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildRatings_BuildId",
                table: "BuildRatings",
                column: "BuildId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildRatings_UserId",
                table: "BuildRatings",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildComments");

            migrationBuilder.DropTable(
                name: "BuildRatings");

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
    }
}
