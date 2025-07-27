using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Dtos.ExamDtos;
using Application.Repositories.Interfaces;
using Application.Services.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Services.Implementations
{
    public class ExamResultService : IExamResultService
    {
        private readonly IExamResultRepository _examResultRepository;
        private readonly IExamRepository _examRepository;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly ILessonAccessCodeRepository _lessonAccessCodeRepository;
        private readonly ISubscriptionService _subscriptionService;
        private readonly ILessonRepository _lessonRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public ExamResultService(
            IExamResultRepository examResultRepository,
            IExamRepository examRepository,
            ISubscriptionRepository subscriptionRepository,
            ILessonAccessCodeRepository lessonAccessCodeRepository,
            ISubscriptionService subscriptionService,
            ILessonRepository lessonRepository,
            UserManager<ApplicationUser> userManager)
        {
            _examResultRepository = examResultRepository;
            _examRepository = examRepository;
            _subscriptionRepository = subscriptionRepository;
            _lessonAccessCodeRepository = lessonAccessCodeRepository;
            _subscriptionService = subscriptionService;
            _lessonRepository = lessonRepository;
            _userManager = userManager;
        }

        public async Task<ExamResultDto> GetByIdAsync(Guid id, string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User ID is required.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new UnauthorizedAccessException("User not found.");

            var examResult = await _examResultRepository.GetByIdAsync(id);
            if (examResult == null)
                throw new KeyNotFoundException("Exam result not found.");

            bool isTeacher = await _userManager.IsInRoleAsync(user, "Teacher");
            if (!isTeacher && examResult.UserId != userId)
                throw new UnauthorizedAccessException("Students can only view their own exam results.");

            return new ExamResultDto
            {
                Id = examResult.Id,
                ExamId = examResult.ExamId,
                UserId = examResult.UserId,
                Answers = examResult.Answers,
                Score = examResult.Score,
                SubmittedAt = examResult.SubmittedAt
            };
        }

        public async Task<List<ExamResultDto>> GetByExamIdAsync(Guid examId, string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User ID is required.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new UnauthorizedAccessException("User not found.");

            bool isTeacher = await _userManager.IsInRoleAsync(user, "Teacher");
            bool isStudent = await _userManager.IsInRoleAsync(user, "Student");

            if (!isTeacher && !isStudent)
                throw new UnauthorizedAccessException("User must be a student or teacher.");

            var examResults = await _examResultRepository.GetByExamIdAsync(examId);
            if (!examResults.Any())
                throw new KeyNotFoundException("No results found for the specified exam.");

            var resultsDtos = new List<ExamResultDto>();
            foreach (var er in examResults)
            {
                if (!isTeacher && er.UserId != userId)
                    continue;

                string firstName = null;
                string lastName = null;
                string email = null;

                if (isTeacher)
                {
                    var examUser = await _userManager.FindByIdAsync(er.UserId);
                    if (examUser != null)
                    {
                        firstName = examUser.FirstName;
                        lastName = examUser.LastName;
                        email = examUser.Email;
                    }
                }

                resultsDtos.Add(new ExamResultDto
                {
                    Id = er.Id,
                    ExamId = er.ExamId,
                    UserId = er.UserId,
                    FullName = firstName + " " + lastName,
                    Email = email,
                    Answers = er.Answers,
                    Score = er.Score,
                    SubmittedAt = er.SubmittedAt
                });
            }

            if (!resultsDtos.Any())
                throw new KeyNotFoundException("No accessible exam results found for the specific lesson.");

            return resultsDtos;

            //return examResults.Select(er => new ExamResultDto
            //{
            //    Id = er.Id,
            //    ExamId = er.ExamId,
            //    UserId = er.UserId,
            //    Answers = er.Answers,
            //    Score = er.Score,
            //    SubmittedAt = er.SubmittedAt
            //}).ToList();
        }

        public async Task<ExamResultDto> GetByUserIdAndExamIdAsync(string userId, Guid examId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID is required.");
            if (examId == Guid.Empty)
                throw new ArgumentException("Exam ID is required.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Student"))
                throw new UnauthorizedAccessException("Only students can view their own exam results.");

            var examResult = await _examResultRepository.GetByExamIdAndUserIdAsync(examId, userId);
            if (examResult == null)
                return null;

            return new ExamResultDto
            {
                Id = examResult.Id,
                ExamId = examResult.ExamId,
                UserId = examResult.UserId,
                Answers = examResult.Answers,
                Score = examResult.Score,
                SubmittedAt = examResult.SubmittedAt
            };
        }

        public async Task<ExamResultDto> SubmitAsync(SubmitExamDto submitExamDto, string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID is required.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Student"))
                throw new UnauthorizedAccessException("Only students can submit exam results.");

            var exam = await _examRepository.GetByIdAsync(submitExamDto.ExamId);
            if (exam == null)
                throw new KeyNotFoundException("Exam not found.");

            var lesson = await _lessonRepository.GetByIdAsync(exam.LessonId);
            if (lesson == null)
                throw new KeyNotFoundException("Lesson not found.");

            // Check if the student has already submitted this exam
            var existingResult = await _examResultRepository.GetByExamIdAndUserIdAsync(submitExamDto.ExamId, userId);
            if (existingResult != null)
                throw new InvalidOperationException("Exam already submitted by this user.");

            // Allow access if the lesson is free or the student has a subscription/access code
            bool hasAccess = lesson.IsFree || await _subscriptionService.CanAccessLessonAsync(userId, exam.LessonId);
            if (!hasAccess)
                throw new UnauthorizedAccessException("User does not have access to the lesson associated with this exam.");

            if (submitExamDto.Answers == null || !submitExamDto.Answers.Any())
                throw new ArgumentException("At least one answer must be provided.");

            var examResult = new ExamResult
            {
                Id = Guid.NewGuid(),
                ExamId = submitExamDto.ExamId,
                UserId = userId,
                Answers = submitExamDto.Answers,
                SubmittedAt = DateTime.UtcNow,
                Score = CalculateScore(exam.Questions, submitExamDto.Answers)
            };

            await _examResultRepository.AddAsync(examResult);

            return new ExamResultDto
            {
                Id = examResult.Id,
                ExamId = examResult.ExamId,
                UserId = examResult.UserId,
                Answers = examResult.Answers,
                Score = examResult.Score,
                SubmittedAt = examResult.SubmittedAt
            };
        }

        public async Task DeleteAsync(Guid id, string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User ID is required.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Teacher"))
                throw new UnauthorizedAccessException("Only teachers can delete exam results.");

            var examResult = await _examResultRepository.GetByIdAsync(id);
            if (examResult == null)
                throw new KeyNotFoundException("Exam result not found.");

            await _examResultRepository.DeleteAsync(id);
        }

        private int CalculateScore(List<McqQuestion> questions, List<int> answers)
        {
            if (questions == null || !questions.Any())
                return 0;

            int score = 0;
            for (int i = 0; i < Math.Min(questions.Count, answers.Count); i++)
            {
                if (answers[i] == questions[i].CorrectOptionIndex)
                    score++;
            }
            return score;
        }
    }
}