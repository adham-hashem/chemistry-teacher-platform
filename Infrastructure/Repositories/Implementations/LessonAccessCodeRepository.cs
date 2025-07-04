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
    public class LessonAccessCodeRepository : ILessonAccessCodeRepository
    {
        private readonly AppDbContext _context;

        public LessonAccessCodeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<LessonAccessCode> GetByIdAsync(Guid id)
        {
            return await _context.LessonAccessCodes
                .Include(lac => lac.Lesson)
                .Include(lac => lac.User)
                .FirstOrDefaultAsync(lac => lac.Id == id);
        }

        public async Task<LessonAccessCode> GetByCodeAsync(string code)
        {
            return await _context.LessonAccessCodes
                .Include(lac => lac.Lesson)
                .Include(lac => lac.User)
                .FirstOrDefaultAsync(lac => lac.Code == code);
        }

        public async Task<List<LessonAccessCode>> GetByLessonIdAsync(Guid lessonId)
        {
            return await _context.LessonAccessCodes
                .Include(lac => lac.Lesson)
                .Include(lac => lac.User)
                .Where(lac => lac.LessonId == lessonId)
                .ToListAsync();
        }

        public async Task<List<LessonAccessCode>> GetByUserIdAsync(string userId)
        {
            return await _context.LessonAccessCodes
                .Include(lac => lac.Lesson)
                .Include(lac => lac.User)
                .Where(lac => lac.UserId == userId)
                .ToListAsync();
        }

        public async Task AddAsync(LessonAccessCode code)
        {
            await _context.LessonAccessCodes.AddAsync(code);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(LessonAccessCode code)
        {
            _context.LessonAccessCodes.Update(code);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var code = await _context.LessonAccessCodes.FindAsync(id);
            if (code != null)
            {
                _context.LessonAccessCodes.Remove(code);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteByLessonIdAsync(Guid lessonId)
        {
            var codes = await _context.LessonAccessCodes
                .Where(lac => lac.LessonId == lessonId)
                .ToListAsync();
            _context.LessonAccessCodes.RemoveRange(codes);
            await _context.SaveChangesAsync();
        }
    }
}