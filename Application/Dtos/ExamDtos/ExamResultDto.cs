using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.ExamDtos
{
    public class ExamResultDto
    {
        public Guid Id { get; set; }
        public Guid ExamId { get; set; }
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public List<int> Answers { get; set; } = new List<int>();
        public int Score { get; set; }
        public DateTime SubmittedAt { get; set; }
    }
}
