using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Bio { get; set; }
        public string? Qualifications { get; set; }
        public EducationalLevel? Grade { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime? LastActiveDate { get; set; }
        public bool IsOnline { get; set; }
        public double VideoCompletionRate { get; set; }
        public List<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public List<Review> Reviews { get; set; } = new List<Review>();
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public List<Question> Questions { get; set; } = new List<Question>();
        public List<Payment> Payments { get; set; } = new List<Payment>();
        public List<ExamResult> ExamResults { get; set; } = new List<ExamResult>();
        public List<Notification> Notifications { get; set; } = new List<Notification>();
        public List<LessonAccessCode> LessonAccessCodes { get; set; } = new List<LessonAccessCode>();
        public List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public bool IsEmailVerified { get; set; } = false;
        public string? EmailVerificationToken { get; set; }
    }
}
