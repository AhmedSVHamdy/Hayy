using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Configurations
{
    internal class RecommendedItemConfiguration : IEntityTypeConfiguration<RecommendedItem>
    {
        public void Configure(EntityTypeBuilder<RecommendedItem> builder)
        {
            builder.ToTable("RecommendedItems");

            builder.HasKey(X => X.Id);


            builder.Property(X => X.ItemType)
                   .HasConversion<string>()
                   .HasMaxLength(50)
                   .IsRequired();

            // لا يوجد علاقة FK صريحة هنا لأنه متعدد الأشكال (Polymorphic Reference)
            builder.Property(X => X.ItemId)
                   .IsRequired();

            // Score: درجة الترشيح (دقة عشرية)
            builder.Property(X => X.Score)
                   .IsRequired()
                   .HasColumnType("decimal(18,2)") 
                   .HasDefaultValue(0);

            builder.Property(X => X.CreatedAt)
                   .IsRequired();

            builder.Property(X => X.UpdatedAt)
                   .IsRequired();

            //  Indexes - مهمة جداً للأداء

            // تسريع البحث عن توصيات مستخدم معين
            builder.HasIndex(X => X.UserId);

            // تسريع البحث عن نوع معين من التوصيات (مثلاً: هات لي كل المطاعم المقترحة)
            builder.HasIndex(X => new { X.ItemType, X.ItemId });

            // Relationships

            // العلاقة مع User
            builder.HasOne(X => X.User)
                   .WithMany() 
                   .HasForeignKey(X => X.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
