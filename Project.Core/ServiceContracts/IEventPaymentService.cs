using Project.Core.DTOs.Payments;
using Project.Core.DTOs.Paymob;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.ServiceContracts
{
    internal interface IEventPaymentService
    {
        Task<string> InitiatePaymentAsync(InitiatePaymentDto dto);

        Task ProcessWebhookAsync(PaymobWebhookDto dto);
    }
}
