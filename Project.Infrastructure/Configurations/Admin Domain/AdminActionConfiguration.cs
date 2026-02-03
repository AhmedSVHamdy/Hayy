using Microsoft.EntityFrameworkCore;
using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Configuration
{
    internal class AdminActionConfiguration : IEntityTypeConfiguration<AdminAction>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<AdminAction> builder)
        {
            // Primary Key
            builder.HasKey(X => X.Id);

            // Properties Configuration
            builder.Property(X => X.ActionType)
            .HasConversion<string>()
            .HasMaxLength(50)
           .IsRequired(); 

        builder.Property(X => X.TargetType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired(); 

        builder.Property(X => X.TargetId)
               .IsRequired(); 

        builder.Property(X => X.Notes)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(X => X.CreatedAt)
                .IsRequired();

            // Relationships Configuration
            builder.HasOne(X => X.Admin)
                .WithMany()
                .HasForeignKey(X => X.AdminId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
