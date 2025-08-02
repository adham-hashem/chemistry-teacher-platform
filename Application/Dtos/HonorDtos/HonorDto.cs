using System;
using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.Dtos.HonorDtos
{
    public class HonorDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Student ID is required.")]
        public string StudentId { get; set; }

        public string? TeacherId { get; set; }

        public string? StudentName { get; set; }

        public EducationalLevel? Grade { get; set; }

        public string? TeacherName { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "Description must be between 1 and 1000 characters.")]
        public string Description { get; set; }

        public string? StudentImageUrl { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}