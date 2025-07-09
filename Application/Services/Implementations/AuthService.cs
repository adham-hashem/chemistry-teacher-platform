using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
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
        private readonly IEmailService _emailService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IJwtService jwtService,
            IRefreshTokenRepository refreshTokenRepository,
            IEmailService emailService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtService = jwtService;
            _refreshTokenRepository = refreshTokenRepository;
            _emailService = emailService;
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

            await _userManager.AddToRoleAsync(user, "Student");

            return await _jwtService.GenerateJwtToken(user);
        }

        public async Task<TokenResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
                throw new Exception("Invalid email or password.");

            var roles = await _userManager.GetRolesAsync(user);
            if (roles == null || !roles.Any())
                throw new Exception("User has no assigned roles.");

            return await _jwtService.GenerateJwtToken(user, roles);
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

            var roles = await _userManager.GetRolesAsync(user);
            if (roles == null || !roles.Any())
                throw new Exception("User has no assigned roles.");

            await _refreshTokenRepository.RevokeAsync(refreshToken);
            return await _jwtService.GenerateJwtToken(user, roles);
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            await _refreshTokenRepository.RevokeAsync(refreshToken);
        }

        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new Exception("User not found.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = $"https://yourapp.com/reset-password?email={email}&token={Uri.EscapeDataString(token)}";

            var subject = "Password Reset Request";
            var body = $@"<!DOCTYPE html>
                            <html lang=""en"">
                            <head>
                                <meta charset=""UTF-8"">
                                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                <style>
                                    body {{
                                        font-family: Arial, sans-serif;
                                        background-color: #f4f4f4;
                                        margin: 0;
                                        padding: 0;
                                    }}
                                    .container {{
                                        max-width: 600px;
                                        margin: 20px auto;
                                        background-color: #ffffff;
                                        padding: 20px;
                                        border-radius: 8px;
                                        box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                                    }}
                                    .header {{
                                        text-align: center;
                                        padding: 20px 0;
                                        background-color: #007bff;
                                        color: #ffffff;
                                        border-radius: 8px 8px 0 0;
                                    }}
                                    .header h1 {{
                                        margin: 0;
                                        font-size: 24px;
                                    }}
                                    .content {{
                                        padding: 20px;
                                        line-height: 1.6;
                                        color: #333333;
                                    }}
                                    .button {{
                                        display: inline-block;
                                        padding: 12px 24px;
                                        background-color: #007bff;
                                        color: #ffffff !important;
                                        text-decoration: none;
                                        border-radius: 5px;
                                        font-weight: bold;
                                        margin: 20px 0;
                                        text-align: center;
                                    }}
                                    .button:hover {{
                                        background-color: #0056b3;
                                    }}
                                    .footer {{
                                        text-align: center;
                                        padding: 20px;
                                        font-size: 12px;
                                        color: #777777;
                                    }}
                                    @media only screen and (max-width: 600px) {{
                                        .container {{
                                            margin: 10px;
                                            padding: 10px;
                                        }}
                                        .header h1 {{
                                            font-size: 20px;
                                        }}
                                        .button {{
                                            padding: 10px 20px;
                                            font-size: 14px;
                                        }}
                                    }}
                                </style>
                            </head>
                            <body>
                                <div class=""container"">
                                    <div class=""header"">
                                        <h1>Password Reset Request</h1>
                                    </div>
                                    <div class=""content"">
                                        <p>Hello {user.FirstName},</p>
                                        <p>We received a request to reset your password. Click the button below to set a new password:</p>
                                        <p style=""text-align: center;"">
                                            <a href=""{callbackUrl}"" class=""button"">Reset Your Password</a>
                                        </p>
                                        <p>If you did not request a password reset, please ignore this email or contact our support team.</p>
                                        <p>Thank you,<br>Chemistry Teacher</p>
                                    </div>
                                    <div class=""footer"">
                                        <p>© {DateTime.UtcNow.Year} YourApp. All rights reserved.</p>
                                        <p>If you have any questions, contact us at <a href=""mailto:adhamhashem2025@gmail.com"">adhamhashem2025@gmail.com</a>.</p>
                                    </div>
                                </div>
                            </body>
                            </html>";

            await _emailService.SendEmailAsync(user.Email, subject, body, isHtml: true);
        }

        public async Task ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null)
                throw new Exception("User not found.");

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);
            if (!result.Succeeded)
                throw new Exception(string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        public async Task SendEmailVerificationAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new Exception("User not found.");

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            user.EmailVerificationToken = token;
            await _userManager.UpdateAsync(user);

            var encodedToken = HttpUtility.UrlEncode(token);
            var verificationLink = $"https://yourapp.com/verify-email?email={HttpUtility.UrlEncode(email)}&token={encodedToken}";
            var emailBody = $"<p>Please verify your email by clicking <a href='{verificationLink}'>here</a>.</p>";

            await _emailService.SendEmailAsync(email, "Verify Your Email", emailBody, isHtml: true);
        }

        public async Task VerifyEmailAsync(VerifyEmailDto verifyEmailDto)
        {
            var user = await _userManager.FindByEmailAsync(verifyEmailDto.Email);
            if (user == null)
                throw new Exception("User not found.");

            var result = await _userManager.ConfirmEmailAsync(user, verifyEmailDto.Token);
            if (!result.Succeeded)
                throw new Exception("Invalid or expired verification token.");

            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
            await _userManager.UpdateAsync(user);
        }
    }
}