using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos.LessonDtos;
using Microsoft.AspNetCore.Http;

namespace Application.Dtos.CourseDtos
{
    public class CourseDto
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = string.Empty;

        [Required]
        public string EducationalLevel { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public IFormFile? ImageFile { get; set; }
        public string? IntroductoryVideoUrl { get; set; }

        [Required]
        public string ShortDescription { get; set; } = string.Empty;
        public string DetailedDescription { get; set; } = string.Empty;
        public string Requirements { get; set; } = string.Empty;
        public string WhatStudentsWillLearn { get; set; } = string.Empty;
        public List<LessonDto>? Lessons { get; set; } = new List<LessonDto>();
    }
}
