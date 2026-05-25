using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Core.Domain.Entities;

namespace Project.Infrastructure.Configuration
{
    internal class EventBookingConfiguration : IEntityTypeConfiguration<EventBooking>
    {
        public void Configure(EntityTypeBuilder<EventBooking> builder)
        {
            builder.HasKey(X => X.Id);

            builder.Property(X => X.TicketQuantity)
                   .IsRequired();

            builder.Property(X => X.CheckedInCount)
                   .HasDefaultValue(0);

            // تحويل حالة الحجز (Pending, Confirmed, Cancelled)
            builder.Property(X => X.Status)
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .IsRequired();

            builder.Property(X => X.IsPaid)
                   .IsRequired();

            builder.Property(X => X.PaidAmount)
                   .IsRequired()
                   .HasColumnType("decimal(18,2)");

            // تحويل طريقة الدفع
            builder.Property(X => X.PaymentMethod)
                   .HasConversion<string>()
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(X => X.PaymentDate)
                   .IsRequired();

            builder.Property(X => X.TransactionId)
                   .HasMaxLength(100)
                   .IsRequired();

            // ==========================================
            // Relationships Configuration (العلاقات الصح بس)
            // ==========================================

            // 1. علاقة الحجز باليوزر
            builder.HasOne(eb => eb.User)
                   .WithMany(u => u.EventBookings)
                   .HasForeignKey(eb => eb.UserId)
                   .OnDelete(DeleteBehavior.NoAction);

            // 2. علاقة الحجز بالإيفينت
            builder.HasOne(eb => eb.Event)
                   .WithMany(e => e.EventBookings)
                   .HasForeignKey(eb => eb.EventId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}