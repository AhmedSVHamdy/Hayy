using Microsoft.AspNetCore.Mvc;
using Project.Core.DTOs.Payments;
using Project.Core.DTOs.Paymob;
using Project.Core.ServiceContracts;

namespace WebApi.Controllers
{
    [Route("api/Payments")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        /// <summary>
        /// يبدأ عملية دفع جديدة لاشتراك بيزنس في باقة معينة.
        /// يرجع PaymentKey اللي الـ Frontend بيستخدمه مع Paymob iFrame.
        /// </summary>
        [HttpPost("initiate")]
        public async Task<IActionResult> InitiatePayment([FromBody] InitiatePaymentDto dto)
        {
            try
            {
                var paymentKey = await _paymentService.InitiatePaymentAsync(dto);
                return Ok(new { PaymentKey = paymentKey });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Webhook endpoint بيستقبل رد Paymob بعد اكتمال العملية.
        /// لازم يرجع 200 OK دايماً حتى لو حصل error عندنا،
        /// عشان Paymob متبعتش الطلب تاني.
        /// </summary>
        [HttpPost("webhook")]
        public async Task<IActionResult> PaymobWebhook([FromBody] PaymobWebhookDto dto, [FromQuery] string hmac)
        {
            // Paymob بيبعت الـ HMAC في الـ Query String مش في الـ Body
            dto.Hmac = hmac;

            try
            {
                await _paymentService.ProcessWebhookAsync(dto);
            }
            catch (Exception ex)
            {
                // نسجل الـ Error عندنا بس نرجع 200 عشان Paymob متكررش الطلب
                Console.WriteLine($"Webhook error: {ex.Message}");
            }

            return Ok();
        }
    }
}