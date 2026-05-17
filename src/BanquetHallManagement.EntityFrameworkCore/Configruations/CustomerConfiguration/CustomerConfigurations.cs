using BanquetHallManagement.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace BanquetHallManagement.Configurations.CustomerConfigurations
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("Customers", t =>
            {
                t.HasComment("جدول يحتوي على بيانات العملاء المستخدمين لنظام الحجز.");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200)
                .HasComment("الاسم الكامل للعميل.");

            builder.Property(x => x.Phone)
                .IsRequired()
                .HasMaxLength(20)
                .HasComment("رقم الهاتف المستخدم للتواصل مع العميل وإرسال الإشعارات.");

            builder.Property(x => x.Company)
                .HasMaxLength(300)
                .HasComment("اسم الشركة أو الجهة التابعة للعميل إن وجدت.");

            builder.HasIndex(x => x.Phone)
                .IsUnique();

            builder.ConfigureByConvention();
        }
    }
}