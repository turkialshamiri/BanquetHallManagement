using BanquetHallManagement.Entities;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace BanquetHallManagement.Configurations.HallConfigurations
{
    public class HallConfiguration : IEntityTypeConfiguration<Hall>
    {
        public void Configure(EntityTypeBuilder<Hall> builder)
        {
            builder.ToTable("Halls", t =>
            {
                t.HasComment("جدول يحتوي على بيانات القاعات المتاحة للحجز داخل النظام.");
            });

            builder.Property(x => x.Name)
                  .IsRequired()
                  .HasMaxLength(200)
                  .HasComment("الاسم الرسمي للقاعة المستخدم في عمليات العرض والبحث والحجز داخل النظام.");

            builder.Property(x => x.Description)
                .HasMaxLength(1000)
                .HasComment("وصف تفصيلي للقاعة يشمل المميزات والخدمات والتجهيزات المتوفرة.");

            builder.Property(x => x.Capacity)
                .IsRequired()
                .HasComment("العدد الأقصى للأشخاص المسموح باستقبالهم داخل القاعة.");

            builder.Property(x => x.PricePerHour)
                .HasColumnType("decimal(18,2)")
                .IsRequired()
                .HasComment("تكلفة استئجار القاعة لكل ساعة محسوبة بالعملة المعتمدة في النظام.");

            builder.Property(x => x.Location)
                .HasMaxLength(500)
                .HasComment("الموقع الجغرافي أو العنوان التفصيلي للقاعة لتسهيل الوصول إليها.");

            builder.Property(x => x.Status)
                .IsRequired()
                .HasComment("الحالة التشغيلية الحالية للقاعة مثل متاحة أو محجوزة أو تحت الصيانة.");

            builder.Property(x => x.Type)
                .IsRequired()
                .HasComment("تصنيف القاعة حسب نوع المناسبة مثل أعراس أو مؤتمرات أو اجتماعات.");
        }
    }
}