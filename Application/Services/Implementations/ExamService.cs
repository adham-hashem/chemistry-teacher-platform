using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Application.Dtos.ExamDtos;
using Application.Repositories.Interfaces;
using Application.Services.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Application.Services.Implementations
{
    public class ExamService : IExamService
    {
        private readonly IExamRepository _examRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly IExamResultRepository _examResultRepository;
        private readonly ISubscriptionService _subscriptionService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly string _uploadPath;

        public ExamService(
            IExamRepository examRepository,
            ILessonRepository lessonRepository,
            IExamResultRepository examResultRepository,
            ISubscriptionService subscriptionService,
            UserManager<ApplicationUser> userManager)
        {
            _examRepository = examRepository;
            _lessonRepository = lessonRepository;
            _examResultRepository = examResultRepository;
            _subscriptionService = subscriptionService;
            _userManager = userManager;
            _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/exams");
            Directory.CreateDirectory(_uploadPath);
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
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    Options = q.Options,
                    CorrectOptionIndex = q.CorrectOptionIndex,
                    ImageUrl = q.ImageUrl,
                    TimeInSeconds = q.TimeInSeconds,
                    DefaultOptionIndex = q.DefaultOptionIndex
                }).ToList() ?? new List<McqQuestionDto>(),
                PdfPath = exam.PdfPath,
                ExamType = exam.ExamType,
                CreatedAt = exam.CreatedAt,
                UpdatedAt = exam.UpdatedAt,
                CertificateThreshold = exam.CertificateThreshold
            };
        }

        public async Task<List<ExamDto>> GetByLessonIdAsync(Guid lessonId, string userId)
        {
            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (lesson == null)
                throw new Exception("Lesson not found.");

            var user = await _userManager.FindByIdAsync(userId);
            bool isTeacher = user != null && await _userManager.IsInRoleAsync(user, "Teacher");

            if (!isTeacher)
            {
                bool canAccess = await _subscriptionService.CanAccessLessonAsync(userId, lessonId);
                if (!canAccess)
                {
                    return new List<ExamDto>();
                }
            }

            var exams = await _examRepository.GetByLessonIdAsync(lessonId);
            return exams.Select(exam => new ExamDto
            {
                Id = exam.Id,
                LessonId = exam.LessonId,
                Title = exam.Title,
                Questions = exam.Questions?.Select(q => new McqQuestionDto
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    Options = q.Options,
                    CorrectOptionIndex = isTeacher ? q.CorrectOptionIndex : -1,
                    ImageUrl = q.ImageUrl,
                    TimeInSeconds = q.TimeInSeconds,
                    DefaultOptionIndex = q.DefaultOptionIndex
                }).ToList() ?? new List<McqQuestionDto>(),
                PdfPath = exam.PdfPath,
                ExamType = exam.ExamType,
                CreatedAt = exam.CreatedAt,
                UpdatedAt = exam.UpdatedAt,
                CertificateThreshold = exam.CertificateThreshold
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
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    Options = q.Options,
                    CorrectOptionIndex = q.CorrectOptionIndex,
                    ImageUrl = q.ImageUrl,
                    TimeInSeconds = q.TimeInSeconds,
                    DefaultOptionIndex = q.DefaultOptionIndex
                }).ToList() ?? new List<McqQuestionDto>(),
                PdfPath = exam.PdfPath,
                ExamType = exam.ExamType,
                CreatedAt = exam.CreatedAt,
                UpdatedAt = exam.UpdatedAt,
                CertificateThreshold = exam.CertificateThreshold
            }).ToList();
        }

        public async Task<ExamDto> CreateAsync(ExamDto examDto, string userId, IFormFile? examPdf = null, List<IFormFile>? questionImageFiles = null)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Teacher"))
                throw new Exception("Only teachers can create exams.");

            var lesson = await _lessonRepository.GetByIdAsync(examDto.LessonId);
            if (lesson == null)
                throw new Exception("Lesson not found.");

            // Validation based on ExamType
            if (examDto.ExamType == ExamType.MCQ)
            {
                if (examDto.Questions == null || !examDto.Questions.Any())
                    throw new Exception("MCQ exams must include at least one question.");
                if (examPdf != null)
                    throw new Exception("MCQ exams cannot include a PDF file.");
                if (questionImageFiles != null && questionImageFiles.Count > examDto.Questions.Count)
                    throw new Exception("Number of image files cannot exceed number of questions.");
            }
            else if (examDto.ExamType == ExamType.PDF)
            {
                if (examPdf == null)
                    throw new Exception("PDF exams must include a PDF file.");
                if (examDto.Questions != null && examDto.Questions.Any())
                    throw new Exception("PDF exams cannot include questions.");
                if (questionImageFiles != null && questionImageFiles.Any())
                    throw new Exception("PDF exams cannot include question images.");
            }
            else
            {
                throw new Exception("Invalid exam type.");
            }

            var exam = new Exam
            {
                Id = Guid.NewGuid(),
                LessonId = examDto.LessonId,
                Title = examDto.Title,
                ExamType = examDto.ExamType,
                CreatedAt = DateTime.UtcNow,
                CertificateThreshold = examDto.CertificateThreshold
            };

            // Handle questions and images for MCQ
            if (examDto.ExamType == ExamType.MCQ && examDto.Questions != null)
            {
                exam.Questions = new List<McqQuestion>();
                for (int i = 0; i < examDto.Questions.Count; i++)
                {
                    var questionDto = examDto.Questions[i];
                    var imageFile = questionImageFiles != null && i < questionImageFiles.Count ? questionImageFiles[i] : null;

                    // Validate question data
                    if (string.IsNullOrEmpty(questionDto.QuestionText))
                        throw new Exception($"Question {i + 1} must have a question text.");
                    if (questionDto.Options == null || questionDto.Options.Count < 2)
                        throw new Exception($"Question {i + 1} must have at least two options.");
                    if (questionDto.CorrectOptionIndex < 0 || questionDto.CorrectOptionIndex >= questionDto.Options.Count)
                        throw new Exception($"Question {i + 1} has an invalid correct option index.");
                    if (questionDto.TimeInSeconds < 1)
                        throw new Exception($"Question {i + 1} must have a valid time in seconds (minimum 1).");

                    var question = new McqQuestion
                    {
                        Id = Guid.NewGuid(),
                        ExamId = exam.Id,
                        QuestionText = questionDto.QuestionText,
                        Options = questionDto.Options,
                        CorrectOptionIndex = questionDto.CorrectOptionIndex,
                        TimeInSeconds = questionDto.TimeInSeconds,
                        DefaultOptionIndex = questionDto.DefaultOptionIndex,
                        ImageUrl = questionDto.ImageUrl // Use provided URL if any
                    };

                    // Save image file if provided and store URL
                    if (imageFile != null)
                    {
                        question.ImageUrl = await SaveFileAsync(imageFile, exam.Id, i);
                    }

                    exam.Questions.Add(question);
                }
            }

            // Handle PDF for PDF exams
            if (examDto.ExamType == ExamType.PDF && examPdf != null)
            {
                exam.PdfPath = await SaveFileAsync(examPdf, exam.Id);
            }

            await _examRepository.AddAsync(exam);

            return new ExamDto
            {
                Id = exam.Id,
                LessonId = exam.LessonId,
                Title = exam.Title,
                Questions = exam.Questions?.Select(q => new McqQuestionDto
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    Options = q.Options,
                    CorrectOptionIndex = q.CorrectOptionIndex,
                    ImageUrl = q.ImageUrl,
                    TimeInSeconds = q.TimeInSeconds,
                    DefaultOptionIndex = q.DefaultOptionIndex
                }).ToList() ?? new List<McqQuestionDto>(),
                PdfPath = exam.PdfPath,
                ExamType = exam.ExamType,
                CreatedAt = exam.CreatedAt,
                UpdatedAt = exam.UpdatedAt,
                CertificateThreshold = exam.CertificateThreshold
            };
        }

        public async Task<ExamDto> UpdateAsync(Guid id, ExamDto examDto, string userId, IFormFile? examPdf = null, List<IFormFile>? questionImageFiles = null)
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

            // Validation based on ExamType
            if (examDto.ExamType == ExamType.MCQ)
            {
                if (examDto.Questions == null || !examDto.Questions.Any())
                    throw new Exception("MCQ exams must include at least one question.");
                if (examPdf != null)
                    throw new Exception("MCQ exams cannot include a PDF file.");
                if (questionImageFiles != null && questionImageFiles.Count > examDto.Questions.Count)
                    throw new Exception("Number of image files cannot exceed number of questions.");
            }
            else if (examDto.ExamType == ExamType.PDF)
            {
                if (examPdf == null && exam.PdfPath == null)
                    throw new Exception("PDF exams must include a PDF file.");
                if (examDto.Questions != null && examDto.Questions.Any())
                    throw new Exception("PDF exams cannot include questions.");
                if (questionImageFiles != null && questionImageFiles.Any())
                    throw new Exception("PDF exams cannot include question images.");
            }
            else
            {
                throw new Exception("Invalid exam type.");
            }

            exam.LessonId = examDto.LessonId;
            exam.Title = examDto.Title;
            exam.ExamType = examDto.ExamType;
            exam.UpdatedAt = DateTime.UtcNow;
            exam.CertificateThreshold = examDto.CertificateThreshold;

            // Handle questions and images for MCQ
            if (examDto.ExamType == ExamType.MCQ && examDto.Questions != null)
            {
                // Delete existing images
                if (exam.Questions != null)
                {
                    foreach (var question in exam.Questions)
                    {
                        DeleteFile(question.ImageUrl);
                    }
                }

                exam.Questions = new List<McqQuestion>();
                for (int i = 0; i < examDto.Questions.Count; i++)
                {
                    var questionDto = examDto.Questions[i];
                    var imageFile = questionImageFiles != null && i < questionImageFiles.Count ? questionImageFiles[i] : null;

                    // Validate question data
                    if (string.IsNullOrEmpty(questionDto.QuestionText))
                        throw new Exception($"Question {i + 1} must have a question text.");
                    if (questionDto.Options == null || questionDto.Options.Count < 2)
                        throw new Exception($"Question {i + 1} must have at least two options.");
                    if (questionDto.CorrectOptionIndex < 0 || questionDto.CorrectOptionIndex >= questionDto.Options.Count)
                        throw new Exception($"Question {i + 1} has an invalid correct option index.");
                    if (questionDto.TimeInSeconds < 1)
                        throw new Exception($"Question {i + 1} must have a valid time in seconds (minimum 1).");

                    var question = new McqQuestion
                    {
                        Id = Guid.NewGuid(),
                        ExamId = exam.Id,
                        QuestionText = questionDto.QuestionText,
                        Options = questionDto.Options,
                        CorrectOptionIndex = questionDto.CorrectOptionIndex,
                        TimeInSeconds = questionDto.TimeInSeconds,
                        DefaultOptionIndex = questionDto.DefaultOptionIndex,
                        ImageUrl = questionDto.ImageUrl // Use provided URL if any
                    };

                    // Save image file if provided and store URL
                    if (imageFile != null)
                    {
                        question.ImageUrl = await SaveFileAsync(imageFile, id, i);
                    }

                    exam.Questions.Add(question);
                }
            }
            else
            {
                // Clear questions and images for non-MCQ exams
                if (exam.Questions != null)
                {
                    foreach (var question in exam.Questions)
                    {
                        DeleteFile(question.ImageUrl);
                    }
                }
                exam.Questions = null;
            }

            // Handle PDF for PDF exams
            if (examDto.ExamType == ExamType.PDF)
            {
                if (examPdf != null)
                {
                    DeleteFile(exam.PdfPath);
                    exam.PdfPath = await SaveFileAsync(examPdf, id);
                }
            }
            else
            {
                // Clear PDF for non-PDF exams
                DeleteFile(exam.PdfPath);
                exam.PdfPath = null;
            }

            await _examRepository.UpdateAsync(exam);

            return new ExamDto
            {
                Id = exam.Id,
                LessonId = exam.LessonId,
                Title = exam.Title,
                Questions = exam.Questions?.Select(q => new McqQuestionDto
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    Options = q.Options,
                    CorrectOptionIndex = q.CorrectOptionIndex,
                    ImageUrl = q.ImageUrl,
                    TimeInSeconds = q.TimeInSeconds,
                    DefaultOptionIndex = q.DefaultOptionIndex
                }).ToList() ?? new List<McqQuestionDto>(),
                PdfPath = exam.PdfPath,
                ExamType = exam.ExamType,
                CreatedAt = exam.CreatedAt,
                UpdatedAt = exam.UpdatedAt,
                CertificateThreshold = exam.CertificateThreshold
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

            // Delete all associated images for MCQ questions
            if (exam.Questions != null)
            {
                foreach (var question in exam.Questions)
                {
                    DeleteFile(question.ImageUrl);
                }
            }

            // Delete PDF file if exists
            DeleteFile(exam.PdfPath);

            var examResults = await _examResultRepository.GetByExamIdAsync(id);
            foreach (var result in examResults)
            {
                await _examResultRepository.DeleteAsync(result.Id);
            }

            await _examRepository.DeleteAsync(id);
        }

        private async Task<string?> SaveFileAsync(IFormFile? file, Guid examId, int? questionIndex = null)
        {
            if (file == null || file.Length == 0)
                return null;

            // Validate file type
            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
                throw new Exception("Invalid file type. Allowed types: pdf, jpg, jpeg, png, gif.");

            // Validate file size (e.g., max 5MB)
            if (file.Length > 5 * 1024 * 1024)
                throw new Exception("File size exceeds 5MB.");

            var fileName = questionIndex.HasValue
                ? $"{examId}_question_{questionIndex}_{Guid.NewGuid()}{fileExtension}"
                : $"{examId}_{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(_uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/Uploads/exams/{fileName}";
        }

        private void DeleteFile(string? filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
        }
    }
}