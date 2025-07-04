using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos.CourseDtos;

namespace Application.Services.Interfaces
{
    public interface ICourseService
    {
        Task<CourseDto> GetByIdAsync(Guid id);
        Task<List<CourseDto>> GetAllAsync();
        Task<CourseDto> CreateAsync(CourseDto courseDto, string userId);
        Task UpdateAsync(CourseDto courseDto, string userId);
        Task DeleteAsync(Guid id, string userId);
    }
}
