using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Dtos.HonorDtos;
using Microsoft.AspNetCore.Http;

namespace Application.Services.Interfaces
{
    public interface IHonorService
    {
        Task<HonorDto> CreateAsync(HonorDto honorDto, string teacherId, IFormFile? studentImage = null);
        Task<HonorDto> GetByIdAsync(Guid id, string userId);
        Task<List<HonorDto>> GetByStudentIdAsync(string requestingUserId);
        Task<List<HonorDto>> GetByTeacherIdAsync(string teacherId, string requestingUserId);
        Task<List<HonorDto>> GetAllAsync(string requestingUserId);
        Task DeleteAsync(Guid id, string teacherId);
    }
}