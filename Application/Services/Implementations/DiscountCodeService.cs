using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos.DiscountCodeDtos;
using Application.Repositories.Interfaces;
using Application.Services.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Services.Implementations
{
    public class DiscountCodeService : IDiscountCodeService
    {
        private readonly IDiscountCodeRepository _discountCodeRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public DiscountCodeService(
            IDiscountCodeRepository discountCodeRepository,
            UserManager<ApplicationUser> userManager)
        {
            _discountCodeRepository = discountCodeRepository;
            _userManager = userManager;
        }

        public async Task<DiscountCodeDto> CreateDiscountCodeAsync(DiscountCodeCreateDto createDto, string teacherId)
        {
            var teacher = await _userManager.FindByIdAsync(teacherId);
            if (teacher == null || !await _userManager.IsInRoleAsync(teacher, "Teacher"))
                throw new Exception("Only teachers can create discount codes.");

            if (createDto.DiscountPercentage <= 0 || createDto.DiscountPercentage > 100)
                throw new Exception("Discount percentage must be between 1 and 100.");

            if (createDto.ValidUntil <= createDto.ValidFrom)
                throw new Exception("Valid until date must be after valid from date.");

            if (createDto.MaxUses.HasValue && createDto.MaxUses <= 0)
                throw new Exception("Max uses must be greater than zero if specified.");

            var existingCode = await _discountCodeRepository.GetByCodeAsync(createDto.Code);
            if (existingCode != null)
                throw new Exception("Discount code already exists.");

            var discountCode = new DiscountCode
            {
                Id = Guid.NewGuid(),
                Code = createDto.Code.ToUpper(),
                DiscountPercentage = createDto.DiscountPercentage,
                ValidFrom = createDto.ValidFrom,
                ValidUntil = createDto.ValidUntil,
                MaxUses = createDto.MaxUses,
                Uses = 0,
                IsActive = true,
                TeacherId = teacherId
            };

            await _discountCodeRepository.AddAsync(discountCode);

            return new DiscountCodeDto
            {
                Id = discountCode.Id,
                Code = discountCode.Code,
                DiscountPercentage = discountCode.DiscountPercentage,
                ValidFrom = discountCode.ValidFrom,
                ValidUntil = discountCode.ValidUntil,
                MaxUses = discountCode.MaxUses,
                Uses = discountCode.Uses,
                IsActive = discountCode.IsActive,
                TeacherId = discountCode.TeacherId
            };
        }

        public async Task<DiscountCodeDto> GetByIdAsync(Guid id, string teacherId)
        {
            var teacher = await _userManager.FindByIdAsync(teacherId);
            if (teacher == null || !await _userManager.IsInRoleAsync(teacher, "Teacher"))
                throw new Exception("Only teachers can view discount codes.");

            var discountCode = await _discountCodeRepository.GetByIdAsync(id);
            if (discountCode == null)
                throw new Exception("Discount code not found.");

            if (discountCode.TeacherId != teacherId)
                throw new Exception("You can only view your own discount codes.");

            return new DiscountCodeDto
            {
                Id = discountCode.Id,
                Code = discountCode.Code,
                DiscountPercentage = discountCode.DiscountPercentage,
                ValidFrom = discountCode.ValidFrom,
                ValidUntil = discountCode.ValidUntil,
                MaxUses = discountCode.MaxUses,
                Uses = discountCode.Uses,
                IsActive = discountCode.IsActive,
                TeacherId = discountCode.TeacherId
            };
        }

        public async Task<DiscountCodeDto> GetByCodeAsync(string code)
        {
            var discountCode = await _discountCodeRepository.GetByCodeAsync(code);
            if (discountCode == null)
                throw new Exception("Discount code not found.");

            return new DiscountCodeDto
            {
                Id = discountCode.Id,
                Code = discountCode.Code,
                DiscountPercentage = discountCode.DiscountPercentage,
                ValidFrom = discountCode.ValidFrom,
                ValidUntil = discountCode.ValidUntil,
                MaxUses = discountCode.MaxUses,
                Uses = discountCode.Uses,
                IsActive = discountCode.IsActive,
                TeacherId = discountCode.TeacherId
            };
        }

        public async Task<List<DiscountCodeDto>> GetByTeacherIdAsync(string teacherId)
        {
            var teacher = await _userManager.FindByIdAsync(teacherId);
            if (teacher == null || !await _userManager.IsInRoleAsync(teacher, "Teacher"))
                throw new Exception("Only teachers can view discount codes.");

            var discountCodes = await _discountCodeRepository.GetByTeacherIdAsync(teacherId);
            return discountCodes.Select(dc => new DiscountCodeDto
            {
                Id = dc.Id,
                Code = dc.Code,
                DiscountPercentage = dc.DiscountPercentage,
                ValidFrom = dc.ValidFrom,
                ValidUntil = dc.ValidUntil,
                MaxUses = dc.MaxUses,
                Uses = dc.Uses,
                IsActive = dc.IsActive,
                TeacherId = dc.TeacherId
            }).ToList();
        }

        public async Task DeleteAsync(Guid id, string teacherId)
        {
            var teacher = await _userManager.FindByIdAsync(teacherId);
            if (teacher == null || !await _userManager.IsInRoleAsync(teacher, "Teacher"))
                throw new Exception("Only teachers can delete discount codes.");

            var discountCode = await _discountCodeRepository.GetByIdAsync(id);
            if (discountCode == null)
                throw new Exception("Discount code not found.");

            if (discountCode.TeacherId != teacherId)
                throw new Exception("You can only delete your own discount codes.");

            await _discountCodeRepository.DeleteAsync(id);
        }

        public async Task<decimal> ValidateAndApplyDiscountAsync(string code, decimal originalAmount)
        {
            var discountCode = await _discountCodeRepository.GetByCodeAsync(code);
            if (discountCode == null)
                throw new Exception("Invalid discount code.");

            if (!discountCode.IsActive)
                throw new Exception("Discount code is not active.");

            if (DateTime.UtcNow < discountCode.ValidFrom || DateTime.UtcNow > discountCode.ValidUntil)
                throw new Exception("Discount code is not valid at this time.");

            if (discountCode.MaxUses.HasValue && discountCode.Uses >= discountCode.MaxUses)
                throw new Exception("Discount code has reached its maximum usage limit.");

            discountCode.Uses++;
            await _discountCodeRepository.UpdateAsync(discountCode);

            var discountAmount = originalAmount * (discountCode.DiscountPercentage / 100);
            return Math.Max(0, originalAmount - discountAmount);
        }
    }
}
