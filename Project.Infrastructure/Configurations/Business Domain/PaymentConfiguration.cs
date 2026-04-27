using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Core.Domain.Entities;
using Project.Core.Enums;

namespace Project.Infrastructure.Configuration
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            // Primary Key
            builder.HasKey(x => x.Id);

            // المبلغ (decimal دقيق بدون تقريب)
            builder.Property(x => x.Amount)
                   .IsRequired()
                   .HasColumnType("decimal(18,2)");

            // العملة
            builder.Property(x => x.Currency)
                   .HasMaxLength(3)
                   .HasDefaultValue("EGP");

            // ✅ تحويل PaymentStatus Enum → String في الداتابيز
            // عشان لو فتحت الداتابيز تلاقي "Pending" مش "0"
            builder.Property(x => x.Status)
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .IsRequired();

            // ✅ تحويل PaymentMethod Enum → String في الداتابيز
            builder.Property(x => x.PaymentMethod)
                   .HasConversion<string>()
                   .HasMaxLength(50)
                   .IsRequired();

            // حقول Paymob (Optional لأنها بتيجي بعد الرد)
            builder.Property(x => x.PaymobOrderId)
                   .IsRequired(false);

            builder.Property(x => x.PaymobTransactionId)
                   .IsRequired(false);

            // العلاقة مع Subscription
            // OnDelete.Restrict عشان لو الاشتراك اتمسح سجل الدفع يفضل كأرشيف
            builder.HasOne(x => x.Subscription)
                   .WithMany(s => s.Payments)
                   .HasForeignKey(x => x.SubscriptionId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}