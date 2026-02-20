using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.DTO
{
    public class UpdateUserSettingsDTO
    {
        public bool EmailNotifications { get; set; }
        public bool NotificationsEnabled { get; set; }
    }
}
