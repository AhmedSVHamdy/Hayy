using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Configuration
{
    internal class OpeningHourConfiguration : IEntityTypeConfiguration<OpeningHour>
    {
        public void Configure(EntityTypeBuilder<OpeningHour> builder)
        {
            builder.ToTable("OpeningHours");

          
            builder.HasKey(X => X.Id);

            
            builder.Property(X => X.DayOfWeek)
                   .HasConversion<string>()
                   .HasMaxLength(15)
                   .IsRequired();

            // TimeSpan: يتم تخزينه في SQL Server كـ TIME(7) بشكل تلقائي
            builder.Property(X => X.OpenTime)
                   .IsRequired();

            builder.Property(X => X.CloseTime)
                   .IsRequired();

            //Relationships

            // العلاقة مع المكان (Place)
            builder.HasOne(X => X.Place)
                   .WithMany(X => X.OpeningHours) // هذه القائمة موجودة في كلاس Place كما رأينا سابقاً
                   .HasForeignKey(X => X.PlaceId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
