using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Application.Dtos.ExamDtos
{
    public class McqQuestionDto
    {
        public Guid Id { get; set; }
        public string QuestionText { get; set; }
        public List<string> Options { get; set; }
        public int CorrectOptionIndex { get; set; }
        public string? ImageUrl { get; set; }
        public IFormFile? ImageFile { get; set; } 
        public int TimeInSeconds { get; set; }
        public int DefaultOptionIndex { get; set; }
    }
}
