using Application.Dtos.PaymentDtos;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [Authorize]
        [HttpPost("initiate")]
        public async Task<IActionResult> InitiatePayment([FromBody] PaymentInitiateRequestDto request)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var response = await _paymentService.InitiatePaymentAsync(request, userId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("callback")]
        public async Task<IActionResult> ProcessCallback([FromBody] PaymentCallbackDto callback)
        {
            try
            {
                await _paymentService.ProcessPaymentCallbackAsync(callback);
                return Ok(new { message = "Payment callback processed successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
