using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class LessonAccessCode
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;
        public Guid LessonId { get; set; }
        public Lesson Lesson { get; set; }
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
        public bool IsUsed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RedeemedAt { get; set; }
    }
}
