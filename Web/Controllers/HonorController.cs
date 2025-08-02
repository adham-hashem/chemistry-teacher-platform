using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Dtos.HonorDtos;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HonorController : ControllerBase
    {
        private readonly IHonorService _honorService;

        public HonorController(IHonorService honorService)
        {
            _honorService = honorService;
        }

        [HttpGet]
        public async Task<ActionResult<List<HonorDto>>> GetAll()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User ID not found.");

                var honors = await _honorService.GetAllAsync(userId);
                return Ok(honors);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult<HonorDto>> Create([FromForm] HonorDto honorDto, [FromForm] IFormFile? studentImage)
        {
            try
            {
                // Debug logging
                Console.WriteLine($"Received honorDto: StudentId={honorDto.StudentId}, Description={honorDto.Description}");
                Console.WriteLine($"Received studentImage: {studentImage?.FileName ?? "null"}");

                if (!ModelState.IsValid)
                {
                    Console.WriteLine("ModelState Errors:");
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        Console.WriteLine(error.ErrorMessage);
                    }
                    return BadRequest(ModelState);
                }

                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User ID not found.");

                var result = await _honorService.CreateAsync(honorDto, userId, studentImage);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<HonorDto>> GetById(Guid id)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User ID not found.");

                var honor = await _honorService.GetByIdAsync(id, userId);
                return Ok(honor);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpGet("student")]
        public async Task<ActionResult<List<HonorDto>>> GetByStudentId()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User ID not found.");

                var honors = await _honorService.GetByStudentIdAsync(userId);
                return Ok(honors);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpGet("teacher/{teacherId}")]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult<List<HonorDto>>> GetByTeacherId(string teacherId)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User ID not found.");

                var honors = await _honorService.GetByTeacherIdAsync(teacherId, userId);
                return Ok(honors);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User ID not found.");

                await _honorService.DeleteAsync(id, userId);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
    }
}