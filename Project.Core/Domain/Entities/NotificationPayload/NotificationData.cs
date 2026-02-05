using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Domain.Entities.NotificationPayload
{
    public class NotificationData
    {
        public string UserName { get; set; } = string.Empty;
        public string UserImage { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string? TargetId { get; set; }
        public int ItemCount { get; set; } = 1;
    }
}
