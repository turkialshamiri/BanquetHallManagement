using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance.Invoices;
using BanquetHallManagement.Finance.Payments;
using BanquetHallManagement.Reservations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace BanquetHallManagement.Configurations.Finance;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices", t =>
        {
            t.HasComment("جدول فواتير الحجوزات.");
        });

        builder.Property(x => x.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50)
            .HasComment("رقم الفاتورة مثل INV-2026-00001.");

        builder.Property(x => x.ReservationId)
            .IsRequired()
            .HasComment("معرف الحجز المرتبط بالفاتورة.");

        builder.Property(x => x.PaymentId)
            .IsRequired()
            .HasComment("معرف الدفعة المرتبطة بالفاتورة.");

        builder.Property(x => x.InvoiceType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasComment("نوع الفاتورة مثل عربون أو قسط.");

        builder.Property(x => x.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired()
            .HasComment("مبلغ الفاتورة.");

        builder.Property(x => x.IssuedAt)
            .IsRequired()
            .HasComment("تاريخ ووقت إصدار الفاتورة.");

        builder.HasOne<Reservation>()
            .WithMany()
            .HasForeignKey(x => x.ReservationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Payment>()
            .WithMany()
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.InvoiceNumber).IsUnique();
        builder.HasIndex(x => x.PaymentId).IsUnique();
        builder.HasIndex(x => x.ReservationId);
        builder.HasIndex(x => x.IssuedAt);

        builder.ConfigureByConvention();
    }
}
