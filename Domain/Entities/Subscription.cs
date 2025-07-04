using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;

namespace Domain.Entities
{
    public class Subscription
    {
        public Guid Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required]
        public SubscriptionType Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [Required]
        public List<int> SubscribedMonths { get; set; } = new List<int>(); // 1-12

        [Required]
        public EducationalLevel Grade { get; set; }
        public int? LectureCount { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
        public List<Guid> AccessedLessons { get; set; } = new List<Guid>();
        public bool IsActive { get; set; }
        public List<Payment> Payments { get; set; } = new List<Payment>();
    }
}
