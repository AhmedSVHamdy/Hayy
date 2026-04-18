using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Core.Domain.Entities;
using System;

namespace Project.Infrastructure.Configurations.User_Domain
{
    internal class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // 1. الحفاظ على اسم الجدول كما هو في قاعدة بياناتك
            builder.ToTable("Users");

            // 2. إرجاع إعدادات Identity الأساسية اللي مابقيناش عايزين نغيرها
            builder.Property(X => X.Email).IsRequired().HasMaxLength(150);
            builder.HasIndex(X => X.Email).IsUnique();
            builder.Property(X => X.PasswordHash).IsRequired();

            // 3. باقي الخصائص
            builder.Property(X => X.FullName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(X => X.UserType)
                   .HasMaxLength(50)
                   .IsRequired(); // شلنا HasConversion<string> لأنك في الـ User عاملها string مش Enum

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

            // Refresh Tokens
            builder.Property(x => x.RefreshToken)
                   .HasMaxLength(500)
                   .IsRequired(false);

            builder.Property(x => x.RefreshTokenExpirationDateTime)
                   .IsRequired(true);

            // 4. العلاقات (التي تم تصحيحها لعدم تكرار UserId)
            builder.HasOne(X => X.UserSettings)
                   .WithOne(X => X.User)
                   .HasForeignKey<UserSettings>(X => X.UserId)
                   .OnDelete(DeleteBehavior.Cascade); // لو اتحذف اليوزر تتحذف إعداداته

            builder.HasOne(x => x.Business)
                   .WithOne(b => b.User)
                   .HasForeignKey<Business>(b => b.UserId)
                   .OnDelete(DeleteBehavior.Restrict); // من الأفضل منع الحذف المتتالي عشان الفواتير والبيانات
        }
    }
}
