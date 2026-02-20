using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Settings
{
    public class PaymobSettings
    {
        public string ApiKey { get; set; }
        public int IntegrationId { get; set; }
        public int IFrameId { get; set; }
        public string HmacSecret { get; set; }
        public string CallbackUrl { get; set; }
    }
}
