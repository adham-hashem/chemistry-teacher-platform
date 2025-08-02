using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Application.Repositories.Interfaces;
    using Domain.Entities;
    using global::Infrastructure.Data;
    using Microsoft.EntityFrameworkCore;

    namespace Infrastructure.Repositories
    {
        public class CertificateRepository : ICertificateRepository
        {
            private readonly AppDbContext _context;

            public CertificateRepository(AppDbContext context)
            {
                _context = context;
            }

            public async Task<Certificate> GetByIdAsync(Guid id)
            {
                return await _context.Certificates
                    .Include(c => c.Student)
                    .Include(c => c.Exam)
                    .FirstOrDefaultAsync(c => c.Id == id);
            }

            public async Task<List<Certificate>> GetByStudentIdAsync(string studentId)
            {
                return await _context.Certificates
                    .Include(c => c.Student)
                    .Include(c => c.Exam)
                    .Where(c => c.StudentId == studentId)
                    .ToListAsync();
            }

            public async Task<List<Certificate>> GetByExamIdAsync(Guid examId)
            {
                return await _context.Certificates
                    .Include(c => c.Student)
                    .Include(c => c.Exam)
                    .Where(c => c.ExamId == examId)
                    .ToListAsync();
            }

            public async Task AddAsync(Certificate certificate)
            {
                await _context.Certificates.AddAsync(certificate);
                await _context.SaveChangesAsync();
            }

            public async Task UpdateAsync(Certificate certificate)
            {
                _context.Certificates.Update(certificate);
                await _context.SaveChangesAsync();
            }

            public async Task DeleteAsync(Guid id)
            {
                var certificate = await _context.Certificates.FindAsync(id);
                if (certificate != null)
                {
                    _context.Certificates.Remove(certificate);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
