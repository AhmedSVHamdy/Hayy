using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Settings
{
    public class PaymobEventSettings
    {
        public string ApiKey { get; set; }
        public string HmacSecret { get; set; }
        public int CardIntegrationId { get; set; } // 👈 خليناها int
        public int IframeId { get; set; }
    }
}
