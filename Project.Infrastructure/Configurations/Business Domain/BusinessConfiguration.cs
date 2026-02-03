using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Configuration
{
    internal class BusinessConfiguration : IEntityTypeConfiguration<Business>
    {
        public void Configure(EntityTypeBuilder<Business> builder)
        {
            // Primary Key
            builder.HasKey(x => x.Id);

            // Properties Configuration
            builder.Property(x => x.BrandName)
            .IsRequired()
            .HasMaxLength(100);

            builder.Property(x => x.LegalName)
                .IsRequired()
                .HasMaxLength(150); 

            builder.Property(x => x.CommercialRegNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.TaxNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.LogoImage)
                .HasMaxLength(500)
                .IsRequired(false);

            
            builder.Property(x => x.VerificationStatus)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            // لضمان عدم تكرار السجل التجاري أو البطاقة الضريبية بين شركتين مختلفتين
            builder.HasIndex(x => x.CommercialRegNumber).IsUnique();
            builder.HasIndex(x => x.TaxNumber).IsUnique();


            // Relationships Configuration
            builder.HasOne(x => x.User)
            .WithOne() // أو WithMany إذا كان المستخدم الواحد يملك أكثر من شركة
            .HasForeignKey<Business>(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

            // علاقة الشركة بالفروع/الأماكن (One-to-Many)
            builder.HasMany(x => x.Places)
                .WithOne(p => p.Business)
                .HasForeignKey(p => p.BusinessId)
                .OnDelete(DeleteBehavior.Cascade); // حذف الشركة يحذف فروعها

            // علاقة الشركة BusinessPlans (One-to-Many)
            builder.HasMany(x => x.BusinessPlans)
                .WithOne() // افترضت أن BusinessPlan لديه BusinessId
                .HasForeignKey("BusinessId");

            // علاقة الشركة بBusinessAnalytics (One-to-Zero-or-One)
            builder.HasOne(x => x.BusinessAnalytics)
                .WithOne(ba => ba.Business)
                .HasForeignKey<BusinessAnalytic>(ba => ba.BusinessId);

            // علاقة الشركة بBusinessVerifications (One-to-Zero-or-One)
            
            builder.HasOne(x => x.BusinessVerifications)
                .WithOne(bv => bv.Business)
                .HasForeignKey<BusinessVerification>(bv => bv.BusinessId);
        }
    }
}
