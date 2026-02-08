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


            // 2. علاقة البيزنس (One-to-One) - 🔴 هنا كان الخطأ وتم إصلاحه
            // 👇👇 2. علاقة البيزنس (تم التصحيح) 👇👇
            builder.HasOne(X => X.Business)
                   .WithMany(b => b.Verifications) // 👈 نربطها بالقائمة Verifications الموجودة في Business
                   .HasForeignKey(X => X.BusinessId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}