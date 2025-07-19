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
        Task<ExamResultDto> GetByIdAsync(Guid id, string userId);
        Task<List<ExamResultDto>> GetByExamIdAsync(Guid examId, string userId);
        Task<ExamResultDto> GetByUserIdAndExamIdAsync(string userId, Guid examId);
        Task<ExamResultDto> SubmitAsync(SubmitExamDto submitExamDto, string userId);
        Task DeleteAsync(Guid id, string userId);
        //Task<bool> HasLessonAccessAsync(string userId, Guid lessonId);
    }
}
