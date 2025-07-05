using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;

namespace Domain.Entities
{
    public class Payment
    {
        public Guid Id { get; set; }
        public Guid SubscriptionId { get; set; }
        public Subscription Subscription { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string PaymentMethod { get; set; } // e.g., "Card", "VodafoneCash", "EtisalatCash"
        public string TransactionId { get; set; }
        public string Status { get; set; } // e.g., "Pending", "Completed", "Failed"
        public DateTime CreatedAt { get; set; }
        public string? DiscountCode { get; set; }
        public decimal? OriginalAmount { get; set; }
    }
}
