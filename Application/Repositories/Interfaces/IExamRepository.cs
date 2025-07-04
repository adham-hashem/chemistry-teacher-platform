using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Repositories.Interfaces
{
    public interface IExamRepository
    {
        Task<Exam> GetByIdAsync(Guid id);
        Task<List<Exam>> GetByLessonIdAsync(Guid lessonId);
        Task<List<Exam>> GetAllAsync();
        Task AddAsync(Exam exam);
        Task UpdateAsync(Exam exam);
        Task DeleteAsync(Guid id);
    }
}
