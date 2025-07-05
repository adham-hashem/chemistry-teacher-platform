using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.PaymentDtos
{
    public class PaymentCallbackDto
    {
        public string PaymentStatus { get; set; } // SUCCESS or FAILURE
        public string CardDataToken { get; set; } // Card token for future payments
        public string MaskedCard { get; set; } // Masked card number
        public string MerchantOrderId { get; set; } // Merchant's order ID
        public string OrderId { get; set; } // Kashier's order ID
        public string CardBrand { get; set; } // Card brand (e.g., Visa, Mastercard)
        public string TransactionId { get; set; } // Transaction identifier
        public string Currency { get; set; } // Currency (e.g., EGP)
        public string Signature { get; set; } // HMAC signature for validation
        public string Mode { get; set; } // test or live
    }
}
