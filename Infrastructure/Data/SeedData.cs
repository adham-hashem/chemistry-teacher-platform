using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.MigrateAsync();

            if (!await roleManager.RoleExistsAsync("Teacher"))
                await roleManager.CreateAsync(new IdentityRole("Teacher"));

            if (!await roleManager.RoleExistsAsync("Student"))
                await roleManager.CreateAsync(new IdentityRole("Student"));

            var teacher = new ApplicationUser
            {
                UserName = "teacher@platform.com",
                Email = "teacher@platform.com",
                FirstName = "Teacher",
                LastName = "One",
                Grade = EducationalLevel.FirstYear,
                CreatedAt = DateTime.UtcNow
            };

            var student = new ApplicationUser
            {
                UserName = "student@platform.com",
                Email = "student@platform.com",
                FirstName = "Student",
                LastName = "One",
                Grade = EducationalLevel.FirstYear,
                CreatedAt = DateTime.UtcNow
            };

            if (await userManager.FindByEmailAsync(teacher.Email) == null)
            {
                var result = await userManager.CreateAsync(teacher, "@TeacherA123456");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(teacher, "Teacher");
            }

            if (await userManager.FindByEmailAsync(student.Email) == null)
            {
                var result = await userManager.CreateAsync(student, "Student123!");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(student, "Student");
            }

            if (!context.Courses.Any())
            {
                var course = new Course
                {
                    Id = Guid.NewGuid(),
                    Name = "Sample Course",
                    Category = "Organic chemistry",
                    EducationalLevel = EducationalLevel.FirstYear,
                    ShortDescription = "A sample course for testing",
                    DetailedDescription = "This is a detailed description of the sample course.",
                    Requirements = "None",
                    WhatStudentsWillLearn = "Basic chemistry concepts",
                    Lessons = new System.Collections.Generic.List<Lesson>
                    {
                        new Lesson
                        {
                            Id = Guid.NewGuid(),
                            Title = "Introduction to Chemistry",
                            VideoUrl = "https://example.com/video1",
                            IsFree = true,
                            MonthAssigned = 1
                        }
                    }
                };

                await context.Courses.AddAsync(course);
                await context.SaveChangesAsync();
            }
        }
    }
}
