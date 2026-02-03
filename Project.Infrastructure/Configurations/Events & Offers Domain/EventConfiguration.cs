using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Configuration
{
    internal class EventConfiguration : IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            builder.HasKey(X => X.Id);

            builder.Property(X => X.Title)
                   .IsRequired()
                   .HasMaxLength(255); // بناءً على الـ ERD

            builder.Property(X => X.Description)
                   .IsRequired()
                   .HasMaxLength(500);

            // GalleryImages: غالباً تخزن كـ JSON أو نص طويل
            builder.Property(X => X.GalleryImages)
                   .IsRequired();

            builder.Property(X => X.Datetime)
                   .IsRequired();

            builder.Property(X => X.Capacity)
                   .IsRequired();

            builder.Property(X => X.Price)
                   .IsRequired()
                   .HasColumnType("decimal(18,2)");

            // العلاقات
            builder.HasOne(X => X.Place)
                   .WithMany() // لم يظهر في كلاس Place قائمة Events، نتركها فارغة أو نعدلها لاحقاً
                   .HasForeignKey(X => X.PlaceId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(X => X.EventBookings)
                   .WithOne(X => X.Event)
                   .HasForeignKey(X => X.EventId)
                   .OnDelete(DeleteBehavior.Cascade); // عند حذف الفعالية تُحذف الحجوزات
        }
    }
}
