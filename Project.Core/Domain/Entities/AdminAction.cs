using Project.Core.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Core.Domain.Entities
{
    public class AdminAction
    {
        public Guid Id { get; set; }

        // 1. مين الأدمن اللي عمل الحركة؟ (رابط بجدول User)
        public Guid AdminId { get; set; }

        // 2. نوع الحركة (Created, Updated, Approved...)
        public AdminActionType ActionType { get; set; }

        // 3. الحركة دي تمت على إيه؟ (Business, User, Category...)
        public TargetType TargetType { get; set; }

        // 4. الآي دي بتاع الحاجة اللي اتعدلت (String عشان يقبل Guid أو Int)
        public string TargetId { get; set; } = string.Empty;

        // 5. تفاصيل إضافية (مثلاً سبب الرفض)
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relationship
        [ForeignKey(nameof(AdminId))]
        public virtual User Admin { get; set; } = null!;
    }
}
