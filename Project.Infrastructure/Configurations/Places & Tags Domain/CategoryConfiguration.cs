using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Configuration
{
    internal class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasKey(X => X.Id);

            // 3. إعدادات الخصائص
            builder.Property(X => X.Name)
                   .IsRequired()
                   .HasMaxLength(100); 

            builder.Property(X => X.ImageUrl)
                   .HasMaxLength(500) 
                   .IsRequired(false); 

            // Relationships

            
            builder.HasMany(X => X.Places)
                   .WithOne(X => X.Category)
                   .HasForeignKey(X => X.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict); // يمنع حذف التصنيف إذا كان مستخدماً من قبل أماكن

            // العلاقة مع CategoryTags (جدول الربط مع الوسوم)
            builder.HasMany(X => X.CategoryTags)
                   .WithOne(X => X.Category)
                   .HasForeignKey(X => X.CategoryId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
