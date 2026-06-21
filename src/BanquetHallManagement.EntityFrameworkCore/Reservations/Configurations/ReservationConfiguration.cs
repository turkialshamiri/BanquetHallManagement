using BanquetHallManagement.Customers;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Reservations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace BanquetHallManagement.EntityFrameworkCore.Reservations.Configurations
{
    public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
    {
        public void Configure(EntityTypeBuilder<Reservation> builder)
        {
            builder.ToTable("Reservations", t =>
            {
                t.HasComment("ÃœÊ· ÌÕ ÊÌ ⁄·Ï »Ì«‰«  «·ÕÃÊ“«  «·Œ«’… »«·ﬁ«⁄«  Ê«·„‰«”»« .");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.HallId)
                .IsRequired()
                .HasComment("„⁄—› «·ﬁ«⁄… «·„— »ÿ… »«·ÕÃ“.");

            builder.Property(x => x.CustomerId)
                .IsRequired()
                .HasComment("„⁄—› «·⁄„Ì· «·–Ì ﬁ«„ »≈‰‘«¡ «·ÕÃ“.");

            builder.Property(x => x.EventDate)
                .IsRequired()
                .HasComment(" «—ÌŒ ≈ﬁ«„… «·„‰«”»… √Ê «·›⁄«·Ì….");

            builder.Property(x => x.StartTime)
                .IsRequired()
                .HasComment("Êﬁ  »œ«Ì… «·ÕÃ“.");

            builder.Property(x => x.EndTime)
                .IsRequired()
                .HasComment("Êﬁ  «‰ Â«¡ «·ÕÃ“.");

            builder.Property(x => x.GuestsCount)
                .IsRequired()
                .HasComment("⁄œœ «·÷ÌÊ› «·„ Êﬁ⁄ Õ÷Ê—Â„ ··„‰«”»….");

            builder.Property(x => x.TotalPrice)
                .HasConversion(
                    money => money.Amount,
                    amount => new Money(amount))
                .HasColumnType("decimal(18,2)")
                .IsRequired()
                .HasComment("«· ﬂ·›… «·≈Ã„«·Ì… ··ÕÃ“ „ ÷„‰… «·Œœ„«  «·≈÷«›Ì….");

            builder.Property(x => x.PaidAmount)
                .HasConversion(
                    money => money.Amount,
                    amount => new Money(amount))
                .HasColumnType("decimal(18,2)")
                .IsRequired()
                .HasDefaultValueSql("0.00")
                .HasComment("≈Ã„«·Ì «·„»«·€ «·„œ›Ê⁄… ⁄·Ï «·ÕÃ“.");

            builder.Property(x => x.ReservationNumber)
                .IsRequired()
                .HasMaxLength(50)
                .HasComment("—ﬁ„ «·ÕÃ“ «· ‘€Ì·Ì „À· RES-2026-00001.");

            builder.Property(x => x.CancellationReason)
                .HasMaxLength(500)
                .HasComment("”»» ≈·€«¡ «·ÕÃ“ ⁄‰œ  Ê›—Â.");

            builder.Property(x => x.CancellationType)
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasComment("‰Ê⁄ ≈·€«¡ «·ÕÃ“ „À·  ⁄«—÷ √Ê ≈·€«¡ ÌœÊÌ √Ê ≈·€«¡  ·ﬁ«∆Ì.");

            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasDefaultValue(ReservationStatus.Pending)
                .HasComment("«·Õ«·… «·Õ«·Ì… ··ÕÃ“ „À· ﬁÌœ «·«‰ Ÿ«— √Ê „ƒﬂœ √Ê „·€Ì √Ê „ﬂ „·.");

            builder.Property(x => x.CompletedAt)
                .HasComment(" «—ÌŒ ÊÊﬁ  ≈ﬂ„«· «·ÕÃ“ ⁄‰œ  Ê›—Â.");

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