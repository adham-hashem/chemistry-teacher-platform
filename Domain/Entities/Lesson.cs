using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Lesson
    {
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string VideoUrl { get; set; } = string.Empty;

        public string? LessonSummaryText { get; set; } = string.Empty;

        public string? LessonSummaryPdfPath { get; set; } = string.Empty;

        public string? EquationsTablePdfPath { get; set; } = string.Empty;

        public string? WorkPapersPdfPath { get; set; } = string.Empty;

        public bool IsFree { get; set; }

        public int? MonthAssigned { get; set; }

        public string? AdditionalResources { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Price { get; set; }

        public Guid CourseId { get; set; }

        public Course Course { get; set; }

        public List<Comment> Comments { get; set; } = new List<Comment>();

        public List<Question> Questions { get; set; } = new List<Question>();

        public List<Exam> Exams { get; set; } = new List<Exam>();

        public List<LessonAccessCode> LessonAccessCodes { get; set; } = new List<LessonAccessCode>();
        
    }
}
