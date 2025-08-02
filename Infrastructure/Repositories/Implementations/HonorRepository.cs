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
    public class HonorRepository : IHonorRepository
    {
        private readonly AppDbContext _context;

        public HonorRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Honor> GetByIdAsync(Guid id)
        {
            return await _context.Honors
                .Include(h => h.Student)
                .Include(h => h.Teacher)
                .FirstOrDefaultAsync(h => h.Id == id);
        }

        public async Task<List<Honor>> GetByStudentIdAsync(string studentId)
        {
            return await _context.Honors
                .Include(h => h.Student)
                .Include(h => h.Teacher)
                .Where(h => h.StudentId == studentId)
                .ToListAsync();
        }

        public async Task<List<Honor>> GetByTeacherIdAsync(string teacherId)
        {
            return await _context.Honors
                .Include(h => h.Student)
                .Include(h => h.Teacher)
                .Where(h => h.TeacherId == teacherId)
                .ToListAsync();
        }

        public async Task<List<Honor>> GetAllAsync()
        {
            return await _context.Honors
                .Include(h => h.Student)
                .Include(h => h.Teacher)
                .ToListAsync();
        }

        public async Task AddAsync(Honor honor)
        {
            await _context.Honors.AddAsync(honor);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Honor honor)
        {
            _context.Honors.Update(honor);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var honor = await _context.Honors.FindAsync(id);
            if (honor != null)
            {
                _context.Honors.Remove(honor);
                await _context.SaveChangesAsync();
            }
        }
    }
}
