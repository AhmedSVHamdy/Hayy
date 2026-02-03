using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Configuration
{
    internal class TagConfiguration : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            // 1. اسم الجدول
            builder.ToTable("Tags");

            builder.HasKey(X => X.Id);

            
            builder.Property(X => X.Name)
                   .IsRequired()
                   .HasMaxLength(100); 

            builder.Property(X => X.ImageUrl)
                   .HasMaxLength(500)
                   .IsRequired(false);

            // Relationships

            builder.HasMany(X => X.CategoryTags)
                   .WithOne(X => X.Tag)
                   .HasForeignKey(X => X.TagId)
                   .OnDelete(DeleteBehavior.Cascade); 

            builder.HasMany(X => X.PlaceTags)
                   .WithOne(X => X.Tag)
                   .HasForeignKey(X => X.TagId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
