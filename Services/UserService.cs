using Lyceum.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Lyceum.Services;

public class UserService(UserManager<User> userManager, SignInManager<User> signInManager, LyceumDbContext context)
{
    public async Task<User?> AuthenticateAsync(string usernameOrEmail, string password)
    {
        var user = await userManager.FindByNameAsync(usernameOrEmail)
                   ?? await userManager.FindByEmailAsync(usernameOrEmail);

        if (user == null || !user.IsActive) return null;

        var result = await signInManager.CheckPasswordSignInAsync(user, password, false);
        return result.Succeeded ? user : null;
    }

    public async Task RegisterAsync(User user, string password)
    {
        user.CreatedAt = DateTime.UtcNow;
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidOperationException(result.Errors.First().Description);

        if (user.Role == UserRole.Student)
        {
            var student = new Student
            {
                UserId = user.Id,
                StudentCode = $"STU-{DateTime.UtcNow.Year}-{new Random().Next(1000, 9999)}",
                EnrollmentDate = DateTime.UtcNow
            };
            context.Students.Add(student);
            await context.SaveChangesAsync();
        }
        else if (user.Role == UserRole.Teacher)
        {
            var teacher = new Teacher
            {
                UserId = user.Id,
                TeacherCode = $"TCH-{DateTime.UtcNow.Year}-{new Random().Next(100, 999)}",
                JoiningDate = DateTime.UtcNow
            };
            context.Teachers.Add(teacher);
            await context.SaveChangesAsync();
        }
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await userManager.Users.OrderByDescending(u => u.CreatedAt).ToListAsync();
    }

    public async Task<List<User>> GetUsersByRoleAsync(UserRole role)
    {
        return await userManager.Users.Where(u => u.Role == role).OrderBy(u => u.FullName).ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await userManager.FindByIdAsync(id.ToString());
    }

    public async Task UpdateUserAsync(User user)
    {
        var existing = await userManager.FindByIdAsync(user.Id.ToString())
            ?? throw new KeyNotFoundException("User not found.");

        existing.FullName = user.FullName;
        existing.Email = user.Email;
        existing.Role = user.Role;
        existing.IsActive = user.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        var result = await userManager.UpdateAsync(existing);
        if (!result.Succeeded)
            throw new InvalidOperationException(result.Errors.First().Description);
    }

    public async Task DeleteUserAsync(int id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user != null)
        {
            var result = await userManager.DeleteAsync(user);
            if (!result.Succeeded)
                throw new InvalidOperationException(result.Errors.First().Description);
        }
    }

    public async Task<int> GetCountByRoleAsync(UserRole role)
    {
        return await userManager.Users.CountAsync(u => u.Role == role);
    }
}
