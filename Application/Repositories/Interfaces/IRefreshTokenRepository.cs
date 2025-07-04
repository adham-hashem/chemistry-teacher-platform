using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Repositories.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task AddAsync(RefreshToken refreshToken);
        Task<RefreshToken> GetByTokenAsync(string token);
        Task RevokeAsync(string token);
    }
}
