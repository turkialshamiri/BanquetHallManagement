using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BanquetHallManagement.Migrations
{
    /// <inheritdoc />
    public partial class _20260617000000_AddDashboardMetricsSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DashboardMetricsSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalHalls = table.Column<long>(type: "bigint", nullable: false),
                    TotalCustomers = table.Column<long>(type: "bigint", nullable: false),
                    TotalServices = table.Column<long>(type: "bigint", nullable: false),
                    TotalReservations = table.Column<long>(type: "bigint", nullable: false),
                    PendingReservations = table.Column<long>(type: "bigint", nullable: false),
                    ConfirmedReservations = table.Column<long>(type: "bigint", nullable: false),
                    CancelledReservations = table.Column<long>(type: "bigint", nullable: false),
                    CompletedReservations = table.Column<long>(type: "bigint", nullable: false),
                    TotalRevenue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDeferredRevenue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SnapshotCreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DashboardMetricsSnapshots", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DashboardMetricsSnapshots_SnapshotCreatedAt",
                table: "DashboardMetricsSnapshots",
                column: "SnapshotCreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DashboardMetricsSnapshots");
        }
    }
}
