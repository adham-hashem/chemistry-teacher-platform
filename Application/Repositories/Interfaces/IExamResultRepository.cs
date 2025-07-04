using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Repositories.Interfaces
{
    public interface IExamResultRepository  
    {
        Task<ExamResult> GetByIdAsync(Guid id);
        Task<List<ExamResult>> GetByExamIdAsync(Guid examId);
        Task<List<ExamResult>> GetByUserIdAsync(string userId);
        Task AddAsync(ExamResult examResult);
        Task UpdateAsync(ExamResult examResult);
        Task DeleteAsync(Guid id);
    }
}
