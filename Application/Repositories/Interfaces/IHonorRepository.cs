using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Repositories.Interfaces
{
    public interface IHonorRepository
    {
        Task<Honor> GetByIdAsync(Guid id);
        Task<List<Honor>> GetByStudentIdAsync(string studentId);
        Task<List<Honor>> GetByTeacherIdAsync(string teacherId);
        Task<List<Honor>> GetAllAsync();
        Task AddAsync(Honor honor);
        Task UpdateAsync(Honor honor);
        Task DeleteAsync(Guid id);
    }
}
