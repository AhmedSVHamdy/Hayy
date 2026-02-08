using Project.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.ServiceContracts
{
    public interface IUserLogService
    {
        Task LogActivityAsync(CreateUserLogDto dto);
    }
}
