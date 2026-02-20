using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Project.Core.DTOs.Paymob
{
    // 1. طلب المصادقة
    public class PaymobAuthRequest
    {
        [JsonPropertyName("api_key")]
        public string ApiKey { get; set; }
    }

    // 2. طلب تسجيل الأوردر
    public class PaymobOrderRequest
    {
        [JsonPropertyName("auth_token")]
        public string AuthToken { get; set; }

        [JsonPropertyName("delivery_needed")]
        public string DeliveryNeeded { get; set; } = "false";

        [JsonPropertyName("amount_cents")]
        public string AmountCents { get; set; } // السعر بالقروش (100 جنيه = 10000)

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "EGP";

        [JsonPropertyName("items")]
        public List<object> Items { get; set; } = new List<object>(); // قائمة فاضية عشان دي خدمة مش منتجات
    }

    // 3. طلب مفتاح الدفع (أهم واحد)
    public class PaymobKeyRequest
    {
        [JsonPropertyName("auth_token")]
        public string AuthToken { get; set; }

        [JsonPropertyName("amount_cents")]
        public string AmountCents { get; set; }

        [JsonPropertyName("expiration")]
        public int Expiration { get; set; } = 3600; // المفتاح يموت بعد ساعة

        [JsonPropertyName("order_id")]
        public string OrderId { get; set; }

        [JsonPropertyName("billing_data")]
        public PaymobBillingData BillingData { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "EGP";

        [JsonPropertyName("integration_id")]
        public int IntegrationId { get; set; }
    }

    public class PaymobBillingData
    {
        // Paymob بيحتاج البيانات دي إجبارية حتى لو وهمية
        [JsonPropertyName("apartment")] public string Apartment { get; set; } = "NA";
        [JsonPropertyName("email")] public string Email { get; set; }
        [JsonPropertyName("floor")] public string Floor { get; set; } = "NA";
        [JsonPropertyName("first_name")] public string FirstName { get; set; }
        [JsonPropertyName("street")] public string Street { get; set; } = "NA";
        [JsonPropertyName("building")] public string Building { get; set; } = "NA";
        [JsonPropertyName("phone_number")] public string PhoneNumber { get; set; }
        [JsonPropertyName("shipping_method")] public string ShippingMethod { get; set; } = "NA";
        [JsonPropertyName("postal_code")] public string PostalCode { get; set; } = "NA";
        [JsonPropertyName("city")] public string City { get; set; } = "NA";
        [JsonPropertyName("country")] public string Country { get; set; } = "NA";
        [JsonPropertyName("last_name")] public string LastName { get; set; }
        [JsonPropertyName("state")] public string State { get; set; } = "NA";
    }
}
