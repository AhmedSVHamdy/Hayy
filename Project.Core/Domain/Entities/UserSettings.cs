using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;

namespace Project.Core.Domain.Entities
{
    public class UserSettings
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public bool EmailNotifications { get; set; }
        public bool NotificationsEnabled { get; set; }

        public User User { get; set; } = null!;
    }
}