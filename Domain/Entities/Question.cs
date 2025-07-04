using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Question
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(1000)]
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public Guid LessonId { get; set; }
        public Lesson Lesson { get; set; }

        [StringLength(1000)]
        public string? Answer { get; set; }
    }
}
