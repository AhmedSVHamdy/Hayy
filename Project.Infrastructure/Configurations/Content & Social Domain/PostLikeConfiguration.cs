using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Configurations
{
    internal class PostLikeConfiguration : IEntityTypeConfiguration<PostLike>
    {
        public void Configure(EntityTypeBuilder<PostLike> builder)
        {
            builder.ToTable("PostLikes");

            builder.HasKey(X => X.Id);

            builder.HasIndex(X => new { X.UserId, X.PostId }).IsUnique();

            builder.HasOne(X => X.Post)
                   .WithMany(X => X.PostLikes) 
                   .HasForeignKey(X => X.PostId)
                   .OnDelete(DeleteBehavior.Cascade);

            // 2. العلاقة مع User
            builder.HasOne(X => X.User)
                   .WithMany()
                   .HasForeignKey(X => X.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }  
}
