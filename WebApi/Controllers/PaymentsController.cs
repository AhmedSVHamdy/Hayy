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
        /// Initiates a payment process for a business subscription and returns a Paymob payment key.
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
        /// Receives Paymob webhook notifications and processes the transaction result.
        /// Always returns 200 OK to prevent Paymob from resending the request.
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