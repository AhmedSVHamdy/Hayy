using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Configuration
{
    internal class AdminConfiguration : IEntityTypeConfiguration<Admin>
    {
        public void Configure(EntityTypeBuilder<Admin> builder)
        {
            //Primary Key
            builder.HasKey(X => X.Id); //

            // Properties Configuration
            builder.Property(X => X.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(X => X.Email)
                .IsRequired()
                .HasMaxLength(255); 

            builder.Property(X => X.Password)
                .IsRequired();

            builder.Property(X => X.ProfileImage)
                .IsRequired(false);    // مش لازم صورة

            builder.Property(X => X.CreatedAt)
                .IsRequired();
            // Relationships Configuration

            builder.HasMany(X => X.BusinessVerifications)
                .WithOne()
                .HasForeignKey(X=>X.AdminId)
                .OnDelete(DeleteBehavior.Cascade); // 

            builder.HasIndex(X => X.Email).IsUnique(); // الايميل مينفعش يتكرر
        }
    }
}
