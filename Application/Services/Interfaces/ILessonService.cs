using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos.LessonDtos;

namespace Application.Services.Interfaces
{
    public interface ILessonService
    {
        Task<LessonDto> CreateAsync(LessonDto lessonDto, string userId);
        Task UpdateAsync(LessonDto lessonDto, string userId);
        Task DeleteAsync(Guid id, string userId);
        Task<List<LessonDto>> GetLessonsByCourseAsync(Guid courseId, string userId);
        Task<LessonDto> GetLessonByIdAsync(Guid lessonId, string userId);
    }
}
