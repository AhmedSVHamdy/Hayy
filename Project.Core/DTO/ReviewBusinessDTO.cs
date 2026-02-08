namespace Project.Core.DTO
{
    public class ReviewBusinessDTO
    {
        public bool IsApproved { get; set; }
        public string? Reason { get; set; } // إجباري فقط في حالة الرفض (تم ضبطه في الفاليديشن)
    }

}




