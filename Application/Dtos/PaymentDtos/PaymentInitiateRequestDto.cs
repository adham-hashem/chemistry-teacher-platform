using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.PaymentDtos
{
    public class PaymentInitiateRequestDto
    {
        public Guid SubscriptionId { get; set; }
        public decimal Amount { get; set; }
        public string DiscountCode { get; set; }
        public string PaymentMethod { get; set; }
    }
}
