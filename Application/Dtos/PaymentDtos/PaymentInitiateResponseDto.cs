using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.PaymentDtos
{
    public class PaymentInitiateResponseDto
    {
        public string PaymentToken { get; set; }
        public string IframeUrl { get; set; }
        public object IframeAttributes { get; set; } // Contains Kashier iframe script attributes
    }
}
