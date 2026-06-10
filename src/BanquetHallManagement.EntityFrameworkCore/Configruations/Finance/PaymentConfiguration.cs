using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance.Payments;
using BanquetHallManagement.Reservations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace BanquetHallManagement.Configurations.Finance;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments", t =>
        {
            t.HasComment("جدول دفعات الحجوزات.");
        });

        builder.Property(x => x.ReservationId)
            .IsRequired()
            .HasComment("معرف الحجز المرتبط بالدفعة.");

        builder.Property(x => x.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired()
            .HasComment("مبلغ الدفعة.");

        builder.Property(x => x.PaymentDate)
            .IsRequired()
            .HasComment("تاريخ ووقت الدفعة.");

        builder.Property(x => x.PaymentType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasComment("نوع الدفعة مثل عربون أو قسط.");

        builder.Property(x => x.ReceiptNumber)
            .IsRequired()
            .HasMaxLength(50)
            .HasComment("رقم إيصال القبض.");

        builder.Property(x => x.JournalEntryId)
            .HasComment("معرف القيد المحاسبي المرتبط بالدفعة إن وجد.");

        builder.HasOne<Reservation>()
            .WithMany()
            .HasForeignKey(x => x.ReservationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ReservationId);
        builder.HasIndex(x => x.ReceiptNumber).IsUnique();
        builder.HasIndex(x => x.PaymentDate);

        builder.ConfigureByConvention();
    }
}
