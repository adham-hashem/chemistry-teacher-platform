using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;

namespace Application.Dtos.ExamDtos
{
    public class ExamDto
    {
        public Guid Id { get; set; }
        public Guid LessonId { get; set; }
        public string Title { get; set; }
        public List<McqQuestionDto>? Questions { get; set; }
        public string? PdfPath { get; set; }
        public ExamType ExamType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public decimal CertificateThreshold { get; set; }
    }
}
