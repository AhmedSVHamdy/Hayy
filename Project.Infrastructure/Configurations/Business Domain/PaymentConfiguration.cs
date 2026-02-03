using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Configuration
{
    internal class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            // Primary Key
            builder.HasKey(x => x.Id);
            // Properties Configuration
            builder.Property(x => x.Amount)
               .IsRequired()
               .HasColumnType("decimal(18,2)");

            builder.Property(x => x.Method)
               .HasConversion<string>()
               .HasMaxLength(50)
               .IsRequired();

            builder.Property(x => x.TransactionId)
               .HasMaxLength(100)
               .IsRequired();

            // Relationships Configuration
            builder.HasOne(x => x.BusinessPlan)
               .WithMany(x => x.Payments) // الربط مع القائمة الموجودة في BusinessPlan
               .HasForeignKey(x => x.BusinessPlanId) // تحديد مفتاح الربط الصريح
               .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
