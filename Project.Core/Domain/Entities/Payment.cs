using Project.Core.Enums;

namespace Project.Core.Domain.Entities
{
    public class Payment
    {
        public Guid Id { get; set; }

        // ربط الدفع بالاشتراك (عشان نعرف الفلوس دي دفعت لانهي فترة)
        public Guid? SubscriptionId { get; set; }

        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EGP";

        public string PaymentMethod { get; set; } // Visa, Wallet, etc.
        public string Status { get; set; } // Pending, Success, Failed

        // 🔴 حقول Paymob المهمة جداً (مش موجودة في الصورة عندك)
        public long? PaymobOrderId { get; set; } // رقم الأوردر عند بايموب
        public long? PaymobTransactionId { get; set; } // رقم العملية المرجعي

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        public Guid BusinessId { get; set; } // مين
        public Guid PlanId { get; set; } // اشترى إيه
        public Guid? EventBookingId { get; set; }
        // public Guid? SubscriptionId { get; set; } // المفتاح الأجنبي
        public BusinessSubscription? Subscription { get; set; }
    }
}

