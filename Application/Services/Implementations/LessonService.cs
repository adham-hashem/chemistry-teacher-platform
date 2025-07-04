using System;
using System.IO;
using System.Threading.Tasks;
using Application.Dtos.LessonDtos;
using Application.Repositories.Interfaces;
using Application.Services.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Application.Services.Implementations
{
    public class LessonService : ILessonService
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISubscriptionService _subscriptionService;
        private readonly string _uploadPath;

        public LessonService(
            ILessonRepository lessonRepository,
            ICourseRepository courseRepository,
            UserManager<ApplicationUser> userManager,
            ISubscriptionService subscriptionService)
        {
            _lessonRepository = lessonRepository;
            _courseRepository = courseRepository;
            _userManager = userManager;
            _subscriptionService = subscriptionService;
            _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/lessons");
            Directory.CreateDirectory(_uploadPath);
        }

        public async Task<LessonDto> CreateAsync(LessonDto lessonDto, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Teacher"))
                throw new Exception("Only teachers can create lessons.");

            var course = await _courseRepository.GetByIdAsync(lessonDto.CourseId);
            if (course == null)
                throw new Exception("Course not found.");

            var lesson = new Lesson
            {
                Id = Guid.NewGuid(),
                Title = lessonDto.Title,
                VideoUrl = lessonDto.VideoUrl,
                LessonSummaryText = lessonDto.LessonSummaryText,
                IsFree = lessonDto.IsFree,
                MonthAssigned = lessonDto.MonthAssigned,
                AdditionalResources = lessonDto.AdditionalResources,
                CourseId = course.Id
            };

            // Handle file uploads and set file paths
            lesson.LessonSummaryPdfPath = await SaveFileAsync(lessonDto.LessonSummaryPdf, lesson.Id);
            lesson.EquationsTablePdfPath = await SaveFileAsync(lessonDto.EquationsTablePdf, lesson.Id);
            lesson.WorkPapersPdfPath = await SaveFileAsync(lessonDto.WorkPapersPdf, lesson.Id);

            await _lessonRepository.AddAsync(lesson);

            return new LessonDto
            {
                Id = lesson.Id,
                Title = lesson.Title,
                VideoUrl = lesson.VideoUrl,
                LessonSummaryText = lesson.LessonSummaryText,
                LessonSummaryPdfPath = lesson.LessonSummaryPdfPath,
                EquationsTablePdfPath = lesson.EquationsTablePdfPath,
                WorkPapersPdfPath = lesson.WorkPapersPdfPath,
                IsFree = lesson.IsFree,
                MonthAssigned = lesson.MonthAssigned,
                AdditionalResources = lesson.AdditionalResources,
                CourseId = lesson.CourseId
            };
        }

        public async Task UpdateAsync(LessonDto lessonDto, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Teacher"))
                throw new Exception("Only teachers can update lessons.");

            if (lessonDto.Id == Guid.Empty)
                throw new Exception("Lesson ID is required.");

            var lesson = await _lessonRepository.GetByIdAsync(lessonDto.Id);
            if (lesson == null)
                throw new Exception("Lesson not found.");

            var course = await _courseRepository.GetByIdAsync(lessonDto.CourseId);
            if (course == null)
                throw new Exception("Course not found.");

            // Update lesson properties
            lesson.Title = lessonDto.Title;
            lesson.VideoUrl = lessonDto.VideoUrl;
            lesson.LessonSummaryText = lessonDto.LessonSummaryText;
            lesson.IsFree = lessonDto.IsFree;
            lesson.MonthAssigned = lessonDto.MonthAssigned;
            lesson.AdditionalResources = lessonDto.AdditionalResources;
            lesson.CourseId = course.Id;

            // Handle file uploads if new files are provided
            if (lessonDto.LessonSummaryPdf != null)
                lesson.LessonSummaryPdfPath = await SaveFileAsync(lessonDto.LessonSummaryPdf, lesson.Id);
            if (lessonDto.EquationsTablePdf != null)
                lesson.EquationsTablePdfPath = await SaveFileAsync(lessonDto.EquationsTablePdf, lesson.Id);
            if (lessonDto.WorkPapersPdfPath != null)
                lesson.WorkPapersPdfPath = await SaveFileAsync(lessonDto.WorkPapersPdf, lesson.Id);

            await _lessonRepository.UpdateAsync(lesson);
        }

        public async Task DeleteAsync(Guid id, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Teacher"))
                throw new Exception("Only teachers can delete lessons.");

            var lesson = await _lessonRepository.GetByIdAsync(id);
            if (lesson == null)
                throw new Exception("Lesson not found.");

            // Optionally delete associated files
            DeleteFile(lesson.LessonSummaryPdfPath);
            DeleteFile(lesson.EquationsTablePdfPath);
            DeleteFile(lesson.WorkPapersPdfPath);

            await _lessonRepository.DeleteAsync(id);
        }

        private async Task<string?> SaveFileAsync(IFormFile? file, Guid lessonId)
        {
            if (file == null || file.Length == 0)
                return null;

            var fileExtension = Path.GetExtension(file.FileName);
            var fileName = $"{lessonId}_{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(_uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/lessons/{fileName}";
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

        public async Task<List<LessonDto>> GetLessonsByCourseAsync(Guid courseId, string userId)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null)
                throw new Exception("Course not found.");

            var lessons = await _lessonRepository.GetByCourseIdAsync(courseId);
            var lessonDtos = new List<LessonDto>();

            // Check if the user is a teacher
            var user = await _userManager.FindByIdAsync(userId);
            bool isTeacher = user != null && await _userManager.IsInRoleAsync(user, "Teacher");

            foreach (var lesson in lessons)
            {
                if (isTeacher)
                {
                    // Teachers get full access to all lesson details
                    lessonDtos.Add(new LessonDto
                    {
                        Id = lesson.Id,
                        Title = lesson.Title,
                        VideoUrl = lesson.VideoUrl,
                        LessonSummaryText = lesson.LessonSummaryText,
                        LessonSummaryPdfPath = lesson.LessonSummaryPdfPath,
                        EquationsTablePdfPath = lesson.EquationsTablePdfPath,
                        WorkPapersPdfPath = lesson.WorkPapersPdfPath,
                        IsFree = lesson.IsFree,
                        MonthAssigned = lesson.MonthAssigned,
                        AdditionalResources = lesson.AdditionalResources,
                        CourseId = lesson.CourseId,
                        IsAccessible = true
                    });
                }
                else
                {
                    // Students follow access check logic
                    var canAccess = await _subscriptionService.CanAccessLessonAsync(userId, lesson.Id);
                    lessonDtos.Add(new LessonDto
                    {
                        Id = lesson.Id,
                        Title = lesson.Title,
                        VideoUrl = canAccess ? lesson.VideoUrl : null,
                        LessonSummaryText = canAccess ? lesson.LessonSummaryText : null,
                        LessonSummaryPdfPath = canAccess ? lesson.LessonSummaryPdfPath : null,
                        EquationsTablePdfPath = canAccess ? lesson.EquationsTablePdfPath : null,
                        WorkPapersPdfPath = canAccess ? lesson.WorkPapersPdfPath : null,
                        IsFree = lesson.IsFree,
                        MonthAssigned = lesson.MonthAssigned,
                        AdditionalResources = canAccess ? lesson.AdditionalResources : null,
                        CourseId = lesson.CourseId,
                        IsAccessible = canAccess
                    });
                }
            }

            return lessonDtos;
        }
    }
}