using Application.Dtos.CourseDtos;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CourseController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [Authorize(Policy = "Teacher")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var course = await _courseService.GetByIdAsync(id);
                return Ok(course);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var courses = await _courseService.GetAllAsync();
                return Ok(courses);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Teacher")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CourseDto courseDto)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var createdCourse = await _courseService.CreateAsync(courseDto, userId);
                return CreatedAtAction(nameof(GetById), new { id = createdCourse.Id }, createdCourse);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Teacher")]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] CourseDto courseDto)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                await _courseService.UpdateAsync(courseDto, userId);
                return NoContent();
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
                await _courseService.DeleteAsync(id, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
