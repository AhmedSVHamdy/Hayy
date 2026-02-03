using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Configurations
{
    public class BusinessPostConfiguration : IEntityTypeConfiguration<BusinessPost>
    {
        public void Configure(EntityTypeBuilder<BusinessPost> builder)
        {
            builder.ToTable("BusinessPosts");

            builder.HasKey(X => X.Id);

            builder.Property(X => X.Content)
                   .IsRequired()
                   .HasMaxLength(2000);

            builder.Property(X => X.PostAttachments)
                   .HasMaxLength(2000)
                   .IsRequired(false);

            builder.Property(X => X.CreatedAt)
                   .IsRequired();

            // العلاقات
            builder.HasOne(X => X.Place)
                   .WithMany(X => X.BusinessPosts)
                   .HasForeignKey(X => X.PlaceId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(X => X.PostLikes)      
               .WithOne(X => X.Post)           
               .HasForeignKey(X => X.PostId)   
               .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(X => X.PostComments)
               .WithOne(X => X.Post)
               .HasForeignKey(X => X.PostId)
               .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
