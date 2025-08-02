using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Honor
    {
        public Guid Id { get; set; }

        [Required]
        public string StudentId { get; set; }
        public ApplicationUser Student { get; set; }

        [Required]
        public string TeacherId { get; set; }
        public ApplicationUser Teacher { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        public string? StudentImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
