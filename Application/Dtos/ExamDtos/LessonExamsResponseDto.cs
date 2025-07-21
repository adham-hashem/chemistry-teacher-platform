using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.ExamDtos
{
    public class LessonExamsResponseDto
    {
        public string LessonTitle { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public List<ExamDto> Exams { get; set; } = new List<ExamDto>();
    }
}
