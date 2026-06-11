using BanquetHallManagement.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BanquetHallManagement.Migrations
{
    [DbContext(typeof(BanquetHallManagementDbContext))]
    [Migration("20260610120000_AddReservationFinancialFields")]
    /// <inheritdoc />
    public partial class AddReservationFinancialFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "Reservations",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                comment: "سبب إلغاء الحجز عند توفره.");

            migrationBuilder.AddColumn<string>(
                name: "CancellationType",
                table: "Reservations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                comment: "نوع إلغاء الحجز مثل تعارض أو إلغاء يدوي أو إلغاء تلقائي.");

            migrationBuilder.AddColumn<decimal>(
                name: "PaidAmount",
                table: "Reservations",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                comment: "إجمالي المبالغ المدفوعة على الحجز.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "CancellationType",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "PaidAmount",
                table: "Reservations");
        }
    }
}
