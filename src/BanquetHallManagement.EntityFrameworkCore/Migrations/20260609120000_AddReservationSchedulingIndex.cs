using BanquetHallManagement.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BanquetHallManagement.Migrations
{
    [DbContext(typeof(BanquetHallManagementDbContext))]
    [Migration("20260609120000_AddReservationSchedulingIndex")]
    /// <inheritdoc />
    public partial class AddReservationSchedulingIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Reservations_HallId_EventDate",
                table: "Reservations",
                columns: new[] { "HallId", "EventDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reservations_HallId_EventDate",
                table: "Reservations");
        }
    }
}
