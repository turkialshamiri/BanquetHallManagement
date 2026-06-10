using BanquetHallManagement.Finance.Accounts;
using BanquetHallManagement.Finance.JournalEntries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace BanquetHallManagement.Configurations.Finance;

public class JournalEntryLineConfiguration : IEntityTypeConfiguration<JournalEntryLine>
{
    public void Configure(EntityTypeBuilder<JournalEntryLine> builder)
    {
        builder.ToTable("JournalEntryLines", t =>
        {
            t.HasComment("جدول بنود القيود المحاسبية.");
        });

        builder.Property(x => x.JournalEntryId)
            .IsRequired()
            .HasComment("معرف القيد المحاسبي.");

        builder.Property(x => x.AccountId)
            .IsRequired()
            .HasComment("معرف الحساب المرتبط بالبند.");

        builder.Property(x => x.Debit)
            .HasColumnType("decimal(18,2)")
            .IsRequired()
            .HasComment("مبلغ المدين.");

        builder.Property(x => x.Credit)
            .HasColumnType("decimal(18,2)")
            .IsRequired()
            .HasComment("مبلغ الدائن.");

        builder.Property(x => x.Description)
            .HasMaxLength(500)
            .HasComment("وصف البند.");

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.JournalEntryId);
        builder.HasIndex(x => x.AccountId);

        builder.ConfigureByConvention();
    }
}
