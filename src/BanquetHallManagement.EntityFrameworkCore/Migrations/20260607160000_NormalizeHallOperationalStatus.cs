using BanquetHallManagement.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BanquetHallManagement.Migrations
{
    [DbContext(typeof(BanquetHallManagementDbContext))]
    [Migration("20260607160000_NormalizeHallOperationalStatus")]
    /// <inheritdoc />
    public partial class NormalizeHallOperationalStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE Halls SET Status = 1 WHERE Status = 3");

            migrationBuilder.Sql(
                "UPDATE Halls SET Status = 2 WHERE Status = 4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
