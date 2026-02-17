using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Domain.Entities
{
    public class BusinessSubscription
    {
        public Guid Id { get; set; }

        // مين المشترك؟
        public Guid BusinessId { get; set; }
        public Business Business { get; set; }

        // مشترك في إيه؟
        public Guid PlanId { get; set; }
        public SubscriptionPlan Plan { get; set; }

        // تفاصيل الوقت (الأهم)
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; } // دي اللي بنشيك عليها في الـ Login

        // حالة الاشتراك
        public bool IsActive { get; set; } // لو ألغى الاشتراك بس لسه وقته مخلصش، دي تبقى False بس لسه شغال لحد EndDate
        public bool AutoRenew { get; set; }

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
