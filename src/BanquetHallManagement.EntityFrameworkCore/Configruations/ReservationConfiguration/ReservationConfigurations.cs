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

            builder.Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasComment("الحالة الحالية للحجز مثل قيد الانتظار أو مؤكد أو ملغي.");

            builder.HasMany(x => x.Services)
                .WithOne()
                .HasForeignKey(x => x.ReservationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.EventDate);

            builder.ConfigureByConvention();
        }
    }
}