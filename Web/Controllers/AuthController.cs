using Application.Dtos.AuthDtos;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                var tokenResponse = await _authService.RegisterAsync(registerDto);
                return Ok(tokenResponse);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var tokenResponse = await _authService.LoginAsync(loginDto);
                return Ok(tokenResponse);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            try
            {
                var tokenResponse = await _authService.RefreshTokenAsync(refreshToken);
                return Ok(tokenResponse);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeRefreshToken([FromBody] string refreshToken)
        {
            try
            {
                await _authService.RevokeRefreshTokenAsync(refreshToken);
                return Ok(new { message = "Refresh token revoked successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
