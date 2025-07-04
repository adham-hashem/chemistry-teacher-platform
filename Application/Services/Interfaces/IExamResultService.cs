using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos.ExamDtos;

namespace Application.Services.Interfaces
{
    public interface IExamResultService
    {
        Task<ExamResultDto> GetByIdAsync(Guid id);
        Task<List<ExamResultDto>> GetByExamIdAsync(Guid examId);
        Task<List<ExamResultDto>> GetByUserIdAsync(string userId);
        Task<ExamResultDto> SubmitAsync(ExamResultDto examResultDto, string userId);
        Task DeleteAsync(Guid id, string userId);
        Task<bool> HasLessonAccessAsync(string userId, Guid lessonId);
    }
}
