using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Configuration
{
    internal class BusinessPlanConfiguration : IEntityTypeConfiguration<BusinessPlan>
    {
       
        public void Configure(EntityTypeBuilder<BusinessPlan> builder)
        {
            // Primary Key
            builder.HasKey(x => x.Id);

            // Properties Configuration
            builder.Property(x => x.StartDate)
               .IsRequired();

            builder.Property(x => x.EndDate)
                   .IsRequired();

            // تحويل الـ Enum إلى String لتخزينه كنص في قاعدة البيانات (Active, Expired, etc.)
            // هذا يوافق الـ ERD الذي يحدد النوع بـ NVARCHAR(20)
            builder.Property(x => x.Status)
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .IsRequired();

            // Relationships Configuration
            builder.HasOne(x => x.Business)
               .WithMany() 
               .HasForeignKey(x => x.BusinessId) 
               .OnDelete(DeleteBehavior.Restrict); // لمنع حذف الشركة إذا كان لديها خطط

            
            builder.HasOne(x => x.SubscriptionPlan)
                   .WithMany(x => x.BusinessPlans) 
                   .HasForeignKey(x => x.PlanId) 
                   .OnDelete(DeleteBehavior.Restrict);

            
            builder.HasMany(x => x.Payments)
                   .WithOne() 
                   .HasForeignKey("BusinessPlanId") 
                   .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
