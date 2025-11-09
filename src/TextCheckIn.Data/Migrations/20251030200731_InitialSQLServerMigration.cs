using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TextCheckIn.Data.Migrations
{
    public partial class InitialSQLServerMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    customer_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    customer_uuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    customer_first_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    customer_last_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    customer_email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    customer_phone_number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    is_fleet_customer = table.Column<bool>(type: "bit", nullable: false),
                    customer_created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    customer_updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customers", x => x.customer_id);
                });

            migrationBuilder.CreateTable(
                name: "services",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    service_uuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    price = table.Column<decimal>(type: "decimal(18,2)", maxLength: 10, nullable: false),
                    estimated_duration_minutes = table.Column<int>(type: "int", maxLength: 10, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_services", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "state_codes",
                columns: table => new
                {
                    code = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_state_codes", x => x.code);
                });

            migrationBuilder.CreateTable(
                name: "check_in_sessions",
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
                    table.PrimaryKey("PK_check_in_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_check_in_sessions_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "customer_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "stores",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    uuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    city = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    state_code = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    zip_code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    phone_number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stores", x => x.id);
                    table.ForeignKey(
                        name: "FK_stores_state_codes_state_code",
                        column: x => x.state_code,
                        principalTable: "state_codes",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vehicles",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    vehicle_uuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    vin = table.Column<string>(type: "nvarchar(17)", maxLength: 17, nullable: true),
                    license_plate = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    state_code = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    make = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    year_of_make = table.Column<int>(type: "int", maxLength: 4, nullable: true),
                    last_mileage = table.Column<int>(type: "int", nullable: true),
                    last_service_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    last_service_mileage = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicles", x => x.id);
                    table.ForeignKey(
                        name: "FK_vehicles_state_codes_state_code",
                        column: x => x.state_code,
                        principalTable: "state_codes",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "check_ins",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    uuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    client_location_id = table.Column<int>(type: "int", nullable: true),
                    omnix_location_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    vehicle_id = table.Column<int>(type: "int", nullable: true),
                    customer_id = table.Column<int>(type: "int", nullable: true),
                    store_id = table.Column<int>(type: "int", nullable: true),
                    is_processed = table.Column<bool>(type: "bit", nullable: false),
                    estimated_mileage = table.Column<int>(type: "int", nullable: true),
                    datetime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_check_ins", x => x.id);
                    table.ForeignKey(
                        name: "FK_check_ins_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "customer_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_check_ins_stores_store_id",
                        column: x => x.store_id,
                        principalTable: "stores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_check_ins_vehicles_vehicle_id",
                        column: x => x.vehicle_id,
                        principalTable: "vehicles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "customers_vehicles",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    customer_id = table.Column<int>(type: "int", nullable: false),
                    vehicle_id = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customers_vehicles", x => x.id);
                    table.ForeignKey(
                        name: "FK_customers_vehicles_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "customer_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_customers_vehicles_vehicles_vehicle_id",
                        column: x => x.vehicle_id,
                        principalTable: "vehicles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "check_in_services",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    check_in_id = table.Column<int>(type: "int", nullable: false),
                    service_id = table.Column<int>(type: "int", nullable: false),
                    is_customer_selected = table.Column<bool>(type: "bit", nullable: false),
                    interval_miles = table.Column<int>(type: "int", nullable: false),
                    last_service_miles = table.Column<int>(type: "int", nullable: true),
                    mileage_bucket = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_check_in_services", x => x.id);
                    table.ForeignKey(
                        name: "FK_check_in_services_check_ins_check_in_id",
                        column: x => x.check_in_id,
                        principalTable: "check_ins",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_check_in_services_services_service_id",
                        column: x => x.service_id,
                        principalTable: "services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "services",
                columns: new[] { "id", "created_at", "description", "estimated_duration_minutes", "name", "price", "service_uuid", "updated_at" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Complete transmission fluid replacement and system inspection to ensure smooth shifting and optimal performance.", 45, "Transmission service", 189.99m, new Guid("c6f1ef6a-8d9f-46ac-94e8-359c3aa11ffd"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Coolant system flush and replacement to prevent overheating and maintain optimal engine temperature.", 30, "Coolant service", 149.99m, new Guid("3e354208-59d0-46e5-a6a0-f3cb47938c44"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Deep cleaning of intake valves and combustion chambers to restore engine performance and fuel efficiency.", 60, "Intake system cleaning", 229.99m, new Guid("4d816599-6a13-4aa4-959a-c8eb8168e71c"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Power steering fluid replacement to ensure easy steering and prevent system damage.", 20, "Power steering fluid service", 89.99m, new Guid("672b3a26-3f03-4db7-a7fb-317f4f60baf2"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Gearbox fluid replacement and inspection to maintain smooth gear changes and extend transmission life.", 40, "Gear box service", 169.99m, new Guid("b54ad209-7e30-47c7-828d-1174dcebbf63"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cabin air filter replacement to ensure clean air circulation and optimal HVAC performance.", 15, "Cabin Air Filter", 49.99m, new Guid("f072fcb0-425f-4961-9894-cb6b7aec5133"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "state_codes",
                columns: new[] { "code", "name" },
                values: new object[,]
                {
                    { "AK", "Alaska" },
                    { "AL", "Alabama" },
                    { "AR", "Arkansas" },
                    { "AZ", "Arizona" },
                    { "CA", "California" },
                    { "CO", "Colorado" },
                    { "CT", "Connecticut" },
                    { "DC", "District of Columbia" },
                    { "DE", "Delaware" },
                    { "FL", "Florida" },
                    { "GA", "Georgia" },
                    { "HI", "Hawaii" },
                    { "IA", "Iowa" },
                    { "ID", "Idaho" },
                    { "IL", "Illinois" },
                    { "IN", "Indiana" },
                    { "KS", "Kansas" },
                    { "KY", "Kentucky" },
                    { "LA", "Louisiana" },
                    { "MA", "Massachusetts" },
                    { "MD", "Maryland" },
                    { "ME", "Maine" },
                    { "MI", "Michigan" },
                    { "MN", "Minnesota" },
                    { "MO", "Missouri" },
                    { "MS", "Mississippi" },
                    { "MT", "Montana" },
                    { "NC", "North Carolina" },
                    { "ND", "North Dakota" },
                    { "NE", "Nebraska" },
                    { "NH", "New Hampshire" },
                    { "NJ", "New Jersey" },
                    { "NM", "New Mexico" },
                    { "NV", "Nevada" },
                    { "NY", "New York" },
                    { "OH", "Ohio" },
                    { "OK", "Oklahoma" },
                    { "OR", "Oregon" },
                    { "PA", "Pennsylvania" },
                    { "RI", "Rhode Island" },
                    { "SC", "South Carolina" },
                    { "SD", "South Dakota" },
                    { "TN", "Tennessee" },
                    { "TX", "Texas" },
                    { "UT", "Utah" },
                    { "VA", "Virginia" },
                    { "VT", "Vermont" },
                    { "WA", "Washington" },
                    { "WI", "Wisconsin" },
                    { "WV", "West Virginia" },
                    { "WY", "Wyoming" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_check_in_services_check_in_id",
                table: "check_in_services",
                column: "check_in_id");

            migrationBuilder.CreateIndex(
                name: "IX_check_in_services_service_id",
                table: "check_in_services",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "IX_check_in_sessions_customer_id",
                table: "check_in_sessions",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_check_ins_customer_id",
                table: "check_ins",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_check_ins_store_id",
                table: "check_ins",
                column: "store_id");

            migrationBuilder.CreateIndex(
                name: "IX_check_ins_vehicle_id",
                table: "check_ins",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "IX_customers_vehicles_customer_id",
                table: "customers_vehicles",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_customers_vehicles_vehicle_id",
                table: "customers_vehicles",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "IX_stores_state_code",
                table: "stores",
                column: "state_code");

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_license_plate_state_code",
                table: "vehicles",
                columns: new[] { "license_plate", "state_code" },
                unique: true,
                filter: "[state_code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_state_code",
                table: "vehicles",
                column: "state_code");

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_vin",
                table: "vehicles",
                column: "vin",
                unique: true,
                filter: "[vin] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "check_in_services");

            migrationBuilder.DropTable(
                name: "check_in_sessions");

            migrationBuilder.DropTable(
                name: "customers_vehicles");

            migrationBuilder.DropTable(
                name: "check_ins");

            migrationBuilder.DropTable(
                name: "services");

            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropTable(
                name: "stores");

            migrationBuilder.DropTable(
                name: "vehicles");

            migrationBuilder.DropTable(
                name: "state_codes");
        }
    }
}
