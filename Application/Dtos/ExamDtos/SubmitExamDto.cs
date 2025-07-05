using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.ExamDtos
{
    public class SubmitExamDto
    {
        [Required]
        public Guid ExamId { get; set; }
        [Required, MinLength(1)]
        public List<int> Answers { get; set; } = new List<int>();
    }
}
