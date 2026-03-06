using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.DTOs.Payments
{
    public class InitiatePaymentDto
    {
        public Guid PlanId { get; set; } // الباقة اللي اختارها
        public Guid BusinessId { get; set; } // مين اللي بيدفع
        // ممكن تضيف PromoCode مستقبلاً هنا
        public Guid? EventBookingId { get; set; }
    }
}
