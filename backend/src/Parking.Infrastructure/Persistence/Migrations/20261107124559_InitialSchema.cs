using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Parking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    occurred_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    username = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    action = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    entity_type = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true),
                    entity_id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true),
                    ip_address = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: true),
                    correlation_id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true),
                    details = table.Column<string>(type: "json", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_logs", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    full_name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    national_id = table.Column<string>(type: "char(14)", fixedLength: true, maxLength: 14, nullable: false),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customers", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "parking_spaces",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    code = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: false),
                    section = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false),
                    number = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false),
                    version = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_parking_spaces", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    username = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    full_name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    password_hash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    role = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    last_login_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "vehicle_brands",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vehicle_brands", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "parking_settings",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    rate_per_minute = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    free_hours = table.Column<int>(type: "int", nullable: false),
                    max_spaces = table.Column<int>(type: "int", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false),
                    updated_by_user_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_parking_settings", x => x.id);
                    table.ForeignKey(
                        name: "fk_parking_settings_users_updated_by_user_id",
                        column: x => x.updated_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    token_hash = table.Column<string>(type: "char(64)", fixedLength: true, maxLength: 64, nullable: false),
                    family_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false),
                    created_by_ip = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: true),
                    revoked_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: true),
                    revoked_reason = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    replaced_by_token_hash = table.Column<string>(type: "char(64)", fixedLength: true, maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "fk_refresh_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "vehicle_models",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    brand_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vehicle_models", x => x.id);
                    table.ForeignKey(
                        name: "fk_vehicle_models_vehicle_brands_brand_id",
                        column: x => x.brand_id,
                        principalTable: "vehicle_brands",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "vehicles",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    plate = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    brand_id = table.Column<int>(type: "int", nullable: false),
                    model_id = table.Column<int>(type: "int", nullable: true),
                    color = table.Column<string>(type: "char(7)", fixedLength: true, maxLength: 7, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vehicles", x => x.id);
                    table.ForeignKey(
                        name: "fk_vehicles_vehicle_brands_brand_id",
                        column: x => x.brand_id,
                        principalTable: "vehicle_brands",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_vehicles_vehicle_models_model_id",
                        column: x => x.model_id,
                        principalTable: "vehicle_models",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tickets",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    code = table.Column<string>(type: "varchar(12)", maxLength: 12, nullable: false),
                    status = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false),
                    space_id = table.Column<int>(type: "int", nullable: false),
                    customer_id = table.Column<int>(type: "int", nullable: false),
                    vehicle_id = table.Column<int>(type: "int", nullable: false),
                    entry_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false),
                    exit_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: true),
                    duration_minutes = table.Column<int>(type: "int", nullable: true),
                    amount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    rate_per_minute = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    free_minutes = table.Column<int>(type: "int", nullable: false),
                    opened_by_user_id = table.Column<int>(type: "int", nullable: false),
                    closed_by_user_id = table.Column<int>(type: "int", nullable: true),
                    active_customer_id = table.Column<int>(type: "int", nullable: true, computedColumnSql: "CASE WHEN `status` = 'Open' THEN `customer_id` END", stored: true),
                    active_space_id = table.Column<int>(type: "int", nullable: true, computedColumnSql: "CASE WHEN `status` = 'Open' THEN `space_id` END", stored: true),
                    active_vehicle_id = table.Column<int>(type: "int", nullable: true, computedColumnSql: "CASE WHEN `status` = 'Open' THEN `vehicle_id` END", stored: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tickets", x => x.id);
                    table.ForeignKey(
                        name: "fk_tickets_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_tickets_parking_spaces_space_id",
                        column: x => x.space_id,
                        principalTable: "parking_spaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_tickets_users_closed_by_user_id",
                        column: x => x.closed_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_tickets_users_opened_by_user_id",
                        column: x => x.opened_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_tickets_vehicles_vehicle_id",
                        column: x => x.vehicle_id,
                        principalTable: "vehicles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_action_occurred_at",
                table: "audit_logs",
                columns: new[] { "action", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_occurred_at",
                table: "audit_logs",
                column: "occurred_at");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_username_occurred_at",
                table: "audit_logs",
                columns: new[] { "username", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "ix_customers_full_name",
                table: "customers",
                column: "full_name");

            migrationBuilder.CreateIndex(
                name: "ux_customers_national_id",
                table: "customers",
                column: "national_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_parking_settings_updated_by_user_id",
                table: "parking_settings",
                column: "updated_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_parking_spaces_section_number",
                table: "parking_spaces",
                columns: new[] { "section", "number" });

            migrationBuilder.CreateIndex(
                name: "ix_parking_spaces_status",
                table: "parking_spaces",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ux_parking_spaces_code",
                table: "parking_spaces",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_family",
                table: "refresh_tokens",
                column: "family_id");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_user_active",
                table: "refresh_tokens",
                columns: new[] { "user_id", "revoked_at", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "ux_refresh_tokens_token_hash",
                table: "refresh_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tickets_closed_by_user_id",
                table: "tickets",
                column: "closed_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_tickets_customer_id",
                table: "tickets",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_tickets_entry_at",
                table: "tickets",
                column: "entry_at");

            migrationBuilder.CreateIndex(
                name: "ix_tickets_opened_by_user_id",
                table: "tickets",
                column: "opened_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_tickets_space_id",
                table: "tickets",
                column: "space_id");

            migrationBuilder.CreateIndex(
                name: "ix_tickets_status_entry_at",
                table: "tickets",
                columns: new[] { "status", "entry_at" });

            migrationBuilder.CreateIndex(
                name: "ix_tickets_status_exit_at",
                table: "tickets",
                columns: new[] { "status", "exit_at" });

            migrationBuilder.CreateIndex(
                name: "ix_tickets_vehicle_id",
                table: "tickets",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "ux_tickets_active_customer",
                table: "tickets",
                column: "active_customer_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_tickets_active_space",
                table: "tickets",
                column: "active_space_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_tickets_active_vehicle",
                table: "tickets",
                column: "active_vehicle_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_tickets_code",
                table: "tickets",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_role_is_active",
                table: "users",
                columns: new[] { "role", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ux_users_username",
                table: "users",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_vehicle_brands_name",
                table: "vehicle_brands",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_vehicle_models_brand_name",
                table: "vehicle_models",
                columns: new[] { "brand_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vehicles_brand_id",
                table: "vehicles",
                column: "brand_id");

            migrationBuilder.CreateIndex(
                name: "ix_vehicles_model_id",
                table: "vehicles",
                column: "model_id");

            migrationBuilder.CreateIndex(
                name: "ux_vehicles_plate",
                table: "vehicles",
                column: "plate",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "parking_settings");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "tickets");

            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropTable(
                name: "parking_spaces");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "vehicles");

            migrationBuilder.DropTable(
                name: "vehicle_models");

            migrationBuilder.DropTable(
                name: "vehicle_brands");
        }
    }
}
