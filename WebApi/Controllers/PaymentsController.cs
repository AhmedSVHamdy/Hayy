using Microsoft.AspNetCore.Mvc;
using Project.Core.DTOs.Payments;
using Project.Core.DTOs.Paymob; // تأكد من الـ Namespace
using Project.Core.ServiceContracts;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // 1. Endpoint لبدء الدفع (القديمة)
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

        // 2. Endpoint لاستقبال الرد (الجديدة - Webhook) 🟢
        // Paymob بيبعت POST Request عليه بيانات العملية
        [HttpPost("webhook")]
        public async Task<IActionResult> PaymobWebhook([FromBody] PaymobWebhookDto dto, [FromQuery] string hmac)
        {
            // ملحوظة: Paymob بيبعت الـ HMAC في الـ Query String مش في الـ Body
            // عشان كده استقبلناه بـ [FromQuery] وضفناه للـ DTO يدوياً
            dto.Hmac = hmac;

            try
            {
                await _paymentService.ProcessWebhookAsync(dto);
            }
            catch (Exception ex)
            {
                // لو حصل خطأ في الـ Logic (زي HMAC غلط)، بنسجله عندنا (Logging)
                 Console.WriteLine(ex.Message);
            }

            // ⚠️ قاعدة ذهبية:
            // لازم نرد بـ 200 OK مهما حصل (حتى لو فيه Error عندنا)
            // لأن لو ردينا بـ BadRequest، بايموب هيفتكر إن الرسالة موصلتش وهيفضل يبعتها تاني وتالت!
            return Ok();
        }
    }
}