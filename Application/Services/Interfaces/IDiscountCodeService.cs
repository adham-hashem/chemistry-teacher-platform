using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos.DiscountCodeDtos;

namespace Application.Services.Interfaces
{
    public interface IDiscountCodeService
    {
        Task<DiscountCodeDto> CreateDiscountCodeAsync(DiscountCodeCreateDto createDto, string teacherId);
        Task<DiscountCodeDto> GetByIdAsync(Guid id, string teacherId);
        Task<DiscountCodeDto> GetByCodeAsync(string code);
        Task<List<DiscountCodeDto>> GetByTeacherIdAsync(string teacherId);
        Task DeleteAsync(Guid id, string teacherId);
        Task<decimal> ValidateAndApplyDiscountAsync(string code, decimal originalAmount);
    }
}
