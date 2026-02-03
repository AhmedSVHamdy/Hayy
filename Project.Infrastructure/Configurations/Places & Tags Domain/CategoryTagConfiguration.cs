using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Configuration
{
    internal class CategoryTagConfiguration : IEntityTypeConfiguration<CategoryTag>
    {
        public void Configure(EntityTypeBuilder<CategoryTag> builder)
        {
            // اسم الجدول
            builder.ToTable("CategoryTags");

          
            builder.HasKey(X => X.Id);

          
            builder.HasIndex(X => new { X.CategoryId, X.TagId }).IsUnique();

           

            builder.HasOne(X => X.Category)
                   .WithMany(X => X.CategoryTags) 
                   .HasForeignKey(X => X.CategoryId)
                   .OnDelete(DeleteBehavior.Cascade); 

          
            builder.HasOne(X => X.Tag)
                   .WithMany(X => X.CategoryTags) 
                   .HasForeignKey(X => X.TagId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
