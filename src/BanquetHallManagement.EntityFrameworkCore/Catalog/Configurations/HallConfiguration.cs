using BanquetHallManagement.Entities;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace BanquetHallManagement.EntityFrameworkCore.Catalog.Configurations
{
    public class HallConfiguration : IEntityTypeConfiguration<Hall>
    {
        public void Configure(EntityTypeBuilder<Hall> builder)
        {
            builder.ToTable("Halls", t =>
            {
                t.HasComment("ÌÏæá íÍÊæí Úáì ÈíÇäÇÊ ÇáŞÇÚÇÊ ÇáãÊÇÍÉ ááÍÌÒ ÏÇÎá ÇáäÙÇã.");
            });

            builder.Property(x => x.Name)
                  .IsRequired()
                  .HasMaxLength(200)
                  .HasComment("ÇáÇÓã ÇáÑÓãí ááŞÇÚÉ ÇáãÓÊÎÏã İí ÚãáíÇÊ ÇáÚÑÖ æÇáÈÍË æÇáÍÌÒ ÏÇÎá ÇáäÙÇã.");

            builder.Property(x => x.Description)
                .HasMaxLength(1000)
                .HasComment("æÕİ ÊİÕíáí ááŞÇÚÉ íÔãá ÇáããíÒÇÊ æÇáÎÏãÇÊ æÇáÊÌåíÒÇÊ ÇáãÊæİÑÉ.");

            builder.Property(x => x.Capacity)
                .IsRequired()
                .HasComment("ÇáÚÏÏ ÇáÃŞÕì ááÃÔÎÇÕ ÇáãÓãæÍ ÈÇÓÊŞÈÇáåã ÏÇÎá ÇáŞÇÚÉ.");

            builder.Property(x => x.PricePerHour)
                .HasColumnType("decimal(18,2)")
                .IsRequired()
                .HasComment("ÊßáİÉ ÇÓÊÆÌÇÑ ÇáŞÇÚÉ áßá ÓÇÚÉ ãÍÓæÈÉ ÈÇáÚãáÉ ÇáãÚÊãÏÉ İí ÇáäÙÇã.");

            builder.Property(x => x.Location)
                .HasMaxLength(500)
                .HasComment("ÇáãæŞÚ ÇáÌÛÑÇİí Ãæ ÇáÚäæÇä ÇáÊİÕíáí ááŞÇÚÉ áÊÓåíá ÇáæÕæá ÅáíåÇ.");

            builder.Property(x => x.Status)
                .IsRequired()
                .HasComment("ÇáÍÇáÉ ÇáÊÔÛíáíÉ ÇáÍÇáíÉ ááŞÇÚÉ ãËá ãÊÇÍÉ Ãæ ãÍÌæÒÉ Ãæ ÊÍÊ ÇáÕíÇäÉ.");

            builder.Property(x => x.Type)
                .IsRequired()
                .HasComment("ÊÕäíİ ÇáŞÇÚÉ ÍÓÈ äæÚ ÇáãäÇÓÈÉ ãËá ÃÚÑÇÓ Ãæ ãÄÊãÑÇÊ Ãæ ÇÌÊãÇÚÇÊ.");
        }
    }
}