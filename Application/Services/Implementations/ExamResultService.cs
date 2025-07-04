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
        private readonly UserManager<ApplicationUser> _userManager;

        public ExamResultService(
            IExamResultRepository examResultRepository,
            IExamRepository examRepository,
            ISubscriptionRepository subscriptionRepository,
            ILessonAccessCodeRepository lessonAccessCodeRepository,
            UserManager<ApplicationUser> userManager)
        {
            _examResultRepository = examResultRepository;
            _examRepository = examRepository;
            _subscriptionRepository = subscriptionRepository;
            _lessonAccessCodeRepository = lessonAccessCodeRepository;
            _userManager = userManager;
        }

        public async Task<ExamResultDto> GetByIdAsync(Guid id)
        {
            var examResult = await _examResultRepository.GetByIdAsync(id);
            if (examResult == null)
                throw new Exception("Exam result not found.");

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

        public async Task<List<ExamResultDto>> GetByExamIdAsync(Guid examId)
        {
            var examResults = await _examResultRepository.GetByExamIdAsync(examId);
            return examResults.Select(er => new ExamResultDto
            {
                Id = er.Id,
                ExamId = er.ExamId,
                UserId = er.UserId,
                Answers = er.Answers,
                Score = er.Score,
                SubmittedAt = er.SubmittedAt
            }).ToList();
        }

        public async Task<List<ExamResultDto>> GetByUserIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Student"))
                throw new Exception("Only students can view their exam results.");

            var examResults = await _examResultRepository.GetByUserIdAsync(userId);
            return examResults.Select(er => new ExamResultDto
            {
                Id = er.Id,
                ExamId = er.ExamId,
                UserId = er.UserId,
                Answers = er.Answers,
                Score = er.Score,
                SubmittedAt = er.SubmittedAt
            }).ToList();
        }

        public async Task<ExamResultDto> SubmitAsync(ExamResultDto examResultDto, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Student"))
                throw new Exception("Only students can submit exam results.");

            var exam = await _examRepository.GetByIdAsync(examResultDto.ExamId);
            if (exam == null)
                throw new Exception("Exam not found.");

            var hasAccess = await HasLessonAccessAsync(userId, exam.LessonId);
            if (!hasAccess)
                throw new Exception("User does not have access to the lesson associated with this exam.");

            var examResult = new ExamResult
            {
                Id = Guid.NewGuid(),
                ExamId = examResultDto.ExamId,
                UserId = userId,
                Answers = examResultDto.Answers,
                SubmittedAt = DateTime.UtcNow,
                Score = CalculateScore(exam.Questions, examResultDto.Answers)
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
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Teacher"))
                throw new Exception("Only teachers can delete exam results.");

            var examResult = await _examResultRepository.GetByIdAsync(id);
            if (examResult == null)
                throw new Exception("Exam result not found.");

            await _examResultRepository.DeleteAsync(id);
        }

        public async Task<bool> HasLessonAccessAsync(string userId, Guid lessonId)
        {
            var subscriptions = await _subscriptionRepository.GetByUserIdAsync(userId);
            foreach (var subscription in subscriptions)
            {
                if (subscription.IsActive && subscription.AccessedLessons.Contains(lessonId))
                    return true;
            }

            var accessCodes = await _lessonAccessCodeRepository.GetByUserIdAsync(userId);
            return accessCodes.Any(ac => ac.LessonId == lessonId);
        }

        private int CalculateScore(List<McqQuestion> questions, List<int> answers)
        {
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