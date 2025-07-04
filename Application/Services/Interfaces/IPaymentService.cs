using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos.PaymentDtos;

namespace Application.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentInitiateResponseDto> InitiatePaymentAsync(PaymentInitiateRequestDto request, string userId);
        Task ProcessPaymentCallbackAsync(PaymentCallbackDto callback);
    }
}
