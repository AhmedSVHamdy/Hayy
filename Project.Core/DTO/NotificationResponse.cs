using Project.Core.Domain.Entities.NotificationPayload;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.DTO
{
    public class NotificationResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; }= string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ReferenceId { get; set; }
        public string? ReferenceType { get; set; }

        // هنا التعديل: بنرجع الداتا مفكوكة (Object) عشان الفرونت يعرضها علطول
        public NotificationData? Data { get; set; }
    }
}
