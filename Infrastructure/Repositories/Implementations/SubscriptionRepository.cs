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
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly AppDbContext _context;

        public SubscriptionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Subscription> GetByIdAsync(Guid id)
        {
            return await _context.Subscriptions
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<Subscription>> GetByUserIdAsync(string userId)
        {
            return await _context.Subscriptions
                .Include(s => s.User)
                .Where(s => s.UserId == userId)
                .ToListAsync();
        }

        public async Task<List<Subscription>> GetAllAsync()
        {
            return await _context.Subscriptions
                .Include(s => s.User)
                .ToListAsync();
        }

        public async Task AddAsync(Subscription subscription)
        {
            await _context.Subscriptions.AddAsync(subscription);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Subscription subscription)
        {
            _context.Subscriptions.Update(subscription);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var subscription = await _context.Subscriptions.FindAsync(id);
            if (subscription != null)
            {
                _context.Subscriptions.Remove(subscription);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteByUserIdAsync(string userId)
        {
            var subscriptions = await _context.Subscriptions
                .Where(s => s.UserId == userId)
                .ToListAsync();
            _context.Subscriptions.RemoveRange(subscriptions);
            await _context.SaveChangesAsync();
        }
    }
}
