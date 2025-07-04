using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Repositories.Interfaces
{
    public interface IPaymentRepository
    {
        Task AddAsync(Payment payment);
        Task<Payment> GetByTransactionIdAsync(string transactionId);
        Task UpdateAsync(Payment payment);
    }
}
