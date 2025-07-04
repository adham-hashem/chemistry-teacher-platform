using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Application.Dtos.LessonDtos
{
    public class LessonDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
        public string LessonSummaryText { get; set; } = string.Empty;
        public IFormFile? LessonSummaryPdf { get; set; }
        public IFormFile? EquationsTablePdf { get; set; }
        public IFormFile? WorkPapersPdf { get; set; }
        public string? LessonSummaryPdfPath { get; set; }
        public string? EquationsTablePdfPath { get; set; }
        public string? WorkPapersPdfPath { get; set; }
        public bool IsFree { get; set; }
        public int? MonthAssigned { get; set; }
        public string? AdditionalResources { get; set; }
        public Guid CourseId { get; set; }
        public bool IsAccessible { get; set; }
        public decimal? Price { get; set; }
    }
}
