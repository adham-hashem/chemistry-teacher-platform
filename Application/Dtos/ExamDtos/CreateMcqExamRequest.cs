using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Application.Dtos.ExamDtos
{
    public class CreateMcqExamRequest
    {
        public Guid LessonId { get; set; }
        public string Title { get; set; }
        public string QuestionsJson { get; set; }
        public decimal CertificateThreshold { get; set; }
        public List<IFormFile>? QuestionImageFiles { get; set; }
    }
}
