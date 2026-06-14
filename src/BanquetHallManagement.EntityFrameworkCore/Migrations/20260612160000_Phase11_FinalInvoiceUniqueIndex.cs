using System;
using BanquetHallManagement.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BanquetHallManagement.Migrations
{
    [DbContext(typeof(BanquetHallManagementDbContext))]
    [Migration("20260612160000_Phase11_FinalInvoiceUniqueIndex")]
    /// <inheritdoc />
    public partial class Phase11_FinalInvoiceUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ReservationId_InvoiceType",
                table: "Invoices",
                columns: new[] { "ReservationId", "InvoiceType" },
                unique: true,
                filter: "[InvoiceType] = 'Final'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Invoices_ReservationId_InvoiceType",
                table: "Invoices");
        }
    }
}
