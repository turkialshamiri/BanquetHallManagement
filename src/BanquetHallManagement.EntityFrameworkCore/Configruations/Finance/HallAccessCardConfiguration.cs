using BanquetHallManagement.Finance.HallAccessCards;
using BanquetHallManagement.Reservations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace BanquetHallManagement.Configurations.Finance;

public class HallAccessCardConfiguration : IEntityTypeConfiguration<HallAccessCard>
{
    public void Configure(EntityTypeBuilder<HallAccessCard> builder)
    {
        builder.ToTable("HallAccessCards", t =>
        {
            t.HasComment("جدول بطاقات دخول القاعات.");
        });

        builder.Property(x => x.ReservationId)
            .IsRequired()
            .HasComment("معرف الحجز المرتبط ببطاقة الدخول.");

        builder.Property(x => x.CardNumber)
            .IsRequired()
            .HasMaxLength(50)
            .HasComment("رقم بطاقة الدخول مثل HAC-2026-00001.");

        builder.Property(x => x.IssuedAt)
            .IsRequired()
            .HasComment("تاريخ ووقت إصدار البطاقة.");

        builder.Property(x => x.EventDate)
            .IsRequired()
            .HasComment("تاريخ المناسبة.");

        builder.Property(x => x.EntryTime)
            .IsRequired()
            .HasComment("وقت الدخول.");

        builder.Property(x => x.ExitTime)
            .IsRequired()
            .HasComment("وقت الخروج.");

        builder.Property(x => x.IsUsed)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("هل تم استخدام البطاقة؟");

        builder.HasOne<Reservation>()
            .WithMany()
            .HasForeignKey(x => x.ReservationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.CardNumber).IsUnique();
        builder.HasIndex(x => x.ReservationId).IsUnique();

        builder.ConfigureByConvention();
    }
}
