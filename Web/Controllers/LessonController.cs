using Application.Dtos.LessonDtos;
using Application.Services.Implementations;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LessonsController : ControllerBase
    {
        private readonly ILessonService _lessonService;
        private readonly ISubscriptionService _subscriptionService;

        public LessonsController(
            ILessonService lessonService, 
            ISubscriptionService subscriptionService)
        {
            _lessonService = lessonService;
            _subscriptionService = subscriptionService;
        }

        [Authorize("Teacher")]
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] LessonDto lessonDto)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var result = await _lessonService.CreateAsync(lessonDto, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize("Teacher")]
        [HttpPut]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update([FromForm] LessonDto lessonDto)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                await _lessonService.UpdateAsync(lessonDto, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize("Teacher")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                await _lessonService.DeleteAsync(id, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Policy = "StudentOrTeacher")]
        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetLessonsByCourse(Guid courseId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var lessons = await _lessonService.GetLessonsByCourseAsync(courseId, userId);
                return Ok(lessons);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Policy = "Student")]
        [HttpGet("{lessonId}/access")]
        public async Task<IActionResult> CheckLessonAccess(Guid lessonId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User ID not found.");

                var hasAccess = await _subscriptionService.CanAccessLessonAsync(userId, lessonId);
                return Ok(new { hasAccess, message = hasAccess ? "Access granted" : "No valid subscription or access code" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { hasAccess = false, message = ex.Message });
            }
        }
    }
}
