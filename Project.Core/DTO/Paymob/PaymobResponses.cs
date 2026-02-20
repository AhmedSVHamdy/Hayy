using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Project.Core.DTOs.Paymob
{
    // 1. نتيجة المصادقة (Authentication)
    public class PaymobAuthResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }
    }

    // 2. نتيجة تسجيل الأوردر (Order Registration)
    public class PaymobOrderResponse
    {
        [JsonPropertyName("id")]
        public long Id { get; set; } // ده الـ PaymobOrderId المهم
    }

    // 3. نتيجة طلب مفتاح الدفع (Payment Key)
    public class PaymobKeyResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; } // ده الـ PaymentKey النهائي
    }
}
