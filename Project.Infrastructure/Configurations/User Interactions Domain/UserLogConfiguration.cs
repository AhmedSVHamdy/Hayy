using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Configurations
{
    internal class UserLogConfiguration : IEntityTypeConfiguration<UserLog>
    {
        public void Configure(EntityTypeBuilder<UserLog> builder)
        {
            builder.HasKey(X => X.Id);


            builder.Property(X => X.ActionType)
                   .HasConversion<string>()
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(X => X.TargetType)
                   .HasConversion<string>()
                   .HasMaxLength(50)
                   .IsRequired();

            // Nullable Guids
            
            builder.Property(X => X.TargetId).IsRequired(false);
            builder.Property(X => X.CategoryId).IsRequired(false);
            builder.Property(X => X.TagId).IsRequired(false);

            // مدة النشاط
            builder.Property(X => X.Duration)
                   .IsRequired()
                   .HasDefaultValue(0);

            builder.Property(X => X.SearchQuery)
                   .HasMaxLength(500) 
                   .IsRequired(false);

            // تاريخ الإنشاء
            builder.Property(X => X.CreatedAt)
                   .IsRequired();

            // Relationships

            //  User
            builder.HasOne(X => X.User)
                   .WithMany() 
                   .HasForeignKey(X => X.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
