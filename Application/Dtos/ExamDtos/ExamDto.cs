using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.ExamDtos
{
    public class ExamDto
    {
        public Guid Id { get; set; }
        public Guid LessonId { get; set; }
        public string Title { get; set; }
        public List<McqQuestionDto> Questions { get; set; } = new List<McqQuestionDto>();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
