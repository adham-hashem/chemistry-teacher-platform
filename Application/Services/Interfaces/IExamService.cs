using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos.ExamDtos;
using Microsoft.AspNetCore.Http;

namespace Application.Services.Interfaces
{
    public interface IExamService
    {
        Task<ExamDto> GetByIdAsync(Guid id);
        Task<List<ExamDto>> GetByLessonIdAsync(Guid lessonId, string userId);
        Task<List<ExamDto>> GetAllAsync();
        Task<ExamDto> CreateAsync(ExamDto examDto, string userId, IFormFile? examPdf = null, List<IFormFile>? questionImageFiles = null);
        Task<ExamDto> UpdateAsync(Guid id, ExamDto examDto, string userId, IFormFile? examPdf = null, List<IFormFile>? questionImageFiles = null);
        Task DeleteAsync(Guid id, string userId);
    }
}
