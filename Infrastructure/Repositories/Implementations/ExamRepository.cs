using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Repositories.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Implementations
{
    public class ExamRepository : IExamRepository
    {
        private readonly AppDbContext _context;

        public ExamRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Exam> GetByIdAsync(Guid id)
        {
            return await _context.Exams
                .Include(e => e.Lesson)
                .Include(e => e.ExamResults)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<Exam>> GetByLessonIdAsync(Guid lessonId)
        {
            return await _context.Exams
                .Include(e => e.Lesson)
                .Include(e => e.ExamResults)
                .Where(e => e.LessonId == lessonId)
                .ToListAsync();
        }

        public async Task<List<Exam>> GetAllAsync()
        {
            return await _context.Exams
                .Include(e => e.Lesson)
                .Include(e => e.ExamResults)
                .ToListAsync();
        }

        public async Task AddAsync(Exam exam)
        {
            await _context.Exams.AddAsync(exam);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Exam exam)
        {
            _context.Exams.Update(exam);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var exam = await _context.Exams.FindAsync(id);
            if (exam != null)
            {
                _context.Exams.Remove(exam);
                await _context.SaveChangesAsync();
            }
        }
    }
}
