using BanquetHallManagement.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace BanquetHallManagement.EntityFrameworkCore.Catalog.Configurations
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("Customers", t =>
            {
                t.HasComment("ÌÏæá íÍÊæí Úáì ÈíÇäÇÊ ÇáÚãáÇÁ ÇáãÓÊÎÏãíä áäÙÇã ÇáÍÌÒ.");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200)
                .HasComment("ÇáÇÓã ÇáßÇãá ááÚãíá.");

            builder.Property(x => x.Phone)
                .IsRequired()
                .HasMaxLength(20)
                .HasComment("ÑÞã ÇáåÇÊÝ ÇáãÓÊÎÏã ááÊæÇÕá ãÚ ÇáÚãíá æÅÑÓÇá ÇáÅÔÚÇÑÇÊ.");

            builder.Property(x => x.Company)
                .HasMaxLength(300)
                .HasComment("ÇÓã ÇáÔÑßÉ Ãæ ÇáÌåÉ ÇáÊÇÈÚÉ ááÚãíá Åä æÌÏÊ.");

            builder.HasIndex(x => x.Phone)
                .IsUnique();

            builder.ConfigureByConvention();
        }
    }
}