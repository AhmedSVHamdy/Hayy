//using Microsoft.AspNetCore.Mvc;
//using Project.Core.DTO;
//using Project.Core.DTOs.Paymob;
//using Project.Core.ServiceContracts;
//using Project.Core.Services;

//namespace WebApi.Controllers
//{
//    [Route("api/payments")]
//    [ApiController]
//    public class PaymentFilterController : ControllerBase
//    {
//        private readonly IEventPaymentService _paymentService;

//        public PaymentFilterController(EventPaymentService paymentService)
//        {
//            _paymentService = paymentService;
//        }

//        [HttpPost("events/initiate")]
//        public async Task<IActionResult> InitiateEventPayment([FromBody] InitiateEventPaymentDto dto)
//        {
//            var key = await _paymentService.InitiateEventPaymentAsync(dto);
//            return Ok(new { PaymentKey = key });
//        }

//        [HttpPost("events/webhook")]
//        public async Task<IActionResult> EventWebhook([FromBody] PaymobWebhookDto dto)
//        {
//            await _paymentService.ProcessEventWebhookAsync(dto);
//            return Ok();
//        }


//    }
//}
