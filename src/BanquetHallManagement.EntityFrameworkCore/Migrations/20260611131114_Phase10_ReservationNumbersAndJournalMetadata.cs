using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BanquetHallManagement.Migrations
{
    /// <inheritdoc />
    public partial class Phase10_ReservationNumbersAndJournalMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReservationNumber",
                table: "Reservations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                comment: "رقم الحجز التشغيلي مثل RES-2026-00001.");

            migrationBuilder.Sql(@"
WITH numbered AS (
    SELECT
        Id,
        CONCAT('RES-LEGACY-', FORMAT(ROW_NUMBER() OVER (ORDER BY CreationTime, Id), '00000')) AS GeneratedNumber
    FROM Reservations
    WHERE ReservationNumber IS NULL
)
UPDATE r
SET r.ReservationNumber = n.GeneratedNumber
FROM Reservations r
INNER JOIN numbered n ON r.Id = n.Id;
");

            migrationBuilder.AlterColumn<string>(
                name: "ReservationNumber",
                table: "Reservations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "JournalEntries",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                comment: "اسم العميل المرتبط بالقيد.");

            migrationBuilder.AddColumn<string>(
                name: "EmployeeName",
                table: "JournalEntries",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                comment: "اسم الموظف الذي أنشأ القيد.");

            migrationBuilder.AddColumn<string>(
                name: "HallName",
                table: "JournalEntries",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                comment: "اسم القاعة المرتبطة بالقيد.");

            migrationBuilder.AddColumn<string>(
                name: "ReservationNumber",
                table: "JournalEntries",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                comment: "رقم الحجز المرتبط بالقيد.");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ReservationNumber",
                table: "Reservations",
                column: "ReservationNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reservations_ReservationNumber",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "ReservationNumber",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "EmployeeName",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "HallName",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "ReservationNumber",
                table: "JournalEntries");
        }
    }
}
