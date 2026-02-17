using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Configuration
{
    internal class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
    {
        public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
        {
            // Primary Key
           builder.HasKey(X => X.Id);

            // Properties Configuration
            builder.Property(X => X.Name)
                .IsRequired()
                .HasMaxLength(100);

           builder.Property(X => X.Price)
            .HasColumnType("decimal(18,2)") 
            .IsRequired();

            builder.Property(X => X.DurationDays)
                .IsRequired();

            builder.Property(X => X.Description)
                   .HasMaxLength(500) 
                   .IsRequired(false);

            builder.Property(x => x.AiPowerLevel)
               .IsRequired()
               .HasDefaultValue(0);

            builder.Property(x => x.IsActive)
                   .HasDefaultValue(true);

            // Relationships Configuration
            builder.HasMany(x => x.BusinessPlans)
               .WithOne()
               .HasForeignKey("SubscriptionPlanId")
               .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
