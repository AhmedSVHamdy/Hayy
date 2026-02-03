using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Configurations
{
    internal class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.HasKey(X => X.Id);

            builder.Property(X => X.Title)
                   .IsRequired()
                   .HasMaxLength(200); 

            builder.Property(X => X.Message)
                   .IsRequired()
                   .HasMaxLength(500); // محتوى الإشعار

            builder.Property(X => X.Type)
                   .HasConversion<string>()
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(X => X.ReferenceType)
                   .HasConversion<string>()
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(X => X.ReferenceId)
                   .IsRequired(false);

            // Payload: بيانات إضافية (JSON عادةً)، يمكن أن تكون Null
            builder.Property(X => X.Payload)
                   .IsRequired(false);

            builder.Property(X => X.IsRead)
                   .HasDefaultValue(false); 

            builder.Property(X => X.CreatedAt)
                   .IsRequired();

            // Relationships

            // العلاقة مع  User
            builder.HasOne(X => X.User)
                   .WithMany() // المستخدم لديه إشعارات كثيرة، عادة لا نضع قائمة Notifications في كلاس User لتخفيف الحمل
                   .HasForeignKey(X => X.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
