using Lyceum.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Lyceum.Services;

public class UserService
{
    private readonly UserManager<User> _userManager;

    public UserService(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<User?> AuthenticateAsync(string usernameOrEmail, string password)
    {
        var user = await _userManager.FindByNameAsync(usernameOrEmail) 
                   ?? await _userManager.FindByEmailAsync(usernameOrEmail);

        if (user == null) return null;

        var result = _userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash ?? "", password);
        if (result == PasswordVerificationResult.Success)
        {
            return user;
        }

        return null;
    }

    public async Task<User> RegisterAsync(User user, string password)
    {
        // Auto-assign IDs if student/teacher
        if (user.Role == UserRole.Student && string.IsNullOrEmpty(user.StudentId))
        {
            var randomSuffix = new Random().Next(1000, 9999);
            user.StudentId = $"STU-{DateTime.UtcNow.Year}-{randomSuffix}";
            user.GPA ??= 0.0;
            user.AttendanceRate ??= 100.0;
        }
        else if (user.Role == UserRole.Teacher && string.IsNullOrEmpty(user.TeacherId))
        {
            var randomSuffix = new Random().Next(100, 999);
            user.TeacherId = $"TCH-{DateTime.UtcNow.Year}-{randomSuffix}";
        }

        user.CreatedAt = DateTime.UtcNow;

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var firstError = result.Errors.FirstOrDefault()?.Description ?? "Registration failed.";
            throw new InvalidOperationException(firstError);
        }

        return user;
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _userManager.Users.OrderByDescending(u => u.CreatedAt).ToListAsync();
    }

    public async Task<List<User>> GetUsersByRoleAsync(UserRole role)
    {
        return await _userManager.Users.Where(u => u.Role == role).OrderBy(u => u.FullName).ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _userManager.FindByIdAsync(id.ToString());
    }

    public async Task<User> UpdateUserAsync(User user)
    {
        var existing = await _userManager.FindByIdAsync(user.Id.ToString());
        if (existing == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        // Update fields
        existing.FullName = user.FullName;
        existing.Email = user.Email;
        existing.Role = user.Role;
        
        // Student fields
        existing.StudentId = user.StudentId;
        existing.GradeLevel = user.GradeLevel;
        existing.GPA = user.GPA;
        existing.AttendanceRate = user.AttendanceRate;

        // Teacher fields
        existing.TeacherId = user.TeacherId;
        existing.Department = user.Department;
        existing.Bio = user.Bio;
        existing.OfficeRoom = user.OfficeRoom;

        var result = await _userManager.UpdateAsync(existing);
        if (!result.Succeeded)
        {
            var firstError = result.Errors.FirstOrDefault()?.Description ?? "Update failed.";
            throw new InvalidOperationException(firstError);
        }

        return existing;
    }

    public async Task DeleteUserAsync(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user != null)
        {
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                var firstError = result.Errors.FirstOrDefault()?.Description ?? "Delete failed.";
                throw new InvalidOperationException(firstError);
            }
        }
    }
}
