using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Dtos.CourseDtos;
using Application.Dtos.LessonDtos;
using Application.Repositories.Interfaces;
using Application.Services.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Services.Implementations
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public CourseService(ICourseRepository courseRepository, UserManager<ApplicationUser> userManager)
        {
            _courseRepository = courseRepository;
            _userManager = userManager;
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

        public async Task<List<CourseDto>> GetAllAsync()
        {
            var courses = await _courseRepository.GetAllAsync();
            return courses.Select(course => new CourseDto
            {
                Id = course.Id,
                Name = course.Name,
                Category = course.Category,
                EducationalLevel = course.EducationalLevel.ToString(),
                ShortDescription = course.ShortDescription,
                DetailedDescription = course.DetailedDescription,
                Requirements = course.Requirements,
                WhatStudentsWillLearn = course.WhatStudentsWillLearn
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

            await _courseRepository.AddAsync(course);

            return new CourseDto
            {
                Id = course.Id,
                Name = course.Name,
                Category = course.Category,
                EducationalLevel = course.EducationalLevel.ToString(),
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

            await _courseRepository.DeleteAsync(id);
        }
    }
}