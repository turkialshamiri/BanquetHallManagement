using BanquetHallManagement.Customers;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Reservations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace BanquetHallManagement.Configurations.ReservationConfigurations
{
    public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
    {
        public void Configure(EntityTypeBuilder<Reservation> builder)
        {
            builder.ToTable("Reservations", t =>
            {
                t.HasComment("جدول يحتوي على بيانات الحجوزات الخاصة بالقاعات والمناسبات.");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.HallId)
                .IsRequired()
                .HasComment("معرف القاعة المرتبطة بالحجز.");

            builder.Property(x => x.CustomerId)
                .IsRequired()
                .HasComment("معرف العميل الذي قام بإنشاء الحجز.");

            builder.Property(x => x.EventDate)
                .IsRequired()
                .HasComment("تاريخ إقامة المناسبة أو الفعالية.");

            builder.Property(x => x.StartTime)
                .IsRequired()
                .HasComment("وقت بداية الحجز.");

            builder.Property(x => x.EndTime)
                .IsRequired()
                .HasComment("وقت انتهاء الحجز.");

            builder.Property(x => x.GuestsCount)
                .IsRequired()
                .HasComment("عدد الضيوف المتوقع حضورهم للمناسبة.");

            builder.Property(x => x.TotalPrice)
                .HasColumnType("decimal(18,2)")
                .IsRequired()
                .HasComment("التكلفة الإجمالية للحجز متضمنة الخدمات الإضافية.");

            builder.Property(x => x.PaidAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired()
                .HasDefaultValue(0m)
                .HasComment("إجمالي المبالغ المدفوعة على الحجز.");

            builder.Property(x => x.ReservationNumber)
                .IsRequired()
                .HasMaxLength(50)
                .HasComment("رقم الحجز التشغيلي مثل RES-2026-00001.");

            builder.Property(x => x.CancellationReason)
                .HasMaxLength(500)
                .HasComment("سبب إلغاء الحجز عند توفره.");

            builder.Property(x => x.CancellationType)
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasComment("نوع إلغاء الحجز مثل تعارض أو إلغاء يدوي أو إلغاء تلقائي.");

            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasDefaultValue(ReservationStatus.Pending)
                .HasComment("الحالة الحالية للحجز مثل قيد الانتظار أو مؤكد أو ملغي أو مكتمل.");

            builder.HasOne<Hall>()
                .WithMany()
                .HasForeignKey(x => x.HallId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne<Customer>()
                .WithMany()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasMany(x => x.Services)
                .WithOne()
                .HasForeignKey(x => x.ReservationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.HallId);
            builder.HasIndex(x => x.CustomerId);
            builder.HasIndex(x => x.EventDate);
            builder.HasIndex(x => new { x.HallId, x.EventDate });
            builder.HasIndex(x => x.ReservationNumber).IsUnique();

            builder.ConfigureByConvention();
        }
    }
}