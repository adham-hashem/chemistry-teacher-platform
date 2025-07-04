using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos.AuthDtos;
using Domain.Entities;

namespace Application.Services.Interfaces
{
    public interface IJwtService
    {
        Task<TokenResponseDto> GenerateJwtToken(ApplicationUser user);
        Task<bool> ValidateRefreshTokenAsync(string refreshToken);
    }
}
