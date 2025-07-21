using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos.LessonAccessCodeDtos;
using Application.Dtos.RedeemCodeDtos;

namespace Application.Services.Interfaces
{
    public interface ILessonAccessCodeService
    {
        Task<LessonAccessCodeDto> GenerateCodeAsync(Guid lessonId, string teacherId);
        Task<LessonAccessCodeDto> RedeemCodeAsync(RedeemCodeDto redeemCodeDto, string userId);
        Task<List<LessonAccessCodeDto>> GetCodesByLessonAsync(Guid lessonId, string teacherId);
        Task RevokeAccessAsync(Guid lessonId, string userId, string teacherId);
        Task RevokeAllAccessAsync(Guid lessonId, string teacherId);
        Task DeleteCodesByLessonAsync(Guid lessonId, string teacherId);
        Task DeleteCodeAsync(Guid codeId, string teacherId);
    }
}
