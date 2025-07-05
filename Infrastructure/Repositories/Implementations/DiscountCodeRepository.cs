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
    public class DiscountCodeRepository : IDiscountCodeRepository
    {
        private readonly AppDbContext _context;

        public DiscountCodeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DiscountCode> GetByIdAsync(Guid id)
        {
            return await _context.DiscountCodes
                .Include(dc => dc.Teacher)
                .FirstOrDefaultAsync(dc => dc.Id == id);
        }

        public async Task<DiscountCode> GetByCodeAsync(string code)
        {
            return await _context.DiscountCodes
                .Include(dc => dc.Teacher)
                .FirstOrDefaultAsync(dc => dc.Code == code);
        }

        public async Task<List<DiscountCode>> GetByTeacherIdAsync(string teacherId)
        {
            return await _context.DiscountCodes
                .Include(dc => dc.Teacher)
                .Where(dc => dc.TeacherId == teacherId)
                .ToListAsync();
        }

        public async Task AddAsync(DiscountCode discountCode)
        {
            await _context.DiscountCodes.AddAsync(discountCode);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(DiscountCode discountCode)
        {
            _context.DiscountCodes.Update(discountCode);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var discountCode = await _context.DiscountCodes.FindAsync(id);
            if (discountCode != null)
            {
                _context.DiscountCodes.Remove(discountCode);
                await _context.SaveChangesAsync();
            }
        }
    }
}
