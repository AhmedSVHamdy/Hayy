using Project.Core.DTOs.Payments;
using Project.Core.DTOs.Paymob;

namespace Project.Core.ServiceContracts
{
    public interface IPaymentService
    {
        Task<string> InitiatePaymentAsync(InitiatePaymentDto dto);

        Task ProcessWebhookAsync(PaymobWebhookDto dto);
    }
}