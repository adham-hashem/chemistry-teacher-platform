using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Repositories.Interfaces
{
    public interface ISubscriptionRepository
    {
        Task<Subscription> GetByIdAsync(Guid id);
        Task<List<Subscription>> GetByUserIdAsync(string userId);
        Task<List<Subscription>> GetAllAsync();
        Task AddAsync(Subscription subscription);
        Task UpdateAsync(Subscription subscription);
        Task DeleteAsync(Guid id);
        Task DeleteByUserIdAsync(string userId);
    }
}
