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
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _context;

        public PaymentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Payment payment)
        {
            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();
        }

        public async Task<Payment> GetByTransactionIdAsync(string transactionId)
        {
            return await _context.Payments.FirstOrDefaultAsync(p => p.TransactionId == transactionId);
        }

        public async Task UpdateAsync(Payment payment)
        {
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();
        }
    }
}
