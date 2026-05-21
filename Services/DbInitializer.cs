using Lyceum.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Lyceum.Services;

public static class DbInitializer
{
    public static async Task InitializeAsync(LyceumDbContext context, UserManager<User> userManager)
    {
        // Automatically apply database migrations on startup
        await context.Database.MigrateAsync();

        // Seed data if empty
        if (!await userManager.Users.AnyAsync())
        {
            var seedUsers = new List<(User User, string Password)>
            {
                (new User
                {
                    UserName = "admin",
                    FullName = "System Administrator",
                    Email = "admin@lyceum.edu",
                    Role = UserRole.Admin,
                    CreatedAt = DateTime.UtcNow.AddMonths(-3)
                }, "Admin@123"),
                (new User
                {
                    UserName = "teacher",
                    FullName = "Prof. Diana Vance",
                    Email = "diana.vance@lyceum.edu",
                    Role = UserRole.Teacher,
                    TeacherId = "TCH-2026-088",
                    Department = "Computer Science & Engineering",
                    Bio = "Diana is an Associate Professor of Computer Science with 12+ years of teaching experience. She specializes in Software Engineering and Database Design.",
                    OfficeRoom = "Science Hall, Room 402",
                    CreatedAt = DateTime.UtcNow.AddMonths(-2)
                }, "Teacher@123"),
                (new User
                {
                    UserName = "student",
                    FullName = "Alex Chen",
                    Email = "alex.chen@lyceum.edu",
                    Role = UserRole.Student,
                    StudentId = "STU-2026-1049",
                    GradeLevel = "Junior (3rd Year)",
                    GPA = 3.7,
                    AttendanceRate = 96.5,
                    CreatedAt = DateTime.UtcNow.AddMonths(-1)
                }, "Student@123"),
                (new User
                {
                    UserName = "jdoe",
                    FullName = "John Doe",
                    Email = "john.doe@lyceum.edu",
                    Role = UserRole.Student,
                    StudentId = "STU-2026-1050",
                    GradeLevel = "Sophomore (2nd Year)",
                    GPA = 3.15,
                    AttendanceRate = 89.2,
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                }, "Student@123"),
                (new User
                {
                    UserName = "msmith",
                    FullName = "Dr. Michael Smith",
                    Email = "michael.smith@lyceum.edu",
                    Role = UserRole.Teacher,
                    TeacherId = "TCH-2026-015",
                    Department = "Mathematics",
                    Bio = "Michael research focuses on abstract algebra and complex systems. He teaches Calculus III and Linear Algebra.",
                    OfficeRoom = "Newton Wing, Room 102",
                    CreatedAt = DateTime.UtcNow.AddMonths(-5)
                }, "Teacher@123")
            };

            foreach (var (user, password) in seedUsers)
            {
                await userManager.CreateAsync(user, password);
            }
        }
        else
        {
            var existingStudent = await userManager.FindByNameAsync("student");
            if (existingStudent != null && (existingStudent.FullName != "Alex Chen" || existingStudent.GPA != 3.7))
            {
                existingStudent.FullName = "Alex Chen";
                existingStudent.GPA = 3.7;
                existingStudent.Email = "alex.chen@lyceum.edu";
                await userManager.UpdateAsync(existingStudent);
            }
        }
    }
}
