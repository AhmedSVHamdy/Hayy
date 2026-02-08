using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Core.Domain.Entities;

namespace Project.Infrastructure.Configuration
{
    internal class BusinessConfiguration : IEntityTypeConfiguration<Business>
    {
        public void Configure(EntityTypeBuilder<Business> builder)
        {
            // ... (باقي الإعدادات كما هي: Id, BrandName, etc) ...
            builder.HasKey(x => x.Id);
            builder.Property(x => x.BrandName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.LegalName).IsRequired().HasMaxLength(150);
            builder.Property(x => x.CommercialRegNumber).IsRequired().HasMaxLength(50);
            builder.Property(x => x.TaxNumber).IsRequired().HasMaxLength(50);
            builder.Property(x => x.LogoImage).HasMaxLength(500).IsRequired(false);
            builder.Property(x => x.VerificationStatus).HasConversion<string>().HasMaxLength(50);
            builder.HasIndex(x => x.CommercialRegNumber).IsUnique();
            builder.HasIndex(x => x.TaxNumber).IsUnique();

            // --- العلاقات (Relationships) ---

            // 1. علاقة المستخدم (User)
            builder.HasOne(x => x.User)
                   .WithOne()
                   .HasForeignKey<Business>(x => x.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            // 2. علاقة الأماكن (Places)
            builder.HasMany(x => x.Places)
                   .WithOne(p => p.Business)
                   .HasForeignKey(p => p.BusinessId)
                   .OnDelete(DeleteBehavior.Cascade);

            // 3. علاقة خطط الأسعار (BusinessPlans)
            builder.HasMany(x => x.BusinessPlans)
                   .WithOne()
                   .HasForeignKey("BusinessId"); // تأكد أن BusinessPlan يحتوي على BusinessId

            // 4. علاقة التحليلات (BusinessAnalytics)
            builder.HasOne(x => x.BusinessAnalytics)
                   .WithOne(ba => ba.Business)
                   .HasForeignKey<BusinessAnalytic>(ba => ba.BusinessId);

            // 👇👇 5. علاقة التوثيق (التعديل الهام هنا) 👇👇
            // العلاقة أصبحت: Business لديه Many Verifications
            builder.HasMany(x => x.Verifications) // لاحظ الاسم Verifications (القائمة)
                   .WithOne(v => v.Business)      // الـ Verification الواحد يتبع Business واحد
                   .HasForeignKey(v => v.BusinessId)
                   .OnDelete(DeleteBehavior.Cascade); // لو حذفنا البيزنس، نحذف سجلات توثيقه
        }
    }
}