using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;

namespace Domain.Entities
{
    public class Course
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        public string Category { get; set; } = string.Empty;
        public EducationalLevel EducationalLevel { get; set; } // FirstYear, SecondYear, ThirdYear

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [StringLength(500)]
        public string? IntroductoryVideoUrl { get; set; }

        [StringLength(500)]
        public string ShortDescription { get; set; } = string.Empty;

        [StringLength(2000)]
        public string DetailedDescription { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Requirements { get; set; } = string.Empty;

        [StringLength(1000)]
        public string WhatStudentsWillLearn { get; set; } = string.Empty;
        public List<Lesson> Lessons { get; set; } = new List<Lesson>();
        public List<Review> Reviews { get; set; } = new List<Review>();
    }
}
