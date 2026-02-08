using Project.Core.Domain.Entities;
using Project.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Domain.RopositoryContracts
{
    public interface IUserLogRepository
    {
        // دالة لإضافة لوج جديد
        Task AddLogAsync(UserLog log);

        
    }
}
