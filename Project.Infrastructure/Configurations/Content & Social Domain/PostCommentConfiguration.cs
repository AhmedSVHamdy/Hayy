using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Configurations
{
    internal class PostCommentConfiguration : IEntityTypeConfiguration<PostComment>
    {
        public void Configure(EntityTypeBuilder<PostComment> builder)
        {
            builder.ToTable("PostComments");

            builder.HasKey(X => X.Id);

            builder.Property(X => X.Content)
                   .IsRequired()
                   .HasMaxLength(1000);

            builder.Property(X => X.CreatedAt)
                   .IsRequired();

            
            builder.HasOne(X => X.Post)
                   .WithMany(X => X.PostComments)
                   .HasForeignKey(X => X.PostId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(X => X.User)
                   .WithMany()
                   .HasForeignKey(X => X.UserId)
                   .OnDelete(DeleteBehavior.Cascade); 

            builder.HasOne(X => X.ParentComment)
                   .WithMany(X => X.Replies)
                   .HasForeignKey(X => X.ParentCommentId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
}
}
