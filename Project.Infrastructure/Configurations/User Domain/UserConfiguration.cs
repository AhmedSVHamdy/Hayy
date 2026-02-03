using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Configurations.User_Domain
{
    internal class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            // 2. المفتاح الأساسي
            builder.HasKey(X => X.Id);

            // 3. إعدادات الخصائص
            builder.Property(X => X.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(X => X.Email)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.HasIndex(X => X.Email).IsUnique();

            builder.Property(X => X.Password)
                   .IsRequired(); // تخزن هنا الـ Hash وليس الباسورد الصريح

            builder.Property(X => X.UserType)
                   .HasConversion<string>()
                   .HasMaxLength(50)
                   .IsRequired();

            //Nullable Strings
            builder.Property(X => X.ProfileImage)
                   .HasMaxLength(500)
                   .IsRequired(false);

            builder.Property(X => X.City)
                   .HasMaxLength(100)
                   .IsRequired(false);

            builder.Property(X => X.IsVerified)
                   .HasDefaultValue(false);

            builder.Property(X => X.CreatedAt)
                   .IsRequired();

            // Relationships

            // علاقة 1:1 مع إعدادات المستخدم (User -> UserSettings)
            builder.HasOne(X => X.UserSettings)
                   .WithOne(X => X.User)
                   .HasForeignKey<UserSettings>(X => X.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
