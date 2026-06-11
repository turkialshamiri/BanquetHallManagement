using System;
using BanquetHallManagement.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BanquetHallManagement.Migrations
{
    [DbContext(typeof(BanquetHallManagementDbContext))]
    [Migration("20260610180000_AddJournalEntryPaymentIdUniqueIndex")]
    /// <inheritdoc />
    public partial class AddJournalEntryPaymentIdUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JournalEntries_PaymentId",
                table: "JournalEntries");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_PaymentId",
                table: "JournalEntries",
                column: "PaymentId",
                unique: true,
                filter: "[PaymentId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JournalEntries_PaymentId",
                table: "JournalEntries");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_PaymentId",
                table: "JournalEntries",
                column: "PaymentId");
        }
    }
}
