using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Configurations
{
    internal class UserInterestProfileConfiguration : IEntityTypeConfiguration<UserInterestProfile>
    {
        public void Configure(EntityTypeBuilder<UserInterestProfile> builder)
        {
            builder.ToTable("UserInterestProfiles");

            builder.HasKey(X => X.Id);


            // InterestScore: درجة اهتمام المستخدم (رقم عشري)
            builder.Property(X => X.InterestScore)
                   .IsRequired()
                   .HasColumnType("decimal(18,2)")
                   .HasDefaultValue(0);

            builder.Property(X => X.LastUpdated)
                   .IsRequired();

            // الأعمدة الاختيارية (Nullable)
            // بما أنها Guid? فالـ EF Core يفهم تلقائياً أنها تقبل Null
            builder.Property(X => X.CategoryId).IsRequired(false);
            builder.Property(X => X.TagId).IsRequired(false);

            
            // نضع Index لتسريع عملية البحث عن اهتمامات مستخدم معين بتصنيف معين
            builder.HasIndex(X => X.UserId);
            builder.HasIndex(X => X.CategoryId);
            builder.HasIndex(X => X.TagId);

            // Relationships

            // العلاقة مع User
            builder.HasOne(X => X.User)
                   .WithMany() // المستخدم لديه اهتمامات كثيرة
                   .HasForeignKey(X => X.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
