using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Configurations
{
    internal class PlaceFollowConfiguration : IEntityTypeConfiguration<PlaceFollow>
    {
        public void Configure(EntityTypeBuilder<PlaceFollow> builder)
        {
            builder.HasKey(X => X.Id);

            // Composite Unique Index
            builder.HasIndex(X => new { X.UserId, X.PlaceId }).IsUnique();

            builder.Property(X => X.CreatedAt)
                   .IsRequired();

            // Relationships

            // العلاقة مع Place
            builder.HasOne(X => X.Place)
                   .WithMany(X => X.PlaceFollows) 
                   .HasForeignKey(X => X.PlaceId)
                   .OnDelete(DeleteBehavior.Cascade); // إذا حُذف المكان، تُحذف كل المتابعات الخاصة به

            // العلاقة مع  User
            builder.HasOne(X => X.User)
                   .WithMany() // أو WithMany(u => u.PlaceFollows) إذا كانت موجودة في User
                   .HasForeignKey(X => X.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
