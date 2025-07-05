using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Repositories.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Implementations
{
    public class ExamResultRepository : IExamResultRepository
    {
        private readonly AppDbContext _context;

        public ExamResultRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ExamResult> GetByIdAsync(Guid id)
        {
            return await _context.ExamResults
                .AsNoTracking()
                .FirstOrDefaultAsync(er => er.Id == id);
        }

        public async Task<List<ExamResult>> GetByExamIdAsync(Guid examId)
        {
            return await _context.ExamResults
                .AsNoTracking()
                .Where(er => er.ExamId == examId)
                .ToListAsync();
        }

        public async Task<List<ExamResult>> GetByUserIdAsync(string userId)
        {
            return await _context.ExamResults
                .AsNoTracking()
                .Where(er => er.UserId == userId)
                .ToListAsync();
        }

        public async Task<ExamResult> GetByExamIdAndUserIdAsync(Guid examId, string userId)
        {
            return await _context.ExamResults
                .AsNoTracking()
                .FirstOrDefaultAsync(er => er.ExamId == examId && er.UserId == userId);
        }

        public async Task AddAsync(ExamResult examResult)
        {
            await _context.ExamResults.AddAsync(examResult);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ExamResult examResult)
        {
            _context.ExamResults.Update(examResult);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var examResult = await _context.ExamResults.FindAsync(id);
            if (examResult == null)
                throw new KeyNotFoundException("Exam result not found.");

            _context.ExamResults.Remove(examResult);
            await _context.SaveChangesAsync();
        }
    }
}