using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Repositories.Interfaces
{
    public interface ILessonAccessCodeRepository
    {
        Task<LessonAccessCode> GetByIdAsync(Guid id);
        Task<LessonAccessCode> GetByCodeAsync(string code);
        Task<List<LessonAccessCode>> GetByLessonIdAsync(Guid lessonId);
        Task<List<LessonAccessCode>> GetByUserIdAsync(string userId);
        Task AddAsync(LessonAccessCode code);
        Task UpdateAsync(LessonAccessCode code);
        Task DeleteAsync(Guid id);
        Task DeleteByLessonIdAsync(Guid lessonId);
    }
}
