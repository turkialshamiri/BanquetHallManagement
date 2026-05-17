using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BanquetHallManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddedReservationSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReservationServices_ReservationId",
                table: "ReservationServices");

            migrationBuilder.AlterTable(
                name: "Services",
                comment: "جدول يحتوي على الخدمات الإضافية المتاحة للحجز.");

            migrationBuilder.AlterTable(
                name: "ReservationServices",
                comment: "جدول وسيط لربط الحجوزات بالخدمات الإضافية.");

            migrationBuilder.AlterTable(
                name: "Reservations",
                comment: "جدول يحتوي على بيانات الحجوزات الخاصة بالقاعات والمناسبات.");

            migrationBuilder.AlterTable(
                name: "Halls",
                comment: "جدول يحتوي على بيانات القاعات المتاحة للحجز داخل النظام.");

            migrationBuilder.AlterTable(
                name: "Customers",
                comment: "جدول يحتوي على بيانات العملاء المستخدمين لنظام الحجز.");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Services",
                type: "decimal(18,2)",
                nullable: false,
                comment: "سعر الخدمة الإضافية.",
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Services",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                comment: "اسم الخدمة الإضافية مثل الضيافة أو التصوير أو التزيين.",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<Guid>(
                name: "ServiceId",
                table: "ReservationServices",
                type: "uniqueidentifier",
                nullable: false,
                comment: "معرف الخدمة المضافة إلى الحجز.",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "ReservationId",
                table: "ReservationServices",
                type: "uniqueidentifier",
                nullable: false,
                comment: "معرف الحجز المرتبط بالخدمة.",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalPrice",
                table: "Reservations",
                type: "decimal(18,2)",
                nullable: false,
                comment: "التكلفة الإجمالية للحجز متضمنة الخدمات الإضافية.",
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Reservations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                comment: "الحالة الحالية للحجز مثل قيد الانتظار أو مؤكد أو ملغي.",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "StartTime",
                table: "Reservations",
                type: "time",
                nullable: false,
                comment: "وقت بداية الحجز.",
                oldClrType: typeof(TimeSpan),
                oldType: "time");

            migrationBuilder.AlterColumn<Guid>(
                name: "HallId",
                table: "Reservations",
                type: "uniqueidentifier",
                nullable: false,
                comment: "معرف القاعة المرتبطة بالحجز.",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<int>(
                name: "GuestsCount",
                table: "Reservations",
                type: "int",
                nullable: false,
                comment: "عدد الضيوف المتوقع حضورهم للمناسبة.",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EventDate",
                table: "Reservations",
                type: "datetime2",
                nullable: false,
                comment: "تاريخ إقامة المناسبة أو الفعالية.",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "EndTime",
                table: "Reservations",
                type: "time",
                nullable: false,
                comment: "وقت انتهاء الحجز.",
                oldClrType: typeof(TimeSpan),
                oldType: "time");

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "Reservations",
                type: "uniqueidentifier",
                nullable: false,
                comment: "معرف العميل الذي قام بإنشاء الحجز.",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "Halls",
                type: "int",
                nullable: false,
                comment: "تصنيف القاعة حسب نوع المناسبة مثل أعراس أو مؤتمرات أو اجتماعات.",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Halls",
                type: "int",
                nullable: false,
                comment: "الحالة التشغيلية الحالية للقاعة مثل متاحة أو محجوزة أو تحت الصيانة.",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "PricePerHour",
                table: "Halls",
                type: "decimal(18,2)",
                nullable: false,
                comment: "تكلفة استئجار القاعة لكل ساعة محسوبة بالعملة المعتمدة في النظام.",
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Halls",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                comment: "الاسم الرسمي للقاعة المستخدم في عمليات العرض والبحث والحجز داخل النظام.",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "Halls",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                comment: "الموقع الجغرافي أو العنوان التفصيلي للقاعة لتسهيل الوصول إليها.",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Halls",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                comment: "وصف تفصيلي للقاعة يشمل المميزات والخدمات والتجهيزات المتوفرة.",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "Capacity",
                table: "Halls",
                type: "int",
                nullable: false,
                comment: "العدد الأقصى للأشخاص المسموح باستقبالهم داخل القاعة.",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Customers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                comment: "رقم الهاتف المستخدم للتواصل مع العميل وإرسال الإشعارات.",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Customers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                comment: "الاسم الكامل للعميل.",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Company",
                table: "Customers",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true,
                comment: "اسم الشركة أو الجهة التابعة للعميل إن وجدت.",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Services_Name",
                table: "Services",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationServices_ReservationId_ServiceId",
                table: "ReservationServices",
                columns: new[] { "ReservationId", "ServiceId" });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_EventDate",
                table: "Reservations",
                column: "EventDate");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Phone",
                table: "Customers",
                column: "Phone",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Services_Name",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_ReservationServices_ReservationId_ServiceId",
                table: "ReservationServices");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_EventDate",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Customers_Phone",
                table: "Customers");

            migrationBuilder.AlterTable(
                name: "Services",
                oldComment: "جدول يحتوي على الخدمات الإضافية المتاحة للحجز.");

            migrationBuilder.AlterTable(
                name: "ReservationServices",
                oldComment: "جدول وسيط لربط الحجوزات بالخدمات الإضافية.");

            migrationBuilder.AlterTable(
                name: "Reservations",
                oldComment: "جدول يحتوي على بيانات الحجوزات الخاصة بالقاعات والمناسبات.");

            migrationBuilder.AlterTable(
                name: "Halls",
                oldComment: "جدول يحتوي على بيانات القاعات المتاحة للحجز داخل النظام.");

            migrationBuilder.AlterTable(
                name: "Customers",
                oldComment: "جدول يحتوي على بيانات العملاء المستخدمين لنظام الحجز.");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Services",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldComment: "سعر الخدمة الإضافية.");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Services",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldComment: "اسم الخدمة الإضافية مثل الضيافة أو التصوير أو التزيين.");

            migrationBuilder.AlterColumn<Guid>(
                name: "ServiceId",
                table: "ReservationServices",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldComment: "معرف الخدمة المضافة إلى الحجز.");

            migrationBuilder.AlterColumn<Guid>(
                name: "ReservationId",
                table: "ReservationServices",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldComment: "معرف الحجز المرتبط بالخدمة.");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalPrice",
                table: "Reservations",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldComment: "التكلفة الإجمالية للحجز متضمنة الخدمات الإضافية.");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Reservations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldComment: "الحالة الحالية للحجز مثل قيد الانتظار أو مؤكد أو ملغي.");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "StartTime",
                table: "Reservations",
                type: "time",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "time",
                oldComment: "وقت بداية الحجز.");

            migrationBuilder.AlterColumn<Guid>(
                name: "HallId",
                table: "Reservations",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldComment: "معرف القاعة المرتبطة بالحجز.");

            migrationBuilder.AlterColumn<int>(
                name: "GuestsCount",
                table: "Reservations",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "عدد الضيوف المتوقع حضورهم للمناسبة.");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EventDate",
                table: "Reservations",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldComment: "تاريخ إقامة المناسبة أو الفعالية.");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "EndTime",
                table: "Reservations",
                type: "time",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "time",
                oldComment: "وقت انتهاء الحجز.");

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "Reservations",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldComment: "معرف العميل الذي قام بإنشاء الحجز.");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "Halls",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "تصنيف القاعة حسب نوع المناسبة مثل أعراس أو مؤتمرات أو اجتماعات.");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Halls",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "الحالة التشغيلية الحالية للقاعة مثل متاحة أو محجوزة أو تحت الصيانة.");

            migrationBuilder.AlterColumn<decimal>(
                name: "PricePerHour",
                table: "Halls",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldComment: "تكلفة استئجار القاعة لكل ساعة محسوبة بالعملة المعتمدة في النظام.");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Halls",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldComment: "الاسم الرسمي للقاعة المستخدم في عمليات العرض والبحث والحجز داخل النظام.");

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "Halls",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldComment: "الموقع الجغرافي أو العنوان التفصيلي للقاعة لتسهيل الوصول إليها.");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Halls",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldComment: "وصف تفصيلي للقاعة يشمل المميزات والخدمات والتجهيزات المتوفرة.");

            migrationBuilder.AlterColumn<int>(
                name: "Capacity",
                table: "Halls",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "العدد الأقصى للأشخاص المسموح باستقبالهم داخل القاعة.");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldComment: "رقم الهاتف المستخدم للتواصل مع العميل وإرسال الإشعارات.");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldComment: "الاسم الكامل للعميل.");

            migrationBuilder.AlterColumn<string>(
                name: "Company",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300,
                oldNullable: true,
                oldComment: "اسم الشركة أو الجهة التابعة للعميل إن وجدت.");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationServices_ReservationId",
                table: "ReservationServices",
                column: "ReservationId");
        }
    }
}
