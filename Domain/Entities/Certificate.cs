using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Certificate
    {
        public Guid Id { get; set; }

        [Required]
        public string StudentId { get; set; }
        public ApplicationUser Student { get; set; }

        [Required]
        public Guid ExamId { get; set; }
        public Exam Exam { get; set; }

        [Required]
        [StringLength(100)]
        public string CertificateTitle { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        public DateTime IssuedAt { get; set; }
        public string PdfPath { get; set; }
    }
}
