using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Configurations.User_Domain
{
    internal class UserSettingsConfiguration : IEntityTypeConfiguration<UserSettings>
    {
        public void Configure(EntityTypeBuilder<UserSettings> builder)
        {
            builder.ToTable("UserSettings");

            builder.HasKey(X => X.Id);

            builder.Property(X => X.EmailNotifications)
                   .HasDefaultValue(true); // افتراضياً تفعيل الإشعارات

            builder.Property(X => X.NotificationsEnabled)
                   .HasDefaultValue(true);

            // العلاقات
            // تم تعريف العلاقة في UserConfiguration، ولكن للتأكيد من الطرفين:
            builder.HasOne(X => X.User)
                   .WithOne(X => X.UserSettings)
                   .HasForeignKey<UserSettings>(X => X.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
