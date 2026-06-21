using BanquetHallManagement.ReservationServices;
using BanquetHallManagement.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace BanquetHallManagement.EntityFrameworkCore.Reservations.Configurations
{
    public class ReservationServiceConfiguration : IEntityTypeConfiguration<ReservationService>
    {
        public void Configure(EntityTypeBuilder<ReservationService> builder)
        {
            builder.ToTable("ReservationServices", t =>
            {
                t.HasComment("ÃœÊ· Ê”Ìÿ ·—»ÿ «·ÕÃÊ“«  »«·Œœ„«  «·≈÷«›Ì….");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ReservationId)
                .IsRequired()
                .HasComment("„⁄—› «·ÕÃ“ «·„— »ÿ »«·Œœ„….");

            builder.Property(x => x.ServiceId)
                .IsRequired()
                .HasComment("„⁄—› «·Œœ„… «·„÷«›… ≈·Ï «·ÕÃ“.");

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