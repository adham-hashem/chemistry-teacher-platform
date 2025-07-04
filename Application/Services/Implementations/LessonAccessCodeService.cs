using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Dtos.LessonAccessCodeDtos;
using Application.Dtos.RedeemCodeDtos;
using Application.Repositories.Interfaces;
using Application.Services.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Services.Implementations
{
    public class LessonAccessCodeService : ILessonAccessCodeService
    {
        private readonly ILessonAccessCodeRepository _codeRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public LessonAccessCodeService(
            ILessonAccessCodeRepository codeRepository,
            ILessonRepository lessonRepository,
            UserManager<ApplicationUser> userManager)
        {
            _codeRepository = codeRepository;
            _lessonRepository = lessonRepository;
            _userManager = userManager;
        }

        public async Task<LessonAccessCodeDto> GenerateCodeAsync(Guid lessonId, string teacherId)
        {
            var teacher = await _userManager.FindByIdAsync(teacherId);
            if (teacher == null || !await _userManager.IsInRoleAsync(teacher, "Teacher"))
                throw new Exception("Only teachers can generate codes.");

            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (lesson == null)
                throw new Exception("Lesson not found.");

            var code = new LessonAccessCode
            {
                Id = Guid.NewGuid(),
                Code = GenerateUniqueCode(),
                LessonId = lessonId,
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            await _codeRepository.AddAsync(code);

            return new LessonAccessCodeDto
            {
                Id = code.Id,
                Code = code.Code,
                LessonId = code.LessonId,
                UserId = code.UserId,
                IsUsed = code.IsUsed,
                CreatedAt = code.CreatedAt,
                RedeemedAt = code.RedeemedAt
            };
        }

        public async Task<LessonAccessCodeDto> RedeemCodeAsync(RedeemCodeDto redeemCodeDto, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Student"))
                throw new Exception("Only students can redeem codes.");

            var code = await _codeRepository.GetByCodeAsync(redeemCodeDto.Code);
            if (code == null)
                throw new Exception("Invalid code.");

            if (code.IsUsed)
                throw new Exception("Code has already been used.");

            code.IsUsed = true;
            code.UserId = userId;
            code.RedeemedAt = DateTime.UtcNow;

            await _codeRepository.UpdateAsync(code);

            return new LessonAccessCodeDto
            {
                Id = code.Id,
                Code = code.Code,
                LessonId = code.LessonId,
                UserId = code.UserId,
                IsUsed = code.IsUsed,
                CreatedAt = code.CreatedAt,
                RedeemedAt = code.RedeemedAt
            };
        }

        public async Task<List<LessonAccessCodeDto>> GetCodesByLessonAsync(Guid lessonId, string teacherId)
        {
            var teacher = await _userManager.FindByIdAsync(teacherId);
            if (teacher == null || !await _userManager.IsInRoleAsync(teacher, "Teacher"))
                throw new Exception("Only teachers can view codes.");

            var codes = await _codeRepository.GetByLessonIdAsync(lessonId);
            return codes.Select(code => new LessonAccessCodeDto
            {
                Id = code.Id,
                Code = code.Code,
                LessonId = code.LessonId,
                UserId = code.UserId,
                IsUsed = code.IsUsed,
                CreatedAt = code.CreatedAt,
                RedeemedAt = code.RedeemedAt
            }).ToList();
        }

        public async Task RevokeAccessAsync(Guid lessonId, string userId, string teacherId)
        {
            var teacher = await _userManager.FindByIdAsync(teacherId);
            if (teacher == null || !await _userManager.IsInRoleAsync(teacher, "Teacher"))
                throw new Exception("Only teachers can revoke access.");

            var code = await _codeRepository.GetByLessonIdAsync(lessonId)
                .ContinueWith(task => task.Result.FirstOrDefault(lac => lac.UserId == userId && lac.IsUsed));
            if (code == null)
                throw new Exception("No access found for this user and lesson.");

            code.IsUsed = false;
            code.UserId = null;
            code.RedeemedAt = null;

            await _codeRepository.UpdateAsync(code);
        }

        public async Task RevokeAllAccessAsync(Guid lessonId, string teacherId)
        {
            var teacher = await _userManager.FindByIdAsync(teacherId);
            if (teacher == null || !await _userManager.IsInRoleAsync(teacher, "Teacher"))
                throw new Exception("Only teachers can revoke access.");

            var codes = await _codeRepository.GetByLessonIdAsync(lessonId);
            foreach (var code in codes.Where(c => c.IsUsed))
            {
                code.IsUsed = false;
                code.UserId = null;
                code.RedeemedAt = null;
                await _codeRepository.UpdateAsync(code);
            }
        }

        private string GenerateUniqueCode()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper();
        }
    }
}