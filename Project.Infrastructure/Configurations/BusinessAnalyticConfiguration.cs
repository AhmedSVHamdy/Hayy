using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Configuration
{
    internal class BusinessAnalyticConfiguration : IEntityTypeConfiguration<BusinessAnalytic>
    {
        public void Configure(EntityTypeBuilder<BusinessAnalytic> builder)
        {
            // Primary Key
            builder.HasKey(X => X.Id);

            // Properties Configuration
            builder.Property(X => X.TotalViews)
               .IsRequired()
               .HasDefaultValue(0);

            builder.Property(X => X.TotalFollowers)
                   .IsRequired()
                   .HasDefaultValue(0);

            builder.Property(X => X.TotalReviews)
                   .IsRequired()
                   .HasDefaultValue(0);

            builder.Property(X => X.AvgRating)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired()
                   .HasDefaultValue(0);

            builder.Property(X => X.MonthlyRevenue)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired()
                   .HasDefaultValue(0);

            //  التواريخ
            builder.Property(X => X.LastUpdated)
                   .IsRequired();

            // Relationships Configuration
            builder.HasOne(X => X.Business)
               .WithMany() 
               .HasForeignKey(X => X.BusinessId)
               .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
