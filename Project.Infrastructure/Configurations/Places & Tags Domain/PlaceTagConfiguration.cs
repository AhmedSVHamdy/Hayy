using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Configuration
{
    internal class PlaceTagConfiguration : IEntityTypeConfiguration<PlaceTag>
    {
        public void Configure(EntityTypeBuilder<PlaceTag> builder)
        {
            builder.ToTable("PlaceTags");

          
            builder.HasKey(X => X.Id);

          
            builder.HasIndex(X => new { X.PlaceId, X.TagId }).IsUnique();
            builder.HasOne(X => X.Place)
                   .WithMany(X => X.PlaceTags) 
                   .HasForeignKey(X => X.PlaceId)
                   .OnDelete(DeleteBehavior.Cascade); 

     
            builder.HasOne(X => X.Tag)
                   .WithMany(X => X.PlaceTags) 
                   .HasForeignKey(X => X.TagId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
