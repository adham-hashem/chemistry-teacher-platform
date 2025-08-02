using System;
using System.Text.Json; // Add this for JsonSerializer
using System.Threading.Tasks;
using Application.Dtos.ExamDtos;
using Application.Services.Interfaces;
using Domain.Enums;
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
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User not authenticated." });

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
        [HttpPost("mcq")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateMcq([FromForm] CreateMcqExamRequest request)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User not authenticated." });

                // Deserialize the Questions JSON string
                var questions = string.IsNullOrEmpty(request.QuestionsJson)
                    ? new List<McqQuestionDto>()
                    : JsonSerializer.Deserialize<List<McqQuestionDto>>(request.QuestionsJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                var examDto = new ExamDto
                {
                    LessonId = request.LessonId,
                    Title = request.Title,
                    Questions = questions,
                    ExamType = ExamType.MCQ,
                    CertificateThreshold = request.CertificateThreshold
                };

                // Pass the image files to the service
                var createdExam = await _examService.CreateAsync(examDto, userId, null, request.QuestionImageFiles);
                return CreatedAtAction(nameof(GetById), new { id = createdExam.Id }, createdExam);
            }
            catch (JsonException)
            {
                return BadRequest(new { message = "Invalid questions JSON format." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Teacher")]
        [HttpPost("pdf")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreatePdf([FromForm] ExamDto examDto, [FromForm] IFormFile examPdf)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User not authenticated." });

                examDto.ExamType = ExamType.PDF;
                var createdExam = await _examService.CreateAsync(examDto, userId, examPdf);
                return CreatedAtAction(nameof(GetById), new { id = createdExam.Id }, createdExam);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Teacher")]
        [HttpPut("{id}/mcq")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateMcq(Guid id, [FromForm] CreateMcqExamRequest request)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User not authenticated." });

                // Deserialize the Questions JSON string
                var questions = string.IsNullOrEmpty(request.QuestionsJson)
                    ? new List<McqQuestionDto>()
                    : JsonSerializer.Deserialize<List<McqQuestionDto>>(request.QuestionsJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                var examDto = new ExamDto
                {
                    LessonId = request.LessonId,
                    Title = request.Title,
                    Questions = questions,
                    ExamType = ExamType.MCQ,
                    CertificateThreshold = request.CertificateThreshold
                };

                // Pass the image files to the service
                var updatedExam = await _examService.UpdateAsync(id, examDto, userId, null, request.QuestionImageFiles);
                return Ok(updatedExam);
            }
            catch (JsonException)
            {
                return BadRequest(new { message = "Invalid questions JSON format." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Teacher")]
        [HttpPut("{id}/pdf")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdatePdf(Guid id, [FromForm] ExamDto examDto, [FromForm] IFormFile? examPdf = null)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User not authenticated." });

                examDto.ExamType = ExamType.PDF;
                var updatedExam = await _examService.UpdateAsync(id, examDto, userId, examPdf);
                return Ok(updatedExam);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Teacher")]
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(Guid id, [FromForm] ExamDto examDto, [FromForm] IFormFile? examPdf = null)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User not authenticated." });

                var updatedExam = await _examService.UpdateAsync(id, examDto, userId, examPdf);
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
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User not authenticated." });

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