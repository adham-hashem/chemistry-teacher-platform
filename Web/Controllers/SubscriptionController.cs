using Application.Dtos.PaymentDtos;
using Application.Dtos.SubscriptionDtos;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateSubscription([FromBody] SubscriptionDto subscriptionDto)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var createdSubscription = await _subscriptionService.CreateSubscriptionAsync(subscriptionDto, userId);
                return CreatedAtAction(nameof(CreateSubscription), new { id = createdSubscription.Id }, createdSubscription);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Teacher")]
        [HttpPost("lecture-based")]
        public async Task<IActionResult> CreateLectureBasedSubscription([FromBody] SubscriptionDto subscriptionDto)
        {
            try
            {
                var teacherId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var createdSubscription = await _subscriptionService.CreateLectureBasedSubscriptionAsync(subscriptionDto, teacherId);
                return CreatedAtAction(nameof(CreateLectureBasedSubscription), new { id = createdSubscription.Id }, createdSubscription);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("access/lesson/{lessonId}")]
        public async Task<IActionResult> CanAccessLesson(Guid lessonId)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var canAccess = await _subscriptionService.CanAccessLessonAsync(userId, lessonId);
                return Ok(new { canAccess });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("access/lesson/{lessonId}")]
        public async Task<IActionResult> MarkLessonAsAccessed(Guid lessonId)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                await _subscriptionService.MarkLessonAsAccessedAsync(userId, lessonId);
                return Ok(new { message = "Lesson marked as accessed." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Teacher")]
        [HttpDelete("user/{userId}")]
        public async Task<IActionResult> DeleteUserSubscriptions(string userId)
        {
            try
            {
                var teacherId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                await _subscriptionService.DeleteUserSubscriptionsAsync(userId, teacherId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Teacher")]
        [HttpDelete("all")]
        public async Task<IActionResult> DeleteAllSubscriptions()
        {
            try
            {
                var teacherId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                await _subscriptionService.DeleteAllSubscriptionsAsync(teacherId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
