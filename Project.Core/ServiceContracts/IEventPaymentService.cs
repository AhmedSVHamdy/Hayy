using Project.Core.DTO;
using Project.Core.DTOs.Payments;
using Project.Core.DTOs.Paymob;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.ServiceContracts
{
    public interface IEventPaymentService
    {
        Task<string> InitiateEventPaymentAsync(InitiateEventPaymentDto dto);
        Task ProcessEventWebhookAsync(PaymobWebhookDto dto);
    }
}
