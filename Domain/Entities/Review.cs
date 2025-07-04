using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Review
    {
        public Guid Id { get; set; }
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; } // 1-5 stars
        [StringLength(1000)]
        public string Comment { get; set; } = string.Empty;
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public Guid CourseId { get; set; }
        public Course Course { get; set; }
    }
}
