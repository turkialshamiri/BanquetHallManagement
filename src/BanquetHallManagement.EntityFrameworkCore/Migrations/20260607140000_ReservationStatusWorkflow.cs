using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using BanquetHallManagement.EntityFrameworkCore;

#nullable disable

namespace BanquetHallManagement.Migrations
{
    [DbContext(typeof(BanquetHallManagementDbContext))]
    [Migration("20260607140000_ReservationStatusWorkflow")]
    /// <inheritdoc />
    public partial class ReservationStatusWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Reservations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Pending",
                comment: "الحالة الحالية للحجز مثل قيد الانتظار أو مؤكد أو ملغي أو مكتمل.",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldComment: "الحالة الحالية للحجز مثل قيد الانتظار أو مؤكد أو ملغي.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Reservations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                comment: "الحالة الحالية للحجز مثل قيد الانتظار أو مؤكد أو ملغي.",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Pending",
                oldComment: "الحالة الحالية للحجز مثل قيد الانتظار أو مؤكد أو ملغي أو مكتمل.");
        }
    }
}
