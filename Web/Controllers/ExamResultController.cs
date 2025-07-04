using Application.Dtos.ExamDtos;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExamResultController : ControllerBase
    {
        private readonly IExamResultService _examResultService;

        public ExamResultController(IExamResultService examResultService)
        {
            _examResultService = examResultService;
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var examResult = await _examResultService.GetByIdAsync(id);
                return Ok(examResult);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("exam/{examId}")]
        public async Task<IActionResult> GetByExamId(Guid examId)
        {
            try
            {
                var examResults = await _examResultService.GetByExamIdAsync(examId);
                return Ok(examResults);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Student")]
        [HttpGet("user")]
        public async Task<IActionResult> GetByUserId()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var examResults = await _examResultService.GetByUserIdAsync(userId);
                return Ok(examResults);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Student")]
        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] ExamResultDto examResultDto)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var createdExamResult = await _examResultService.SubmitAsync(examResultDto, userId);
                return CreatedAtAction(nameof(GetById), new { id = createdExamResult.Id }, createdExamResult);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Teacher")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                await _examResultService.DeleteAsync(id, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
