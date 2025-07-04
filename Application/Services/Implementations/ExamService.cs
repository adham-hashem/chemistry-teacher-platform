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
    public class ExamService : IExamService
    {
        private readonly IExamRepository _examRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly IExamResultRepository _examResultRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public ExamService(
            IExamRepository examRepository,
            ILessonRepository lessonRepository,
            IExamResultRepository examResultRepository,
            UserManager<ApplicationUser> userManager)
        {
            _examRepository = examRepository;
            _lessonRepository = lessonRepository;
            _examResultRepository = examResultRepository;
            _userManager = userManager;
        }

        public async Task<ExamDto> GetByIdAsync(Guid id)
        {
            var exam = await _examRepository.GetByIdAsync(id);
            if (exam == null)
                throw new Exception("Exam not found.");

            return new ExamDto
            {
                Id = exam.Id,
                LessonId = exam.LessonId,
                Title = exam.Title,
                Questions = exam.Questions?.Select(q => new McqQuestionDto
                {
                    QuestionText = q.QuestionText,
                    Options = q.Options,
                    CorrectOptionIndex = q.CorrectOptionIndex
                }).ToList() ?? new List<McqQuestionDto>(),
                CreatedAt = exam.CreatedAt,
                UpdatedAt = exam.UpdatedAt
            };
        }

        public async Task<List<ExamDto>> GetByLessonIdAsync(Guid lessonId)
        {
            var exams = await _examRepository.GetByLessonIdAsync(lessonId);
            return exams.Select(exam => new ExamDto
            {
                Id = exam.Id,
                LessonId = exam.LessonId,
                Title = exam.Title,
                Questions = exam.Questions?.Select(q => new McqQuestionDto
                {
                    QuestionText = q.QuestionText,
                    Options = q.Options,
                    CorrectOptionIndex = q.CorrectOptionIndex
                }).ToList() ?? new List<McqQuestionDto>(),
                CreatedAt = exam.CreatedAt,
                UpdatedAt = exam.UpdatedAt
            }).ToList();
        }

        public async Task<List<ExamDto>> GetAllAsync()
        {
            var exams = await _examRepository.GetAllAsync();
            return exams.Select(exam => new ExamDto
            {
                Id = exam.Id,
                LessonId = exam.LessonId,
                Title = exam.Title,
                Questions = exam.Questions?.Select(q => new McqQuestionDto
                {
                    QuestionText = q.QuestionText,
                    Options = q.Options,
                    CorrectOptionIndex = q.CorrectOptionIndex
                }).ToList() ?? new List<McqQuestionDto>(),
                CreatedAt = exam.CreatedAt,
                UpdatedAt = exam.UpdatedAt
            }).ToList();
        }

        public async Task<ExamDto> CreateAsync(ExamDto examDto, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Teacher"))
                throw new Exception("Only teachers can create exams.");

            var lesson = await _lessonRepository.GetByIdAsync(examDto.LessonId);
            if (lesson == null)
                throw new Exception("Lesson not found.");

            var exam = new Exam
            {
                Id = Guid.NewGuid(),
                LessonId = examDto.LessonId,
                Title = examDto.Title,
                Questions = examDto.Questions?.Select(q => new McqQuestion
                {
                    QuestionText = q.QuestionText,
                    Options = q.Options,
                    CorrectOptionIndex = q.CorrectOptionIndex
                }).ToList() ?? new List<McqQuestion>(),
                CreatedAt = DateTime.UtcNow
            };

            await _examRepository.AddAsync(exam);

            return new ExamDto
            {
                Id = exam.Id,
                LessonId = exam.LessonId,
                Title = exam.Title,
                Questions = exam.Questions?.Select(q => new McqQuestionDto
                {
                    QuestionText = q.QuestionText,
                    Options = q.Options,
                    CorrectOptionIndex = q.CorrectOptionIndex
                }).ToList() ?? new List<McqQuestionDto>(),
                CreatedAt = exam.CreatedAt,
                UpdatedAt = exam.UpdatedAt
            };
        }

        public async Task<ExamDto> UpdateAsync(Guid id, ExamDto examDto, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Teacher"))
                throw new Exception("Only teachers can update exams.");

            var exam = await _examRepository.GetByIdAsync(id);
            if (exam == null)
                throw new Exception("Exam not found.");

            var lesson = await _lessonRepository.GetByIdAsync(examDto.LessonId);
            if (lesson == null)
                throw new Exception("Lesson not found.");

            exam.LessonId = examDto.LessonId;
            exam.Title = examDto.Title;
            exam.Questions = examDto.Questions?.Select(q => new McqQuestion
            {
                QuestionText = q.QuestionText,
                Options = q.Options,
                CorrectOptionIndex = q.CorrectOptionIndex
            }).ToList() ?? new List<McqQuestion>();
            exam.UpdatedAt = DateTime.UtcNow;

            await _examRepository.UpdateAsync(exam);

            return new ExamDto
            {
                Id = exam.Id,
                LessonId = exam.LessonId,
                Title = exam.Title,
                Questions = exam.Questions?.Select(q => new McqQuestionDto
                {
                    QuestionText = q.QuestionText,
                    Options = q.Options,
                    CorrectOptionIndex = q.CorrectOptionIndex
                }).ToList() ?? new List<McqQuestionDto>(),
                CreatedAt = exam.CreatedAt,
                UpdatedAt = exam.UpdatedAt
            };
        }

        public async Task DeleteAsync(Guid id, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Teacher"))
                throw new Exception("Only teachers can delete exams.");

            var exam = await _examRepository.GetByIdAsync(id);
            if (exam == null)
                throw new Exception("Exam not found.");

            var examResults = await _examResultRepository.GetByExamIdAsync(id);
            foreach (var result in examResults)
            {
                await _examResultRepository.DeleteAsync(result.Id);
            }

            await _examRepository.DeleteAsync(id);
        }
    }
}