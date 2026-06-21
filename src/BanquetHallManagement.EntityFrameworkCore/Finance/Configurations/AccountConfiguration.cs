using BanquetHallManagement.Finance.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace BanquetHallManagement.EntityFrameworkCore.Finance.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("FinanceAccounts", t =>
        {
            t.HasComment("جدول شجرة الحسابات المالية.");
        });

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(20)
            .HasComment("رمز الحساب مثل 1100.");

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("اسم الحساب.");

        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasComment("نوع الحساب: أصل أو التزام أو إيراد.");

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true)
            .HasComment("هل الحساب نشط؟");

        builder.HasIndex(x => x.Code).IsUnique();

        builder.ConfigureByConvention();
    }
}
