using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Repositories.Interfaces
{
    public interface ICertificateRepository
    {
        Task<Certificate> GetByIdAsync(Guid id);
        Task<List<Certificate>> GetByStudentIdAsync(string studentId);
        Task<List<Certificate>> GetByExamIdAsync(Guid examId);
        Task AddAsync(Certificate certificate);
        Task UpdateAsync(Certificate certificate);
        Task DeleteAsync(Guid id);
    }
}
