using Application.Dtos.DiscountCodeDtos;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiscountCodeController : ControllerBase
    {
        private readonly IDiscountCodeService _discountCodeService;

        public DiscountCodeController(IDiscountCodeService discountCodeService)
        {
            _discountCodeService = discountCodeService;
        }

        [Authorize(Roles = "Teacher")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateDiscountCode([FromBody] DiscountCodeCreateDto createDto)
        {
            try
            {
                var discountCode = await _discountCodeService.CreateDiscountCodeAsync(createDto, User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                return Ok(discountCode);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [Authorize(Roles = "Teacher")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var discountCode = await _discountCodeService.GetByIdAsync(id, User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                return Ok(discountCode);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("code/{code}")]
        public async Task<IActionResult> GetByCode(string code)
        {
            try
            {
                var discountCode = await _discountCodeService.GetByCodeAsync(code);
                return Ok(discountCode);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [Authorize(Roles = "Teacher")]
        [HttpGet("teacher")]
        public async Task<IActionResult> GetByTeacher()
        {
            try
            {
                var discountCodes = await _discountCodeService.GetByTeacherIdAsync(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                return Ok(discountCodes);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [Authorize(Roles = "Teacher")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _discountCodeService.DeleteAsync(id, User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                return Ok(new { Message = "Discount code deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
