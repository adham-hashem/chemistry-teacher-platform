using Application.Dtos.UserDtos;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UserController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // Get all users (Admin only)
        [Authorize(Roles = "Teacher")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApplicationUser>>> GetUsers()
        {
            var users = await _userManager.Users
                .Select(u => new
                {
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    //u.IsOnline,
                    u.RegistrationDate,
                    u.LastActiveDate,
                    u.IsEmailVerified
                })
                .ToListAsync();
            return Ok(users);
        }

        // Get all students
        [Authorize(Roles = "Teacher")]
        [HttpGet("students")]
        public async Task<ActionResult<IEnumerable<object>>> GetStudents()
        {
            var students = await _userManager.GetUsersInRoleAsync("Student");
            var studentData = students.Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                u.Grade,
                //u.IsOnline,
                u.RegistrationDate,
                u.LastActiveDate,
                u.IsEmailVerified,
            }).ToList();

            return Ok(studentData);
        }

        // Get specific user by ID
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found");
            }

            // Check if requested user or admin
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId != id && !User.IsInRole("Teacher"))
            {
                return Unauthorized();
            }

            return Ok(new
            {
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Bio,
                user.Qualifications,
                user.Grade,
                user.RegistrationDate,
                user.LastActiveDate,
                //user.IsOnline,
                user.IsEmailVerified
            });
        }

        // Update user profile
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found");
            }

            // Check authorization
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId != id && !User.IsInRole("Teacher"))
            {
                return Unauthorized();
            }

            user.FirstName = model.FirstName ?? user.FirstName;
            user.LastName = model.LastName ?? user.LastName;
            user.Bio = model.Bio ?? user.Bio;
            user.Qualifications = model.Qualifications ?? user.Qualifications;
            user.Grade = model.Grade ?? user.Grade;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Ok(new { message = "User updated successfully" });
            }

            return BadRequest(result.Errors);
        }

        // Delete user
        [Authorize(Roles = "Teacher")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return Ok(new { message = "User deleted successfully" });
            }

            return BadRequest(result.Errors);
        }

        // Update user online status
        //[Authorize]
        //[HttpPost("update-status")]
        //public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusDto model)
        //{
        //    var user = await _userManager.GetUserAsync(User);
        //    if (user == null)
        //    {
        //        return NotFound("User not found");
        //    }

        //    user.IsOnline = model.IsOnline;
        //    user.LastActiveDate = DateTime.UtcNow;

        //    var result = await _userManager.UpdateAsync(user);
        //    if (result.Succeeded)
        //    {
        //        return Ok(new { message = "Status updated successfully" });
        //    }

        //    return BadRequest(result.Errors);
        //}
    }
}
