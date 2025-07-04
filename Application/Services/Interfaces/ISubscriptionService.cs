using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos.SubscriptionDtos;

namespace Application.Services.Interfaces
{
    public interface ISubscriptionService
    {
        Task<SubscriptionDto> CreateSubscriptionAsync(SubscriptionDto subscriptionDto, string userId);
        Task<SubscriptionDto> CreateLectureBasedSubscriptionAsync(SubscriptionDto subscriptionDto, string teacherId);
        Task<bool> CanAccessLessonAsync(string userId, Guid lessonId);
        Task MarkLessonAsAccessedAsync(string userId, Guid lessonId);
        Task DeleteUserSubscriptionsAsync(string userId, string teacherId);
        Task DeleteAllSubscriptionsAsync(string teacherId);
    }
}
