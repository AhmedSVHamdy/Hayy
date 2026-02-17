using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Core.Domain.Entities; // تأكد من الـ Namespace بتاعك

namespace Project.Infrastructure.Configuration
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            // اسم الجدول
           // builder.ToTable("Payments");

            // Primary Key
            builder.HasKey(x => x.Id);

            // التعامل مع الفلوس (مهم جداً عشان ميعملش تقريب غلط)
            builder.Property(x => x.Amount)
                   .IsRequired()
                   .HasColumnType("decimal(18,2)"); // 18 رقم، منهم 2 عشري

            // العملة
            builder.Property(x => x.Currency)
                   .HasMaxLength(3) // EGP, USD
                   .HasDefaultValue("EGP");

            // حالة الدفع (Success, Pending, Failed)
            // لو انت عاملها Enum استخدم HasConversion<string>()
            builder.Property(x => x.Status)
                   .HasMaxLength(20)
                   .IsRequired();

            // طريقة الدفع (Visa, Wallet)
            builder.Property(x => x.PaymentMethod)
                   .HasMaxLength(50);

            // 🔴 أعمدة Paymob (مهمة جداً)
            // عملناها Optional عشان وانت بتنشأ الريكويست لسه ميكونش جالك الرد
            builder.Property(x => x.PaymobOrderId)
                   .IsRequired(false);

            builder.Property(x => x.PaymobTransactionId)
                   .IsRequired(false);

            // العلاقات Relationships
            // كل عملية دفع مربوطة باشتراك (اختياري، لأن ممكن دفع لسبب تاني غير الاشتراك)
            builder.HasOne(x => x.Subscription)
                   .WithMany(s => s.Payments)
                   .HasForeignKey(x => x.SubscriptionId)
                   .OnDelete(DeleteBehavior.Restrict); // عشان لو مسحت الاشتراك سجل الدفع ميتمسحش (أرشيف)
        }
    }
}