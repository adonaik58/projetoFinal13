using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SpaceCreatorAndCustomerPhone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "created_by_user_id",
                table: "parking_spaces",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "phone",
                table: "customers",
                type: "varchar(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_parking_spaces_created_by_user_id",
                table: "parking_spaces",
                column: "created_by_user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_parking_spaces_users_created_by_user_id",
                table: "parking_spaces",
                column: "created_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            // Backfill the creator of existing spaces from the audit trail (matched by code/range and time).
            migrationBuilder.Sql("""
                UPDATE parking_spaces s
                JOIN audit_logs a ON a.action = 'space.created' AND a.entity_id = s.code
                    AND a.occurred_at BETWEEN s.created_at - INTERVAL 2 MINUTE AND s.created_at + INTERVAL 2 MINUTE
                SET s.created_by_user_id = a.user_id
                WHERE s.created_by_user_id IS NULL AND a.user_id IS NOT NULL;
                """);
            migrationBuilder.Sql("""
                UPDATE parking_spaces s
                JOIN audit_logs a ON a.action = 'space.bulk_created'
                    AND JSON_CONTAINS(UPPER(JSON_EXTRACT(a.details, '$.sections')), JSON_QUOTE(s.section))
                    AND s.number BETWEEN CAST(a.details->>'$.fromNumber' AS UNSIGNED) AND CAST(a.details->>'$.toNumber' AS UNSIGNED)
                    AND a.occurred_at BETWEEN s.created_at - INTERVAL 2 MINUTE AND s.created_at + INTERVAL 2 MINUTE
                SET s.created_by_user_id = a.user_id
                WHERE s.created_by_user_id IS NULL AND a.user_id IS NOT NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_parking_spaces_users_created_by_user_id",
                table: "parking_spaces");

            migrationBuilder.DropIndex(
                name: "ix_parking_spaces_created_by_user_id",
                table: "parking_spaces");

            migrationBuilder.DropColumn(
                name: "created_by_user_id",
                table: "parking_spaces");

            migrationBuilder.DropColumn(
                name: "phone",
                table: "customers");
        }
    }
}
