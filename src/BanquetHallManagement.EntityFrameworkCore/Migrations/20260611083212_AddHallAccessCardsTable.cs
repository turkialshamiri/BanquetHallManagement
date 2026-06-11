using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BanquetHallManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddHallAccessCardsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HallAccessCards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReservationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "معرف الحجز المرتبط ببطاقة الدخول."),
                    CardNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "رقم بطاقة الدخول مثل HAC-2026-00001."),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "تاريخ ووقت إصدار البطاقة."),
                    EventDate = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "تاريخ المناسبة."),
                    EntryTime = table.Column<TimeSpan>(type: "time", nullable: false, comment: "وقت الدخول."),
                    ExitTime = table.Column<TimeSpan>(type: "time", nullable: false, comment: "وقت الخروج."),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false, comment: "هل تم استخدام البطاقة؟"),
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
                    table.PrimaryKey("PK_HallAccessCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HallAccessCards_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "جدول بطاقات دخول القاعات.");

            migrationBuilder.CreateIndex(
                name: "IX_HallAccessCards_CardNumber",
                table: "HallAccessCards",
                column: "CardNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HallAccessCards_ReservationId",
                table: "HallAccessCards",
                column: "ReservationId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HallAccessCards");
        }
    }
}
