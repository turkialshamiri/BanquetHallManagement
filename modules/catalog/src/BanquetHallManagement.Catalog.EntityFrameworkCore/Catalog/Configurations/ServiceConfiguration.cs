using BanquetHallManagement.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace BanquetHallManagement.EntityFrameworkCore.Catalog.Configurations
{
    public class ServiceConfiguration : IEntityTypeConfiguration<Service>
    {
        public void Configure(EntityTypeBuilder<Service> builder)
        {
            builder.ToTable("Services", t =>
            {
                t.HasComment("ÌÏæá íÍÊæí Úáì ÇáÎÏãÇÊ ÇáÅÖÇÝíÉ ÇáãÊÇÍÉ ááÍÌÒ.");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200)
                .HasComment("ÇÓã ÇáÎÏãÉ ÇáÅÖÇÝíÉ ãËá ÇáÖíÇÝÉ Ãæ ÇáÊÕæíÑ Ãæ ÇáÊÒííä.");

            builder.Property(x => x.Price)
                .HasColumnType("decimal(18,2)")
                .IsRequired()
                .HasComment("ÓÚÑ ÇáÎÏãÉ ÇáÅÖÇÝíÉ.");

            builder.HasIndex(x => x.Name);

            builder.ConfigureByConvention();
        }
    }
}