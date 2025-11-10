using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TextCheckIn.Data.Migrations
{
    /// <inheritdoc />
    public partial class ActualMileageMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "check_in_sessions");

            migrationBuilder.AlterColumn<string>(
                name: "customer_phone_number",
                table: "customers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "actual_mileage",
                table: "check_ins",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    customer_id = table.Column<int>(type: "int", nullable: true),
                    ip_address = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    last_activity = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_sessions_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "customer_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "services",
                columns: new[] { "id", "created_at", "description", "estimated_duration_minutes", "name", "price", "service_uuid", "updated_at" },
                values: new object[] { 7, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Comprehensive oil change service including lubrication of chassis components and replacement of oil filter to ensure engine longevity.", 25, "Lube, Oil and Filter", 99.99m, new Guid("eceed116-245a-4be3-9e6e-16ec807c6241"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.CreateIndex(
                name: "IX_sessions_customer_id",
                table: "sessions",
                column: "customer_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sessions");

            migrationBuilder.DeleteData(
                table: "services",
                keyColumn: "id",
                keyValue: 7);

            migrationBuilder.DropColumn(
                name: "actual_mileage",
                table: "check_ins");

            migrationBuilder.AlterColumn<string>(
                name: "customer_phone_number",
                table: "customers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.CreateTable(
                name: "check_in_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    customer_id = table.Column<int>(type: "int", nullable: true),
                    ip_address = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    last_activity = table.Column<DateTime>(type: "datetime2", nullable: false),
                    payload = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_check_in_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_check_in_sessions_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "customer_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_check_in_sessions_customer_id",
                table: "check_in_sessions",
                column: "customer_id");
        }
    }
}
