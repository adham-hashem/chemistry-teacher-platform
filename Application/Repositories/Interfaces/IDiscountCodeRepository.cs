using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Repositories.Interfaces
{
    public interface IDiscountCodeRepository
    {
        Task<DiscountCode> GetByIdAsync(Guid id);
        Task<DiscountCode> GetByCodeAsync(string code);
        Task<List<DiscountCode>> GetByTeacherIdAsync(string teacherId);
        Task AddAsync(DiscountCode discountCode);
        Task UpdateAsync(DiscountCode discountCode);
        Task DeleteAsync(Guid id);
    }
}
