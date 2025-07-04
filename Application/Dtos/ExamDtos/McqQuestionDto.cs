using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.ExamDtos
{
    public class McqQuestionDto
    {
        public string QuestionText { get; set; }
        public List<string> Options { get; set; } = new List<string>();
        public int CorrectOptionIndex { get; set; }
    }
}
