using BanquetHallManagement.ReservationServices;
using BanquetHallManagement.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace BanquetHallManagement.Configurations.ReservationServiceConfigurations
{
    public class ReservationServiceConfiguration : IEntityTypeConfiguration<ReservationService>
    {
        public void Configure(EntityTypeBuilder<ReservationService> builder)
        {
            builder.ToTable("ReservationServices", t =>
            {
                t.HasComment("جدول وسيط لربط الحجوزات بالخدمات الإضافية.");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ReservationId)
                .IsRequired()
                .HasComment("معرف الحجز المرتبط بالخدمة.");

            builder.Property(x => x.ServiceId)
                .IsRequired()
                .HasComment("معرف الخدمة المضافة إلى الحجز.");

            builder.HasOne<Service>()
                .WithMany()
                .HasForeignKey(x => x.ServiceId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasIndex(x => x.ReservationId);
            builder.HasIndex(x => x.ServiceId);
            builder.HasIndex(x => new { x.ReservationId, x.ServiceId });

            builder.ConfigureByConvention();
        }
    }
}