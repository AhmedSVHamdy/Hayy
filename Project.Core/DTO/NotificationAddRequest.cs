using Project.Core.Domain.Entities.NotificationPayload;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Project.Core.DTO
{
    public class NotificationAddRequest
    {
        [Required] public string Title { get; set; } = string.Empty;
        [Required] public string Body { get; set; }  = string.Empty;
        public string Type { get; set; } = "System";
        public Guid UserId { get; set; }

        public string? ReferenceId { get; set; }
        public string? ReferenceType { get; set; }
        public string? GroupKey { get; set; }

        // هنا التعديل: بنستقبل الداتا كـ Object عادي مش String
        public NotificationData? Data { get; set; }
    }
}
