using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos.ExamDtos;

namespace Application.Services.Interfaces
{
    public interface IExamService
    {
        Task<ExamDto> GetByIdAsync(Guid id);
        Task<List<ExamDto>> GetByLessonIdAsync(Guid lessonId);
        Task<List<ExamDto>> GetAllAsync();
        Task<ExamDto> CreateAsync(ExamDto examDto, string userId);
        Task<ExamDto> UpdateAsync(Guid id, ExamDto examDto, string userId);
        Task DeleteAsync(Guid id, string userId);
    }
}
