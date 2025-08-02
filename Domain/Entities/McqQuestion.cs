using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class McqQuestion
    {
        public Guid Id { get; set; }

        [Required]
        public string QuestionText { get; set; } = string.Empty;

        [Required]
        public List<string> Options { get; set; } = new List<string>();

        [Required]
        [Range(0, int.MaxValue)]
        public int CorrectOptionIndex { get; set; }
        public string? ImageUrl { get; set; }

        [Range(1, int.MaxValue)]
        public int TimeInSeconds { get; set; } = 60;

        [Range(0, int.MaxValue)]
        public int DefaultOptionIndex { get; set; } = -1;

        public Guid ExamId { get; set; }

        public Exam Exam { get; set; }
    }
}
