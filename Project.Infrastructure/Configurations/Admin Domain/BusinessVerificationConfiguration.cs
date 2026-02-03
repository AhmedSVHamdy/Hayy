using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Configuration
{
    internal class BusinessVerificationConfiguration : IEntityTypeConfiguration<BusinessVerification>
    {
        public void Configure(EntityTypeBuilder<BusinessVerification> builder)
        {
            // Primary Key
            builder.HasKey(X => X.Id);

            // Properties Configuration

            builder.Property(X => X.CommercialRegImage)
            .IsRequired()
            .HasMaxLength(500); // تخزين مسار الصورة

            builder.Property(X => X.TaxCardImage)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(X => X.IdentityCardImage)
                .IsRequired()
                .HasMaxLength(500);

            // تحويل الـ Enum الخاص بالحالة إلى نص عند التخزين
            builder.Property(X => X.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(X => X.RejectionReason)
                .HasMaxLength(1000)
                .IsRequired(false); // فاضي لو اتقبل

            builder.Property(X => X.SubmittedAt)
                .IsRequired();

            builder.Property(X => X.ReviewedAt)
                .IsRequired(false); // فاضي لو لسه مراجعش   


            // Relationships Configuration
            builder.HasOne(X => X.Admin)
                .WithMany(A => A.BusinessVerifications)
                .HasForeignKey(X => X.AdminId)
                .OnDelete(DeleteBehavior.Cascade); // لو اتشال الادمن، كل الفيريفيكيشن بتاعته تتشال 

            builder.HasOne(X => X.Business)
                   .WithMany()
                   .HasForeignKey(X => X.BusinessId)
                   .OnDelete(DeleteBehavior.Cascade); // لو اتشال البيزنس، كل الفيريفيكيشن بتاعته تتشال
        }
    }
}
