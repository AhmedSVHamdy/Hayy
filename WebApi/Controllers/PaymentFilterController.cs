using Microsoft.AspNetCore.Mvc;
using Project.Core.DTO;
using Project.Core.DTOs.Paymob;
using Project.Core.ServiceContracts;

namespace WebApi.Controllers
{
    [Route("api/EventPayment")]
    [ApiController]
    public class PaymentFilterController : ControllerBase
    {
        private readonly IEventPaymentService _paymentService;

        public PaymentFilterController(IEventPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // 1. مسار بدء الدفع (الفلاتر بيكلمه)
        /// <summary>
        /// Initiates a payment process for an event and returns a URL for the payment interface.
        /// </summary>
        /// <remarks>Use this endpoint to start the payment workflow for an event. The returned URL can be
        /// used to redirect the user to the payment provider's interface.</remarks>
        /// <param name="dto">An object containing the details required to initiate the event payment. Must not be null.</param>
        /// <returns>An HTTP 200 response containing an object with the payment interface URL if the initiation is successful.</returns>
        [HttpPost("initiate")]
        public async Task<IActionResult> InitiateEventPayment([FromBody] InitiateEventPaymentDto dto)
        {
            var iframeUrl = await _paymentService.InitiateEventPaymentAsync(dto);
            return Ok(new { url = iframeUrl });
        }

        // 2. مسار الـ Webhook (بايموب بيكلمه أوتوماتيك بعد الدفع)
        /// <summary>
        /// Handles incoming Paymob webhook events by processing the event data and updating the payment status
        /// accordingly.
        /// </summary>
        /// <remarks>Paymob expects a 200 OK response to confirm successful receipt of the webhook event.
        /// Failure to return 200 OK may result in repeated webhook delivery attempts.</remarks>
        /// <param name="dto">The webhook event data received from Paymob. Must contain valid event information as expected by the payment
        /// service.</param>
        /// <returns>An HTTP 200 OK result indicating successful receipt and processing of the webhook event.</returns>
        [HttpPost("webhook")]
        public async Task<IActionResult> EventWebhook(
                      [FromBody] PaymobWebhookDto dto,
                      [FromQuery] string hmac)
        {
            dto.Hmac = hmac; // ← حط الـ HMAC في الـ DTO قبل ما تبعته للسيرفيس
            await _paymentService.ProcessEventWebhookAsync(dto);
            return Ok();
        
        }
        /// <summary>
        /// Handles the result of a payment process and returns an appropriate response based on the success or failure of the payment.
        /// </summary>
        /// <param name="success"></param>
        /// <returns></returns>
        [HttpGet("payment-result")]
        public IActionResult PaymentResult([FromQuery] bool success)
        {
            return success
                ? Ok("تم الدفع بنجاح")
                : BadRequest("فشل الدفع");
        }
    }
}