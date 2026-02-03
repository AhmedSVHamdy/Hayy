using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Configuration
{
    public class OfferConfiguration : IEntityTypeConfiguration<Offer>
    {
        public void Configure(EntityTypeBuilder<Offer> builder)
        {
            builder.HasKey(X => X.Id);

            builder.Property(X => X.Title)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(X => X.Description)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(X => X.GalleryImages)
                   .IsRequired();

            builder.Property(X => X.Discount)
                   .IsRequired()
                   .HasColumnType("decimal(18,2)"); // نسبة الخصم أو قيمته

            builder.Property(X => X.StartDate)
                   .IsRequired();

            builder.Property(X => X.EndDate)
                   .IsRequired();

            // تحويل الـ Enum إلى نص
            builder.Property(X => X.Status)
                   .HasConversion<string>()
                   .HasMaxLength(20) // Active, Expired
                   .IsRequired();

            // Relationships Configuration
            builder.HasOne(X => X.Place)
                   .WithMany()
                   .HasForeignKey(X => X.PlaceId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
