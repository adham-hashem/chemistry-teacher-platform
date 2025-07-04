using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos.AuthDtos;
using Application.Repositories.Interfaces;
using Application.Services.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IJwtService _jwtService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IJwtService jwtService,
            IRefreshTokenRepository refreshTokenRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtService = jwtService;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<TokenResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            var user = new ApplicationUser
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Grade = Enum.Parse<Domain.Enums.EducationalLevel>(registerDto.Grade),
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
                throw new Exception(string.Join("; ", result.Errors.Select(e => e.Description)));

            //if (!await _roleManager.RoleExistsAsync("registerDto.Role"))
            //    await _roleManager.CreateAsync(new IdentityRole(registerDto.Role));

            await _userManager.AddToRoleAsync(user, "Student");

            return await _jwtService.GenerateJwtToken(user);
        }

        public async Task<TokenResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
                throw new Exception("Invalid email or password.");

            return await _jwtService.GenerateJwtToken(user);
        }

        public async Task<TokenResponseDto> RefreshTokenAsync(string refreshToken)
        {
            var isValid = await _jwtService.ValidateRefreshTokenAsync(refreshToken);
            if (!isValid)
                throw new Exception("Invalid or expired refresh token.");

            var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
            var user = await _userManager.FindByIdAsync(token.UserId);
            if (user == null)
                throw new Exception("User not found.");

            await _refreshTokenRepository.RevokeAsync(refreshToken);
            return await _jwtService.GenerateJwtToken(user);
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            await _refreshTokenRepository.RevokeAsync(refreshToken);
        }
    }
}
