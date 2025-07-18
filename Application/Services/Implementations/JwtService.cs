﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos.AuthDtos;
using Application.Repositories.Interfaces;
using Application.Services.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace Application.Services.Implementations
{
    public class JwtService : IJwtService
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public JwtService(
            IRefreshTokenRepository refreshTokenRepository,
            UserManager<ApplicationUser> userManager)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _userManager = userManager;
        }

        public async Task<TokenResponseDto> GenerateJwtToken(ApplicationUser user, IList<string> roles = null)
        {
            // Fetch roles if not provided
            var userRoles = roles ?? await _userManager.GetRolesAsync(user);
            if (userRoles == null)
                throw new Exception("Unable to retrieve user roles.");

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

            var secretKey = Environment.GetEnvironmentVariable("CTP_DEV_JWT_SECRET");
            if (string.IsNullOrEmpty(secretKey))
                throw new Exception("JWT secret key is not configured.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var accessTokenExpires = DateTime.UtcNow.AddDays(4);
            var token = new JwtSecurityToken(
                issuer: Environment.GetEnvironmentVariable("CTP_DEV_JWT_ISSUER"),
                audience: null,
                claims: claims,
                expires: accessTokenExpires,
                signingCredentials: creds);

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = Guid.NewGuid().ToString(),
                Expires = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            await _refreshTokenRepository.AddAsync(refreshToken);

            return new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                AccessTokenExpires = accessTokenExpires,
                Roles = userRoles.ToList()
            };
        }

        public async Task<bool> ValidateRefreshTokenAsync(string refreshToken)
        {
            var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
            return token != null && !token.IsRevoked && token.Expires > DateTime.UtcNow;
        }
    }
}