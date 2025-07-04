using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Domain.Entities;
using Domain.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<LessonAccessCode> LessonAccessCodes { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<ExamResult> ExamResults { get; set; }
        public DbSet<McqQuestion> McqQuestions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Course configuration
            builder.Entity<Course>()
                .Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Entity<Course>()
                .Property(c => c.Category)
                .HasMaxLength(50);

            builder.Entity<Course>()
                .Property(c => c.ImageUrl)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Entity<Course>()
                .Property(c => c.IntroductoryVideoUrl)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Entity<Course>()
                .Property(c => c.ShortDescription)
                .HasMaxLength(500);

            builder.Entity<Course>()
                .Property(c => c.DetailedDescription)
                .HasMaxLength(2000);

            builder.Entity<Course>()
                .Property(c => c.Requirements)
                .HasMaxLength(1000);

            builder.Entity<Course>()
                .Property(c => c.WhatStudentsWillLearn)
                .HasMaxLength(1000);

            // Lesson configuration
            builder.Entity<Lesson>()
                .HasOne(l => l.Course)
                .WithMany(c => c.Lessons)
                .HasForeignKey(l => l.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Lesson>()
                .Property(l => l.Title)
                .IsRequired();

            builder.Entity<Lesson>()
                .Property(l => l.VideoUrl)
                .IsRequired();

            builder.Entity<Lesson>()
                .Property(l => l.LessonSummaryText)
                .IsRequired(false);

            builder.Entity<Lesson>()
                .Property(l => l.LessonSummaryPdfPath)
                .IsRequired(false);

            builder.Entity<Lesson>()
                .Property(l => l.EquationsTablePdfPath)
                .IsRequired(false);

            builder.Entity<Lesson>()
                .Property(l => l.AdditionalResources)
                .IsRequired(false);

            builder.Entity<Lesson>()
                .Property(l => l.MonthAssigned)
                .IsRequired(false);

            builder.Entity<Lesson>()
                .Property(l => l.Price)
                .HasColumnType("decimal(18,2)")
                .IsRequired(false);

            // Review configuration
            builder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Review>()
                .HasOne(r => r.Course)
                .WithMany(c => c.Reviews)
                .HasForeignKey(r => r.CourseId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Review>()
                .Property(r => r.Comment)
                .HasMaxLength(1000)
                .IsRequired(false);

            builder.Entity<Review>()
                .HasIndex(r => new { r.UserId, r.CourseId })
                .IsUnique();

            // Comment configuration
            builder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Comment>()
                .HasOne(c => c.Lesson)
                .WithMany(l => l.Comments)
                .HasForeignKey(c => c.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Comment>()
                .HasIndex(c => new { c.UserId, c.LessonId });

            // Question configuration
            builder.Entity<Question>()
                .HasOne(q => q.User)
                .WithMany(u => u.Questions)
                .HasForeignKey(q => q.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Question>()
                .HasOne(q => q.Lesson)
                .WithMany(l => l.Questions)
                .HasForeignKey(q => q.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Question>()
                .Property(q => q.Answer)
                .IsRequired(false);

            builder.Entity<Question>()
                .HasIndex(q => new { q.UserId, q.LessonId });

            // Subscription configuration
            builder.Entity<Subscription>()
                .HasOne(s => s.User)
                .WithMany(u => u.Subscriptions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Subscription>()
                .Property(s => s.SubscribedMonths)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }),
                    v => JsonSerializer.Deserialize<List<int>>(v, new JsonSerializerOptions()) ?? new List<int>(),
                    new ValueComparer<List<int>>(
                        (a, b) => (a == null && b == null) || (a != null && b != null && a.SequenceEqual(b)),
                        list => list != null ? list.Aggregate(0, (hash, i) => HashCode.Combine(hash, i)) : 0,
                        list => list != null ? list.ToList() : new List<int>()
                    ));

            builder.Entity<Subscription>()
                .Property(s => s.AccessedLessons)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }),
                    v => JsonSerializer.Deserialize<List<Guid>>(v, new JsonSerializerOptions()) ?? new List<Guid>(),
                    new ValueComparer<List<Guid>>(
                        (a, b) => (a == null && b == null) || (a != null && b != null && a.SequenceEqual(b)),
                        list => list != null ? list.Aggregate(0, (hash, i) => HashCode.Combine(hash, i)) : 0,
                        list => list != null ? list.ToList() : new List<Guid>()
                    ));

            builder.Entity<Subscription>()
                .Property(s => s.Price)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Subscription>()
                .Property(s => s.Type)
                .IsRequired();

            builder.Entity<Subscription>()
                .Property(s => s.LectureCount)
                .IsRequired(false);

            builder.Entity<Subscription>()
                .HasIndex(s => new { s.UserId, s.Grade });

            // Payment configuration
            builder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Payment>()
                .HasOne(p => p.Subscription)
                .WithMany(s => s.Payments)
                .HasForeignKey(p => p.SubscriptionId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Payment>()
                .Property(p => p.Currency)
                .IsRequired();

            builder.Entity<Payment>()
                .Property(p => p.PaymentMethod)
                .IsRequired();

            builder.Entity<Payment>()
                .Property(p => p.TransactionId)
                .IsRequired();

            builder.Entity<Payment>()
                .Property(p => p.Status)
                .IsRequired();

            builder.Entity<Payment>()
                .HasIndex(p => p.TransactionId)
                .IsUnique();

            // Notification configuration
            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Notification>()
                .HasIndex(n => n.UserId);

            // LessonAccessCode configuration
            builder.Entity<LessonAccessCode>()
                .HasOne(lac => lac.Lesson)
                .WithMany(l => l.LessonAccessCodes)
                .HasForeignKey(lac => lac.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<LessonAccessCode>()
                .HasOne(lac => lac.User)
                .WithMany(u => u.LessonAccessCodes)
                .HasForeignKey(lac => lac.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<LessonAccessCode>()
                .HasIndex(lac => lac.Code)
                .IsUnique();

            builder.Entity<LessonAccessCode>()
                .HasIndex(lac => new { lac.LessonId, lac.UserId });

            // RefreshToken configuration
            builder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RefreshToken>()
                .HasIndex(rt => rt.Token)
                .IsUnique();

            // Exam configuration
            builder.Entity<Exam>()
                .HasKey(e => e.Id);

            builder.Entity<Exam>()
                .Property(e => e.Title)
                .IsRequired();

            builder.Entity<Exam>()
                .Property(e => e.CreatedAt)
                .IsRequired();

            builder.Entity<Exam>()
                .Property(e => e.UpdatedAt)
                .IsRequired(false);

            builder.Entity<Exam>()
                .HasOne(e => e.Lesson)
                .WithMany(l => l.Exams)
                .HasForeignKey(e => e.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            // ExamResult configuration
            builder.Entity<ExamResult>()
                .HasKey(er => er.Id);

            builder.Entity<ExamResult>()
                .Property(er => er.Answers)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }),
                    v => JsonSerializer.Deserialize<List<int>>(v, new JsonSerializerOptions()) ?? new List<int>(),
                    new ValueComparer<List<int>>(
                        (a, b) => (a == null && b == null) || (a != null && b != null && a.SequenceEqual(b)),
                        list => list != null ? list.Aggregate(0, (hash, i) => HashCode.Combine(hash, i)) : 0,
                        list => list != null ? list.ToList() : new List<int>()
                    ));

            builder.Entity<ExamResult>()
                .HasOne(er => er.User)
                .WithMany(u => u.ExamResults)
                .HasForeignKey(er => er.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ExamResult>()
                .HasOne(er => er.Exam)
                .WithMany(e => e.ExamResults)
                .HasForeignKey(er => er.ExamId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ExamResult>()
                .HasIndex(er => new { er.UserId, er.ExamId });

            // McqQuestion configuration
            builder.Entity<McqQuestion>()
                .HasKey(q => q.Id);

            builder.Entity<McqQuestion>()
                .Property(q => q.QuestionText)
                .IsRequired();

            builder.Entity<McqQuestion>()
                .Property(q => q.Options)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }),
                    v => JsonSerializer.Deserialize<List<string>>(v, new JsonSerializerOptions()) ?? new List<string>(),
                    new ValueComparer<List<string>>(
                        (a, b) => (a == null && b == null) || (a != null && b != null && a.SequenceEqual(b)),
                        list => list != null ? list.Aggregate(0, (hash, i) => HashCode.Combine(hash, i)) : 0,
                        list => list != null ? list.ToList() : new List<string>()
                    ));

            builder.Entity<McqQuestion>()
                .Property(q => q.CorrectOptionIndex)
                .IsRequired();

            builder.Entity<McqQuestion>()
                .HasOne(q => q.Exam)
                .WithMany(e => e.Questions)
                .HasForeignKey(q => q.ExamId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}