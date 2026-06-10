using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BanquetHallManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddFinanceAccountingFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinanceAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, comment: "رمز الحساب مثل 1100."),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, comment: "اسم الحساب."),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "نوع الحساب: أصل أو التزام أو إيراد."),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "هل الحساب نشط؟"),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinanceAccounts", x => x.Id);
                },
                comment: "جدول شجرة الحسابات المالية.");

            migrationBuilder.CreateTable(
                name: "FinanceNumberSequences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Prefix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, comment: "بادئة الرقم مثل JE أو RC أو INV."),
                    Year = table.Column<int>(type: "int", nullable: false, comment: "السنة المرتبطة بالتسلسل."),
                    LastNumber = table.Column<int>(type: "int", nullable: false, comment: "آخر رقم تم إصداره.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinanceNumberSequences", x => x.Id);
                },
                comment: "جدول تسلسل أرقام المستندات المالية.");

            migrationBuilder.CreateTable(
                name: "JournalEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntryNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "رقم القيد مثل JE-2026-00001."),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "تاريخ القيد."),
                    SourceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "مصدر القيد مثل إيراد عربون أو ترحيل إيراد."),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, comment: "وصف القيد."),
                    ReservationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true, comment: "معرف الحجز المرتبط بالقيد إن وجد."),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true, comment: "معرف الدفعة المرتبطة بالقيد إن وجد."),
                    IsPosted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false, comment: "هل تم ترحيل القيد؟"),
                    PostedTime = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "تاريخ ووقت ترحيل القيد."),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalEntries", x => x.Id);
                },
                comment: "جدول القيود المحاسبية.");

            migrationBuilder.CreateTable(
                name: "JournalEntryLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JournalEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "معرف القيد المحاسبي."),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "معرف الحساب المرتبط بالبند."),
                    Debit = table.Column<decimal>(type: "decimal(18,2)", nullable: false, comment: "مبلغ المدين."),
                    Credit = table.Column<decimal>(type: "decimal(18,2)", nullable: false, comment: "مبلغ الدائن."),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, comment: "وصف البند.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalEntryLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalEntryLines_FinanceAccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "FinanceAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JournalEntryLines_JournalEntries_JournalEntryId",
                        column: x => x.JournalEntryId,
                        principalTable: "JournalEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "جدول بنود القيود المحاسبية.");

            migrationBuilder.CreateIndex(
                name: "IX_FinanceAccounts_Code",
                table: "FinanceAccounts",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinanceNumberSequences_Prefix_Year",
                table: "FinanceNumberSequences",
                columns: new[] { "Prefix", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_EntryDate",
                table: "JournalEntries",
                column: "EntryDate");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_EntryNumber",
                table: "JournalEntries",
                column: "EntryNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_PaymentId",
                table: "JournalEntries",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_ReservationId",
                table: "JournalEntries",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_AccountId",
                table: "JournalEntryLines",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_JournalEntryId",
                table: "JournalEntryLines",
                column: "JournalEntryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinanceNumberSequences");

            migrationBuilder.DropTable(
                name: "JournalEntryLines");

            migrationBuilder.DropTable(
                name: "JournalEntries");

            migrationBuilder.DropTable(
                name: "FinanceAccounts");
        }
    }
}
