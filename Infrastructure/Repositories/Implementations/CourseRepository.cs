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
    public class CourseRepository : ICourseRepository
    {
        private readonly AppDbContext _context;

        public CourseRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Course> GetByIdAsync(Guid id)
        {
            return await _context.Courses
                .Include(c => c.Lessons)
                .Include(c => c.Reviews)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Course>> GetAllAsync()
        {
            return await _context.Courses
                .Include(c => c.Lessons)
                .Include(c => c.Reviews)
                .ToListAsync();
        }

        public async Task AddAsync(Course course)
        {
            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Course course)
        {
            _context.Courses.Update(course);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
            }
        }
    }
}
