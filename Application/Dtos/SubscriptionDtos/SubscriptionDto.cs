using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.SubscriptionDtos
{
    public class SubscriptionDto
    {
        public Guid Id { get; set; }
        
        [Required]
        public string UserId { get; set; }
        
        [Required]
        public string Type { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        [Required]
        public List<int> SubscribedMonths { get; set; } = new List<int>();
        
        [Required]
        public string Grade { get; set; } = string.Empty;
        public int? LectureCount { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
        public List<Guid> AccessedLessons { get; set; } = new List<Guid>();
    }
}
