using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Repositories.Interfaces
{
    public interface ICourseRepository
    {
        Task<Course> GetByIdAsync(Guid id);
        Task<List<Course>> GetAllAsync();
        Task AddAsync(Course course);
        Task UpdateAsync(Course course);
        Task DeleteAsync(Guid id);
    }
}
