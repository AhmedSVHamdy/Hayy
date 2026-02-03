using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Configurations
{
    internal class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.ToTable("Reviews");

            builder.HasKey(X => X.Id);

            builder.Property(X => X.Rating)
                   .IsRequired(); // التقييم إجباري

            builder.Property(X => X.Comment)
                   .HasMaxLength(1000) 
                   .IsRequired(false); // مسموح أن يكون فارغاً (تقييم بالنجوم فقط)

            // الصور تخزن كنص (روابط مفصولة بفاصلة أو JSON)
            builder.Property(X => X.ReviewImages)
                   .HasMaxLength(2000)
                   .IsRequired(false);

            builder.Property(X => X.CreatedAt)
                   .IsRequired();

            // Relationships

            // العلاقة مع  Place
            builder.HasOne(X => X.Place)
                   .WithMany(X => X.Reviews) 
                   .HasForeignKey(X => X.PlaceId)
                   .OnDelete(DeleteBehavior.Cascade); 

            //  العلاقة معUser
            builder.HasOne(X => X.User)
                   .WithMany() 
                   .HasForeignKey(X => X.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
