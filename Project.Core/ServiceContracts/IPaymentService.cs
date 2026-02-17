using Project.Core.DTOs.Payments;
using Project.Core.DTOs.Paymob;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.ServiceContracts
{
    public interface IPaymentService
    {
        // ده العقد: "أي حد هينفذ الخدمة دي لازم يعرف يعمل InitiatePayment"
        Task<string> InitiatePaymentAsync(InitiatePaymentDto dto);

        Task ProcessWebhookAsync(PaymobWebhookDto dto);
    }
}