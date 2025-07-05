using Application.Dtos.PaymentDtos;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [Route("webhook")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public WebhookController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<IActionResult> HandleWebhook([FromBody] PaymentCallbackDto callback)
        {
            try
            {
                await _paymentService.ProcessPaymentCallbackAsync(callback);
                return Ok(new { success = true, message = "Webhook processed successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
