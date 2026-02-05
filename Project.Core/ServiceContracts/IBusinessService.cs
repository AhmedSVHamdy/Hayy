using Project.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;
using Project.Core.DTO;

namespace Project.Core.ServiceContracts
{
    public interface IBusinessService
    {
        // دالة الموافقة على البيزنس
        Task<BusinessResponse?> ApproveBusinessProfile(Guid businessId);
    }
}
