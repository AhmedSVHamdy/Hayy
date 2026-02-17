using System;
using System.Collections.Generic;
using System.Text;

using System.Text.Json.Serialization;

namespace Project.Core.DTOs.Paymob
{
    public class PaymobWebhookDto
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } // نوع العملية (TRANSACTION)

        [JsonPropertyName("obj")]
        public PaymobTransactionData Obj { get; set; } // تفاصيل العملية

        [JsonPropertyName("hmac")]
        public string Hmac { get; set; } // التوقيع الأمني
    }

    public class PaymobTransactionData
    {
        [JsonPropertyName("id")]
        public long Id { get; set; } // PaymobTransactionId

        [JsonPropertyName("order")]
        public PaymobOrderData Order { get; set; } // عشان نجيب منه الـ PaymobOrderId

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("amount_cents")]
        public long AmountCents { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; }

        [JsonPropertyName("error_occured")]
        public bool ErrorOccured { get; set; }

        [JsonPropertyName("source_data")]
        public PaymobSourceData SourceData { get; set; }
    }

    public class PaymobOrderData
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }
    }

    public class PaymobSourceData
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("sub_type")]
        public string SubType { get; set; }

        [JsonPropertyName("pan")]
        public string Pan { get; set; }
    }
}
