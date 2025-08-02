using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Dtos.HonorDtos;
using Application.Repositories.Interfaces;
using Application.Services.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace Application.Services.Implementations
{
    public class HonorService : IHonorService
    {
        private readonly IHonorRepository _honorRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly string _uploadPath;

        public HonorService(
            IHonorRepository honorRepository,
            UserManager<ApplicationUser> userManager)
        {
            _honorRepository = honorRepository;
            _userManager = userManager;
            _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/honors");
            Directory.CreateDirectory(_uploadPath);
        }

        public async Task<List<HonorDto>> GetAllAsync(string requestingUserId)
        {
            var requestingUser = await _userManager.FindByIdAsync(requestingUserId);
            if (requestingUser == null)
                throw new UnauthorizedAccessException("Requesting user not found.");

            bool isTeacher = await _userManager.IsInRoleAsync(requestingUser, "Teacher");
            bool isStudent = await _userManager.IsInRoleAsync(requestingUser, "Student");
            if (!isTeacher && !isStudent)
                throw new UnauthorizedAccessException("User must be a student or teacher.");

            var honors = await _honorRepository.GetAllAsync();
            var honorDtos = new List<HonorDto>();

            foreach (var honor in honors)
            {
                // Students can only see their own honors
                //if (isStudent && honor.StudentId != requestingUserId)
                //    continue;

                var student = await _userManager.FindByIdAsync(honor.StudentId);
                var teacher = await _userManager.FindByIdAsync(honor.TeacherId);

                honorDtos.Add(new HonorDto
                {
                    Id = honor.Id,
                    StudentId = honor.StudentId,
                    TeacherId = honor.TeacherId,
                    StudentName = student != null ? $"{student.FirstName} {student.LastName}" : "Unknown",
                    Grade = student?.Grade,
                    TeacherName = teacher != null ? $"{teacher.FirstName} {teacher.LastName}" : "Unknown",
                    Description = honor.Description,
                    StudentImageUrl = honor.StudentImageUrl,
                    CreatedAt = honor.CreatedAt
                });
            }

            return honorDtos;
        }

        public async Task<HonorDto> CreateAsync(HonorDto honorDto, string teacherId, IFormFile? studentImage = null)
        {
            if (honorDto == null)
                throw new ArgumentNullException(nameof(honorDto), "Honor data is required.");

            if (string.IsNullOrWhiteSpace(honorDto.StudentId))
                throw new ArgumentException("Student ID is required.", nameof(honorDto.StudentId));

            if (string.IsNullOrWhiteSpace(honorDto.Description))
                throw new ArgumentException("Description is required.", nameof(honorDto.Description));

            var teacher = await _userManager.FindByIdAsync(teacherId);
            if (teacher == null || !await _userManager.IsInRoleAsync(teacher, "Teacher"))
                throw new UnauthorizedAccessException("Only teachers can create honors.");

            var student = await _userManager.FindByIdAsync(honorDto.StudentId);
            if (student == null || !await _userManager.IsInRoleAsync(student, "Student"))
                throw new ArgumentException("Invalid student ID.", nameof(honorDto.StudentId));

            var honor = new Honor
            {
                Id = Guid.NewGuid(),
                StudentId = honorDto.StudentId,
                TeacherId = teacherId,
                Description = honorDto.Description,
                StudentImageUrl = await SaveFileAsync(studentImage, Guid.NewGuid()), // Use new Guid for file name
                CreatedAt = DateTime.UtcNow
            };

            await _honorRepository.AddAsync(honor);

            return new HonorDto
            {
                Id = honor.Id,
                StudentId = honor.StudentId,
                TeacherId = honor.TeacherId,
                StudentName = $"{student.FirstName} {student.LastName}",
                TeacherName = $"{teacher.FirstName} {teacher.LastName}",
                Description = honor.Description,
                StudentImageUrl = honor.StudentImageUrl,
                CreatedAt = honor.CreatedAt
            };
        }

        public async Task<HonorDto> GetByIdAsync(Guid id, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new UnauthorizedAccessException("User not found.");

            var honor = await _honorRepository.GetByIdAsync(id);
            if (honor == null)
                throw new KeyNotFoundException("Honor not found.");

            bool isTeacher = await _userManager.IsInRoleAsync(user, "Teacher");
            bool isStudent = await _userManager.IsInRoleAsync(user, "Student");
            if (!isTeacher && !isStudent)
                throw new UnauthorizedAccessException("User must be a student or teacher.");
            if (isStudent && honor.StudentId != userId)
                throw new UnauthorizedAccessException("Students can only view their own honors.");

            var student = await _userManager.FindByIdAsync(honor.StudentId);
            var teacher = await _userManager.FindByIdAsync(honor.TeacherId);

            return new HonorDto
            {
                Id = honor.Id,
                StudentId = honor.StudentId,
                TeacherId = honor.TeacherId,
                StudentName = student != null ? $"{student.FirstName} {student.LastName}" : "Unknown",
                TeacherName = teacher != null ? $"{teacher.FirstName} {teacher.LastName}" : "Unknown",
                Description = honor.Description,
                StudentImageUrl = honor.StudentImageUrl,
                CreatedAt = honor.CreatedAt
            };
        }

        public async Task<List<HonorDto>> GetByStudentIdAsync(string requestingUserId)
        {
            var requestingUser = await _userManager.FindByIdAsync(requestingUserId);
            if (requestingUser == null)
                throw new UnauthorizedAccessException("Requesting user not found.");

            bool isTeacher = await _userManager.IsInRoleAsync(requestingUser, "Teacher");
            bool isStudent = await _userManager.IsInRoleAsync(requestingUser, "Student");
            if (!isTeacher && !isStudent)
                throw new UnauthorizedAccessException("User must be a student or teacher.");
            if (!isStudent)
                throw new UnauthorizedAccessException("Students can only view their own honors.");

            //var student = await _userManager.FindByIdAsync(studentId);
            //if (student == null || !await _userManager.IsInRoleAsync(student, "Student"))
            //    throw new ArgumentException("Invalid student ID.", nameof(studentId));

            var honors = await _honorRepository.GetByStudentIdAsync(requestingUserId);
            var honorDtos = new List<HonorDto>();
            foreach (var honor in honors)
            {
                var teacher = await _userManager.FindByIdAsync(honor.TeacherId);
                honorDtos.Add(new HonorDto
                {
                    Id = honor.Id,
                    StudentId = honor.StudentId,
                    TeacherId = honor.TeacherId,
                    StudentName = $"{requestingUser.FirstName} {requestingUser.LastName}",
                    TeacherName = teacher != null ? $"{teacher.FirstName} {teacher.LastName}" : "Unknown",
                    Grade = requestingUser?.Grade,
                    Description = honor.Description,
                    StudentImageUrl = honor.StudentImageUrl,
                    CreatedAt = honor.CreatedAt
                });
            }
            return honorDtos;
        }

        public async Task<List<HonorDto>> GetByTeacherIdAsync(string teacherId, string requestingUserId)
        {
            var requestingUser = await _userManager.FindByIdAsync(requestingUserId);
            if (requestingUser == null)
                throw new UnauthorizedAccessException("Requesting user not found.");

            bool isTeacher = await _userManager.IsInRoleAsync(requestingUser, "Teacher");
            if (!isTeacher || teacherId != requestingUserId)
                throw new UnauthorizedAccessException("Only teachers can view their own created honors.");

            var teacher = await _userManager.FindByIdAsync(teacherId);
            if (teacher == null || !await _userManager.IsInRoleAsync(teacher, "Teacher"))
                throw new ArgumentException("Invalid teacher ID.", nameof(teacherId));

            var honors = await _honorRepository.GetByTeacherIdAsync(teacherId);
            var honorDtos = new List<HonorDto>();
            foreach (var honor in honors)
            {
                var student = await _userManager.FindByIdAsync(honor.StudentId);
                honorDtos.Add(new HonorDto
                {
                    Id = honor.Id,
                    StudentId = honor.StudentId,
                    TeacherId = honor.TeacherId,
                    StudentName = student != null ? $"{student.FirstName} {student.LastName}" : "Unknown",
                    TeacherName = $"{teacher.FirstName} {teacher.LastName}",
                    Description = honor.Description,
                    StudentImageUrl = honor.StudentImageUrl,
                    CreatedAt = honor.CreatedAt
                });
            }
            return honorDtos;
        }

        public async Task DeleteAsync(Guid id, string teacherId)
        {
            var teacher = await _userManager.FindByIdAsync(teacherId);
            if (teacher == null || !await _userManager.IsInRoleAsync(teacher, "Teacher"))
                throw new UnauthorizedAccessException("Only teachers can delete honors.");

            var honor = await _honorRepository.GetByIdAsync(id);
            if (honor == null)
                throw new KeyNotFoundException("Honor not found.");

            if (honor.TeacherId != teacherId)
                throw new UnauthorizedAccessException("Only the teacher who created the honor can delete it.");

            DeleteFile(honor.StudentImageUrl);
            await _honorRepository.DeleteAsync(id);
        }

        private async Task<string?> SaveFileAsync(IFormFile? file, Guid fileId)
        {
            if (file == null || file.Length == 0)
                return null;

            var fileExtension = Path.GetExtension(file.FileName);
            if (!new[] { ".jpg", ".jpeg", ".png" }.Contains(fileExtension.ToLower()))
                throw new ArgumentException("Invalid image file format. Only JPG, JPEG, or PNG allowed.");

            var fileName = $"{fileId}{fileExtension}";
            var filePath = Path.Combine(_uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/honors/{fileName}";
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