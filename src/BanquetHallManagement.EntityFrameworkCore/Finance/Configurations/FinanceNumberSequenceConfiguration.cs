using BanquetHallManagement.Finance.Sequences;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace BanquetHallManagement.EntityFrameworkCore.Finance.Configurations;

public class FinanceNumberSequenceConfiguration : IEntityTypeConfiguration<FinanceNumberSequence>
{
    public void Configure(EntityTypeBuilder<FinanceNumberSequence> builder)
    {
        builder.ToTable("FinanceNumberSequences", t =>
        {
            t.HasComment("جدول تسلسل أرقام المستندات المالية.");
        });

        builder.Property(x => x.Prefix)
            .IsRequired()
            .HasMaxLength(10)
            .HasComment("بادئة الرقم مثل JE أو RC أو INV.");

        builder.Property(x => x.Year)
            .IsRequired()
            .HasComment("السنة المرتبطة بالتسلسل.");

        builder.Property(x => x.LastNumber)
            .IsRequired()
            .HasComment("آخر رقم تم إصداره.");

        builder.HasIndex(x => new { x.Prefix, x.Year }).IsUnique();

        builder.ConfigureByConvention();
    }
}
