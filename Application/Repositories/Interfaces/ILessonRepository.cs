using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Repositories.Interfaces
{
    public interface ILessonRepository
    {
        Task<Lesson> GetByIdAsync(Guid id);
        Task<bool> HasAccessCodeAsync(Guid lessonId, string userId);
        Task AddAsync(Lesson lesson);
        Task UpdateAsync(Lesson lesson);
        Task DeleteAsync(Guid id);
        Task<List<Lesson>> GetByCourseIdAsync(Guid courseId);

    }
}
