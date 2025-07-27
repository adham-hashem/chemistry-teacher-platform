using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Application.Dtos.CourseDtos;
using Application.Dtos.LessonDtos;
using Application.Repositories.Interfaces;
using Application.Services.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;

namespace Application.Services.Implementations
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly string _uploadPath;

        public CourseService(ICourseRepository courseRepository, UserManager<ApplicationUser> userManager, IWebHostEnvironment environment)
        {
            _courseRepository = courseRepository;
            _userManager = userManager;
            _uploadPath = Path.Combine(environment.WebRootPath, "uploads", "courses");
            Directory.CreateDirectory(_uploadPath);
        }

        public async Task<CourseDto> GetByIdAsync(Guid id)
        {
            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null)
                throw new Exception("Course not found.");

            return new CourseDto
            {
                Id = course.Id,
                Name = course.Name,
                Category = course.Category,
                EducationalLevel = course.EducationalLevel.ToString(),
                ImageUrl = course.ImageUrl,
                IntroductoryVideoUrl = course.IntroductoryVideoUrl,
                ShortDescription = course.ShortDescription,
                DetailedDescription = course.DetailedDescription,
                Requirements = course.Requirements,
                WhatStudentsWillLearn = course.WhatStudentsWillLearn,
                //Lessons = course.Lessons?.Select(l => new LessonDto
                //{
                //    Id = l.Id,
                //    Title = l.Title,
                //    VideoUrl = l.VideoUrl,
                //    LessonSummaryText = l.LessonSummaryText,
                //    LessonSummaryPdfPath = l.LessonSummaryPdfPath,
                //    EquationsTablePdfPath = l.EquationsTablePdfPath,
                //    IsFree = l.IsFree,
                //    MonthAssigned = l.MonthAssigned,
                //    AdditionalResources = l.AdditionalResources,
                //    CourseId = l.CourseId
                //}).ToList() ?? new List<LessonDto>()
            };
        }

        public async Task<List<CourseDto>> GetAllAsync()
        {
            var courses = await _courseRepository.GetAllAsync();
            return courses.Select(course => new CourseDto
            {
                Id = course.Id,
                Name = course.Name,
                Category = course.Category,
                EducationalLevel = course.EducationalLevel.ToString(),
                ImageUrl = course.ImageUrl,
                IntroductoryVideoUrl = course.IntroductoryVideoUrl,
                ShortDescription = course.ShortDescription,
                DetailedDescription = course.DetailedDescription,
                Requirements = course.Requirements,
                WhatStudentsWillLearn = course.WhatStudentsWillLearn,
                NumberOfLessons = course.Lessons.Count
            }).ToList();
        }

        public async Task<CourseDto> CreateAsync(CourseDto courseDto, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Teacher"))
                throw new Exception("Only teachers can create courses.");

            var course = new Course
            {
                Id = Guid.NewGuid(),
                Name = courseDto.Name,
                Category = courseDto.Category,
                EducationalLevel = Enum.Parse<Domain.Enums.EducationalLevel>(courseDto.EducationalLevel),
                ShortDescription = courseDto.ShortDescription,
                DetailedDescription = courseDto.DetailedDescription,
                Requirements = courseDto.Requirements,
                WhatStudentsWillLearn = courseDto.WhatStudentsWillLearn,
                IntroductoryVideoUrl = courseDto.IntroductoryVideoUrl,
                Lessons = courseDto.Lessons?.Select(l => new Lesson
                {
                    Id = l.Id,
                    Title = l.Title,
                    VideoUrl = l.VideoUrl,
                    LessonSummaryText = l.LessonSummaryText,
                    LessonSummaryPdfPath = l.LessonSummaryPdfPath,
                    EquationsTablePdfPath = l.EquationsTablePdfPath,
                    IsFree = l.IsFree,
                    MonthAssigned = l.MonthAssigned,
                    AdditionalResources = l.AdditionalResources,
                    CourseId = l.CourseId
                }).ToList() ?? new List<Lesson>()
            };

            // Handle image upload
            course.ImageUrl = await SaveFileAsync(courseDto.ImageFile, course.Id);

            await _courseRepository.AddAsync(course);

            return new CourseDto
            {
                Id = course.Id,
                Name = course.Name,
                Category = course.Category,
                EducationalLevel = course.EducationalLevel.ToString(),
                ImageUrl = course.ImageUrl,
                IntroductoryVideoUrl = course.IntroductoryVideoUrl,
                ShortDescription = course.ShortDescription,
                DetailedDescription = course.DetailedDescription,
                Requirements = course.Requirements,
                WhatStudentsWillLearn = course.WhatStudentsWillLearn,
                Lessons = course.Lessons?.Select(l => new LessonDto
                {
                    Id = l.Id,
                    Title = l.Title,
                    VideoUrl = l.VideoUrl,
                    LessonSummaryText = l.LessonSummaryText,
                    LessonSummaryPdfPath = l.LessonSummaryPdfPath,
                    EquationsTablePdfPath = l.EquationsTablePdfPath,
                    IsFree = l.IsFree,
                    MonthAssigned = l.MonthAssigned,
                    AdditionalResources = l.AdditionalResources,
                    CourseId = l.CourseId
                }).ToList() ?? new List<LessonDto>()
            };
        }

        public async Task UpdateAsync(CourseDto courseDto, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Teacher"))
                throw new Exception("Only teachers can update courses.");

            var course = await _courseRepository.GetByIdAsync(courseDto.Id);
            if (course == null)
                throw new Exception("Course not found.");

            course.Name = courseDto.Name;
            course.Category = courseDto.Category;
            course.EducationalLevel = Enum.Parse<Domain.Enums.EducationalLevel>(courseDto.EducationalLevel);
            course.ShortDescription = courseDto.ShortDescription;
            course.DetailedDescription = courseDto.DetailedDescription;
            course.Requirements = courseDto.Requirements;
            course.WhatStudentsWillLearn = courseDto.WhatStudentsWillLearn;
            course.IntroductoryVideoUrl = courseDto.IntroductoryVideoUrl;
            course.Lessons = courseDto.Lessons?.Select(l => new Lesson
            {
                Id = l.Id,
                Title = l.Title,
                VideoUrl = l.VideoUrl,
                LessonSummaryText = l.LessonSummaryText,
                LessonSummaryPdfPath = l.LessonSummaryPdfPath,
                EquationsTablePdfPath = l.EquationsTablePdfPath,
                IsFree = l.IsFree,
                MonthAssigned = l.MonthAssigned,
                AdditionalResources = l.AdditionalResources,
                CourseId = l.CourseId
            }).ToList() ?? new List<Lesson>();

            // Handle image upload if a new file is provided
            if (courseDto.ImageFile != null)
            {
                // Delete old image if exists
                DeleteFile(course.ImageUrl);
                // Save new image
                course.ImageUrl = await SaveFileAsync(courseDto.ImageFile, course.Id);
            }

            await _courseRepository.UpdateAsync(course);
        }

        public async Task DeleteAsync(Guid id, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Teacher"))
                throw new Exception("Only teachers can delete courses.");

            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null)
                throw new Exception("Course not found.");

            // Delete image if exists
            DeleteFile(course.ImageUrl);

            await _courseRepository.DeleteAsync(id);
        }

        private async Task<string?> SaveFileAsync(IFormFile? file, Guid courseId)
        {
            if (file == null || file.Length == 0)
                return null;

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
                throw new Exception("Invalid file type. Only JPG and PNG are allowed.");

            // Validate file size (5MB limit)
            if (file.Length > 5 * 1024 * 1024)
                throw new Exception("File size exceeds 5MB.");

            var fileName = $"{courseId}_{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(_uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/courses/{fileName}";
        }

        private void DeleteFile(string? filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                var fullPath = Path.Combine(_uploadPath, filePath.TrimStart('/'));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
        }
    }
}