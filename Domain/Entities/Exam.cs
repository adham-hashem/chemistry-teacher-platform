using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;

namespace Domain.Entities
{
    public class Exam
    {
        public Guid Id { get; set; }
        public Guid LessonId { get; set; }
        public Lesson Lesson { get; set; }
        public string Title { get; set; }
        public List<McqQuestion> Questions { get; set; } = new List<McqQuestion>();
        public string? PdfPath { get; set; }
        public ExamType ExamType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<ExamResult> ExamResults { get; set; } = new List<ExamResult>();
        public decimal CertificateThreshold { get; set; } = 90.0m;
        public List<Certificate> Certificates { get; set; } = new List<Certificate>();
    }
}
