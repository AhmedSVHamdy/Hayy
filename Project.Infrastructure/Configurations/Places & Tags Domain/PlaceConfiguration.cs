using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Configuration
{
    internal class PlaceConfiguration : IEntityTypeConfiguration<Place>
    {
        public void Configure(EntityTypeBuilder<Place> builder)
        {
            // Primary Key
            builder.HasKey(X => X.Id);

            // النصوص
            builder.Property(X => X.Name).IsRequired().HasMaxLength(150); 
            builder.Property(X => X.Description).HasMaxLength(1000); 
            builder.Property(X => X.Governorate).HasMaxLength(100);
            builder.Property(X => X.District).HasMaxLength(100);
            builder.Property(X => X.StreetAddress).HasMaxLength(255);
            builder.Property(X => X.City).HasMaxLength(100);
            builder.Property(X => X.PhoneNumber).HasMaxLength(20);


            // الصور (غالباً روابط أو JSON)
            builder.Property(X => X.GalleryImages).IsRequired(false);
            builder.Property(X => X.CoverImage).IsRequired(false);

            // الإحداثيات (Dqecimal للخرائط يتطلب دقة 9,6 عادةً)
            builder.Property(X => X.Latitude).HasColumnType("decimal(9,6)");
            builder.Property(X => X.Longitude).HasColumnType("decimal(9,6)");

            // التقييمات
            builder.Property(X => X.AvgRating).HasColumnType("decimal(3,2)").HasDefaultValue(0);
            builder.Property(X => X.TotalReviews).HasDefaultValue(0);

            builder.Property(X => X.IsActive).HasDefaultValue(true);

            // العلاقات
            builder.HasOne(X => X.Business)
                   .WithMany() // إذا كان للـ Business قائمة Places، ضعها هنا: WithMany(b => b.Places)
                   .HasForeignKey(X => X.BusinessId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(X => X.Category)
                   .WithMany(X => X.Places)
                   .HasForeignKey(X => X.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
