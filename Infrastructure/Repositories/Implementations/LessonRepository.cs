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
    public class LessonRepository : ILessonRepository
    {
        private readonly AppDbContext _context;

        public LessonRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Lesson> GetByIdAsync(Guid id)
        {
            return await _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<bool> HasAccessCodeAsync(Guid lessonId, string userId)
        {
            return await _context.LessonAccessCodes
                .AnyAsync(lac => lac.LessonId == lessonId && lac.UserId == userId && lac.IsUsed);
        }

        public async Task AddAsync(Lesson lesson)
        {
            await _context.Lessons.AddAsync(lesson);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Lesson lesson)
        {
            _context.Lessons.Update(lesson);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson != null)
            {
                _context.Lessons.Remove(lesson);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Lesson>> GetByCourseIdAsync(Guid courseId)
        {
            return await _context.Lessons
                .Include(l => l.Course)
                .Where(l => l.CourseId == courseId)
                .ToListAsync();
        }
    }
}
