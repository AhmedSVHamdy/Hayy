using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Configurations.Content___Social_Domain
{
    public class ReviewReplyConfiguration : IEntityTypeConfiguration<ReviewReply>
    {
        public void Configure(EntityTypeBuilder<ReviewReply> builder)
        {
            builder.ToTable("ReviewReplies");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.ReplyText)
                .IsRequired()
                .HasMaxLength(1000); // حد أقصى للرد

            // العلاقة: الرد لازم يكون تبع ريفيو
            builder.HasOne(r => r.Review)
                .WithMany(rev => rev.ReviewReplies) // لازم تضيف الكولكشن دي في Review Entity
                .HasForeignKey(r => r.ReviewId)
                .OnDelete(DeleteBehavior.Cascade); // لو الريفيو اتمسح، الردود تتمسح
        }
    }
}
