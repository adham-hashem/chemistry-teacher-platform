using Application.Dtos.ExamDtos;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExamController : ControllerBase
    {
        private readonly IExamService _examService;

        public ExamController(IExamService examService)
        {
            _examService = examService;
        }

        [Authorize(Policy = "Teacher")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var exam = await _examService.GetByIdAsync(id);
                return Ok(exam);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [Authorize(Policy = "StudentOrTeacher")]
        [HttpGet("lesson/{lessonId}")]
        public async Task<IActionResult> GetByLessonId(Guid lessonId)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var exams = await _examService.GetByLessonIdAsync(lessonId, userId);
                return Ok(exams);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Policy = "Teacher")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var exams = await _examService.GetAllAsync();
                return Ok(exams);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Teacher")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ExamDto examDto)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var createdExam = await _examService.CreateAsync(examDto, userId);
                return CreatedAtAction(nameof(GetById), new { id = createdExam.Id }, createdExam);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Teacher")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ExamDto examDto)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var updatedExam = await _examService.UpdateAsync(id, examDto, userId);
                return Ok(updatedExam);
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
                await _examService.DeleteAsync(id, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
