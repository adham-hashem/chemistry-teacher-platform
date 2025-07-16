using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;

namespace Domain.Entities
{
    public class Notification
    {
        public Guid Id { get; set; }

        [Required]
        public NotificationType Type { get; set; } // "NewStudent", "NewEnrollment", "NewQuestion", "NewReview", "PaymentReceived", "ProfitDeposit", "PlatformUpdate", "TeachingTip"

        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
