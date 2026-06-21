using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance.JournalEntries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace BanquetHallManagement.EntityFrameworkCore.Finance.Configurations;

public class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.ToTable("JournalEntries", t =>
        {
            t.HasComment("جدول القيود المحاسبية.");
        });

        builder.Property(x => x.EntryNumber)
            .IsRequired()
            .HasMaxLength(50)
            .HasComment("رقم القيد مثل JE-2026-00001.");

        builder.Property(x => x.EntryDate)
            .IsRequired()
            .HasComment("تاريخ القيد.");

        builder.Property(x => x.SourceType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasComment("مصدر القيد مثل إيراد عربون أو ترحيل إيراد.");

        builder.Property(x => x.Description)
            .HasMaxLength(500)
            .HasComment("وصف القيد.");

        builder.Property(x => x.ReservationId)
            .HasComment("معرف الحجز المرتبط بالقيد إن وجد.");

        builder.Property(x => x.PaymentId)
            .HasComment("معرف الدفعة المرتبطة بالقيد إن وجد.");

        builder.Property(x => x.ReservationNumber)
            .HasMaxLength(50)
            .HasComment("رقم الحجز المرتبط بالقيد.");

        builder.Property(x => x.CustomerName)
            .HasMaxLength(200)
            .HasComment("اسم العميل المرتبط بالقيد.");

        builder.Property(x => x.HallName)
            .HasMaxLength(200)
            .HasComment("اسم القاعة المرتبطة بالقيد.");

        builder.Property(x => x.EmployeeName)
            .HasMaxLength(200)
            .HasComment("اسم الموظف الذي أنشأ القيد.");

        builder.Property(x => x.IsPosted)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("هل تم ترحيل القيد؟");

        builder.Property(x => x.PostedTime)
            .HasComment("تاريخ ووقت ترحيل القيد.");

        builder.HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey(x => x.JournalEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.EntryNumber).IsUnique();
        builder.HasIndex(x => x.EntryDate);
        builder.HasIndex(x => x.ReservationId);
        builder.HasIndex(x => x.PaymentId)
            .IsUnique()
            .HasFilter("[PaymentId] IS NOT NULL");

        builder.ConfigureByConvention();
    }
}
