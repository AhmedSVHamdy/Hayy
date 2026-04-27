using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.ServiceContracts
{
    public interface IDatabaseMonitorService
    {
        Task CheckDatabaseSizeAsync();
    }
}
