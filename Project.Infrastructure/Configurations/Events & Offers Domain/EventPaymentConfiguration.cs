using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Configurations.Events___Offers_Domain
{
    public class EventPaymentConfiguration : IEntityTypeConfiguration<EventPayment>
    {
        public void Configure(EntityTypeBuilder<EventPayment> builder)
        {
            // 1. تحديد اسم الجدول
            builder.ToTable("EventPayments");

            // 2. المفتاح الأساسي (Primary Key)
            builder.HasKey(e => e.Id);

            // 3. ضبط حقل السعر (عشان ميعملش Warning في الداتابيز)
            builder.Property(e => e.Amount)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            // 4. ضبط العملة بحجم ثابت
            builder.Property(e => e.Currency)
                   .HasMaxLength(10)
                   .IsRequired();

            // 5. ضبط الـ Enum (عشان يتحفظ كـ Text في الداتابيز بدل أرقام لسهولة القراءة لو حابب)
            // لو عاوزه يتحفظ أرقام عادي، ممكن تشيل السطرين دول
            builder.Property(e => e.PaymentMethod)
                   .HasConversion<string>()
                   .HasMaxLength(50);

            builder.Property(e => e.Status)
                   .HasConversion<string>()
                   .HasMaxLength(50);

            // 6. جعل PaymobOrderId فريد (Unique) عشان نضمن مفيش أوردر يتسجل مرتين
            builder.HasIndex(e => e.PaymobOrderId)
                   .IsUnique()
                   .HasFilter("[PaymobOrderId] IS NOT NULL"); // Index filter for nullable column

            // 7. إضافة Index على UserId و EventId عشان تسريع عمليات البحث
            builder.HasIndex(e => e.UserId);
            builder.HasIndex(e => e.EventId);
        }
    }
}
