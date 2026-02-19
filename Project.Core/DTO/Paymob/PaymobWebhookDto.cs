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
        public string? Hmac { get; set; } // التوقيع الأمني
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
        [JsonPropertyName("has_parent_transaction")]
        public bool HasParentTransaction { get; set; }

        [JsonPropertyName("integration_id")]
        public long IntegrationId { get; set; }

        [JsonPropertyName("is_3d_secure")]
        public bool Is3dSecure { get; set; }

        [JsonPropertyName("is_auth")]
        public bool IsAuth { get; set; }

        [JsonPropertyName("is_capture")]
        public bool IsCapture { get; set; }

        [JsonPropertyName("is_refunded")]
        public bool IsRefunded { get; set; }

        [JsonPropertyName("is_standalone_payment")]
        public bool IsStandalonePayment { get; set; }

        [JsonPropertyName("is_voided")]
        public bool IsVoided { get; set; }

        [JsonPropertyName("owner")]
        public long Owner { get; set; }

        [JsonPropertyName("pending")]
        public bool Pending { get; set; }
    }

    public class PaymobOrderData
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }
    }

    public class PaymobSourceData
    {
        [JsonPropertyName("pan")]
        public string? Pan { get; set; }

        [JsonPropertyName("sub_type")]
        public string? SubType { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }
    }
}
