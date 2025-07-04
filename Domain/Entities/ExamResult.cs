using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ExamResult
    {
        public Guid Id { get; set; }
        public Guid ExamId { get; set; }
        public Exam Exam { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public List<int> Answers { get; set; } = new List<int>();
        public int Score { get; set; }
        public DateTime SubmittedAt { get; set; }
    }
}
