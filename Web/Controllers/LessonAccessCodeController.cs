using Application.Dtos.LessonAccessCodeDtos;
using Application.Dtos.RedeemCodeDtos;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LessonAccessCodeController : ControllerBase
    {
        private readonly ILessonAccessCodeService _lessonAccessCodeService;

        public LessonAccessCodeController(ILessonAccessCodeService lessonAccessCodeService)
        {
            _lessonAccessCodeService = lessonAccessCodeService;
        }

        [Authorize(Roles = "Teacher")]
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateCode([FromQuery] Guid lessonId)
        {
            try
            {
                var teacherId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var code = await _lessonAccessCodeService.GenerateCodeAsync(lessonId, teacherId);
                return Ok(code);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Student")]
        [HttpPost("redeem")]
        public async Task<IActionResult> RedeemCode([FromBody] RedeemCodeDto redeemCodeDto)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var code = await _lessonAccessCodeService.RedeemCodeAsync(redeemCodeDto, userId);
                return Ok(code);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Teacher")]
        [HttpGet("lesson/{lessonId}")]
        public async Task<IActionResult> GetCodesByLesson(Guid lessonId)
        {
            try
            {
                var teacherId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var codes = await _lessonAccessCodeService.GetCodesByLessonAsync(lessonId, teacherId);
                return Ok(codes);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Teacher")]
        [HttpDelete("revoke/{lessonId}/{userId}")]
        public async Task<IActionResult> RevokeAccess(Guid lessonId, string userId)
        {
            try
            {
                var teacherId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                await _lessonAccessCodeService.RevokeAccessAsync(lessonId, userId, teacherId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Teacher")]
        [HttpDelete("revoke-all/{lessonId}")]
        public async Task<IActionResult> RevokeAllAccess(Guid lessonId)
        {
            try
            {
                var teacherId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                await _lessonAccessCodeService.RevokeAllAccessAsync(lessonId, teacherId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Teacher")]
        [HttpDelete("codes/{lessonId}")]
        public async Task<IActionResult> DeleteCodesByLesson(Guid lessonId)
        {
            try
            {
                var teacherId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                await _lessonAccessCodeService.DeleteCodesByLessonAsync(lessonId, teacherId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Teacher")]
        [HttpDelete("{codeId}")]
        public async Task<IActionResult> DeleteCodeAsync(Guid codeId)
        {
            try
            {
                var teacherId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                await _lessonAccessCodeService.DeleteCodeAsync(codeId, teacherId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
