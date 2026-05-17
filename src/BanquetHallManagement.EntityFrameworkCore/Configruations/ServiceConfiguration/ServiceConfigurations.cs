using BanquetHallManagement.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace BanquetHallManagement.Configurations.ServiceConfigurations
{
    public class ServiceConfiguration : IEntityTypeConfiguration<Service>
    {
        public void Configure(EntityTypeBuilder<Service> builder)
        {
            builder.ToTable("Services", t =>
            {
                t.HasComment("جدول يحتوي على الخدمات الإضافية المتاحة للحجز.");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200)
                .HasComment("اسم الخدمة الإضافية مثل الضيافة أو التصوير أو التزيين.");

            builder.Property(x => x.Price)
                .HasColumnType("decimal(18,2)")
                .IsRequired()
                .HasComment("سعر الخدمة الإضافية.");

            builder.HasIndex(x => x.Name);

            builder.ConfigureByConvention();
        }
    }
}