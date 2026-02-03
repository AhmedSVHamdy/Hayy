using Project.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.ServiceContracts
{
    public interface INotifier
    {
        // الدالة دي وظيفتها: "يا سيستم، وصل الرسالة دي لليوزر ده"
        Task SendToUserAsync(Guid userId, NotificationResponse notification);
    }
}
