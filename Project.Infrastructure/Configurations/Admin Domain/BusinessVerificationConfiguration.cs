using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Core.Domain.Entities;

namespace Project.Infrastructure.Configuration
{
    internal class BusinessVerificationConfiguration : IEntityTypeConfiguration<BusinessVerification>
    {
        public void Configure(EntityTypeBuilder<BusinessVerification> builder)
        {
            // Primary Key
            builder.HasKey(X => X.Id);

            // Properties
            builder.Property(X => X.CommercialRegImage).IsRequired().HasMaxLength(500);
            builder.Property(X => X.TaxCardImage).IsRequired().HasMaxLength(500);
            builder.Property(X => X.IdentityCardImage).IsRequired().HasMaxLength(500);

            builder.Property(X => X.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(X => X.RejectionReason).HasMaxLength(1000).IsRequired(false);
            builder.Property(X => X.SubmittedAt).IsRequired();

            // ⚠️ هام: جعلنا تاريخ المراجعة يقبل Null لأن الطلب الجديد لم يراجع بعد
            builder.Property(X => X.ReviewedAt).IsRequired(false);

            // Relationships Configuration

            // 1. علاقة الأدمن (One-to-Many)
            // جعلنا العلاقة اختيارية (IsRequired(false)) لأن الطلب في البداية ليس له أدمن
            builder.HasOne(X => X.Admin)
                .WithMany(A => A.BusinessVerifications)
                .HasForeignKey(X => X.AdminId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // 2. علاقة البيزنس (One-to-One) - 🔴 هنا كان الخطأ وتم إصلاحه
            builder.HasOne(X => X.Business)
                   .WithOne(b => b.BusinessVerifications) // 👈 تم التغيير من WithMany إلى WithOne
                   .HasForeignKey<BusinessVerification>(X => X.BusinessId) // Foreign Key في جدول الـ Verification
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}