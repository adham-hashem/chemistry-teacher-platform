using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Dtos.SubscriptionDtos;
using Application.Repositories.Interfaces;
using Application.Services.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Application.Services.Implementations
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public SubscriptionService(
            ISubscriptionRepository subscriptionRepository,
            ILessonRepository lessonRepository,
            UserManager<ApplicationUser> userManager)
        {
            _subscriptionRepository = subscriptionRepository;
            _lessonRepository = lessonRepository;
            _userManager = userManager;
        }

        public async Task<SubscriptionDto> CreateSubscriptionAsync(SubscriptionDto subscriptionDto, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found.");

            var subscriptionType = Enum.Parse<SubscriptionType>(subscriptionDto.Type);

            var subscription = new Subscription
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = subscriptionType,
                StartDate = subscriptionDto.StartDate,
                EndDate = subscriptionDto.EndDate,
                SubscribedMonths = subscriptionDto.SubscribedMonths,
                Grade = Enum.Parse<Domain.Enums.EducationalLevel>(subscriptionDto.Grade),
                LectureCount = subscriptionDto.LectureCount,
                Price = subscriptionDto.Price,
                AccessedLessons = subscriptionDto.AccessedLessons,
                IsActive = subscriptionType == SubscriptionType.Monthly
            };

            if (subscription.Grade != user.Grade)
                throw new Exception("Subscription grade must match user's grade.");

            if (subscriptionType == SubscriptionType.Yearly)
            {
                var currentYear = DateTime.UtcNow.Year;
                subscription.StartDate = new DateTime(currentYear, 8, 1, 0, 0, 0, DateTimeKind.Utc);
                subscription.EndDate = new DateTime(currentYear + 1, 8, 1, 0, 0, 0, DateTimeKind.Utc);
                subscription.SubscribedMonths = Enumerable.Range(1, 12).ToList();
            }
            else if (subscriptionType == SubscriptionType.LectureBased)
            {
                if (subscription.LectureCount == null || subscription.LectureCount <= 0)
                    throw new Exception("Lecture count must be specified and greater than zero for lecture-based subscriptions.");
                if (subscription.Price <= 0)
                    throw new Exception("Price must be greater than zero for lecture-based subscriptions.");
            }
            else
            {
                subscription.LectureCount = null;
                subscription.Price = 0;
            }

            await _subscriptionRepository.AddAsync(subscription);

            return new SubscriptionDto
            {
                Id = subscription.Id,
                UserId = subscription.UserId,
                Type = subscription.Type.ToString(),
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
                SubscribedMonths = subscription.SubscribedMonths,
                Grade = subscription.Grade.ToString(),
                LectureCount = subscription.LectureCount,
                Price = subscription.Price,
                AccessedLessons = subscription.AccessedLessons
            };
        }

        public async Task<SubscriptionDto> CreateLectureBasedSubscriptionAsync(SubscriptionDto subscriptionDto, string teacherId)
        {
            var teacher = await _userManager.FindByIdAsync(teacherId);
            if (teacher == null || !await _userManager.IsInRoleAsync(teacher, "Teacher"))
                throw new Exception("Only teachers can create lecture-based subscriptions.");

            var subscriptionType = Enum.Parse<SubscriptionType>(subscriptionDto.Type);
            if (subscriptionType != SubscriptionType.LectureBased)
                throw new Exception("Only lecture-based subscriptions can be created via this endpoint.");

            if (subscriptionDto.LectureCount == null || subscriptionDto.LectureCount <= 0)
                throw new Exception("Lecture count must be specified and greater than zero.");
            if (subscriptionDto.Price <= 0)
                throw new Exception("Price must be greater than zero.");

            var subscription = new Subscription
            {
                Id = Guid.NewGuid(),
                UserId = subscriptionDto.UserId,
                Type = subscriptionType,
                StartDate = subscriptionDto.StartDate,
                EndDate = subscriptionDto.EndDate,
                SubscribedMonths = subscriptionDto.SubscribedMonths,
                Grade = Enum.Parse<Domain.Enums.EducationalLevel>(subscriptionDto.Grade),
                LectureCount = subscriptionDto.LectureCount,
                Price = subscriptionDto.Price,
                AccessedLessons = subscriptionDto.AccessedLessons,
                IsActive = false
            };

            await _subscriptionRepository.AddAsync(subscription);

            return new SubscriptionDto
            {
                Id = subscription.Id,
                UserId = subscription.UserId,
                Type = subscription.Type.ToString(),
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
                SubscribedMonths = subscription.SubscribedMonths,
                Grade = subscription.Grade.ToString(),
                LectureCount = subscription.LectureCount,
                Price = subscription.Price,
                AccessedLessons = subscription.AccessedLessons
            };
        }

        public async Task<bool> CanAccessLessonAsync(string userId, Guid lessonId)
        {
            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (lesson == null)
                return false;

            var hasAccessCode = await _lessonRepository.HasAccessCodeAsync(lessonId, userId);
            if (hasAccessCode)
                return true;

            var subscriptions = await _subscriptionRepository.GetByUserIdAsync(userId);

            foreach (var subscription in subscriptions)
            {
                if (!subscription.IsActive || subscription.Grade != lesson.Course.EducationalLevel)
                    continue;

                if (subscription.Type == SubscriptionType.Monthly ||
                    subscription.Type == SubscriptionType.Yearly)
                {
                    if (subscription.SubscribedMonths.Contains(DateTime.UtcNow.Month))
                        return true;
                }
                else if (subscription.Type == SubscriptionType.LectureBased)
                {
                    if (subscription.LectureCount > subscription.AccessedLessons.Count &&
                        !subscription.AccessedLessons.Contains(lessonId))
                        return true;
                }
            }

            return lesson.IsFree;
        }

        public async Task MarkLessonAsAccessedAsync(string userId, Guid lessonId)
        {
            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (lesson == null)
                throw new Exception("Lesson not found.");

            var hasAccessCode = await _lessonRepository.HasAccessCodeAsync(lessonId, userId);
            if (hasAccessCode)
                return;

            var subscriptions = await _subscriptionRepository.GetByUserIdAsync(userId);
            var validSubscription = subscriptions.FirstOrDefault(s =>
                s.IsActive &&
                s.Type == SubscriptionType.LectureBased &&
                s.Grade == lesson.Course.EducationalLevel &&
                s.LectureCount > s.AccessedLessons.Count &&
                !s.AccessedLessons.Contains(lessonId));

            if (validSubscription == null)
                throw new Exception("No valid subscription to access this lesson.");

            validSubscription.AccessedLessons.Add(lessonId);
            await _subscriptionRepository.UpdateAsync(validSubscription);
        }

        public async Task DeleteUserSubscriptionsAsync(string userId, string teacherId)
        {
            var teacher = await _userManager.FindByIdAsync(teacherId);
            if (teacher == null || !await _userManager.IsInRoleAsync(teacher, "Teacher"))
                throw new Exception("Only teachers can delete subscriptions.");

            await _subscriptionRepository.DeleteByUserIdAsync(userId);
        }

        public async Task DeleteAllSubscriptionsAsync(string teacherId)
        {
            var teacher = await _userManager.FindByIdAsync(teacherId);
            if (teacher == null || !await _userManager.IsInRoleAsync(teacher, "Teacher"))
                throw new Exception("Only teachers can delete subscriptions.");

            var subscriptions = await _subscriptionRepository.GetAllAsync();
            foreach (var subscription in subscriptions)
            {
                await _subscriptionRepository.DeleteAsync(subscription.Id);
            }
        }
    }
}