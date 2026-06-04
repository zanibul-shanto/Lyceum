using Lyceum.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Lyceum.Services;

public class TeacherService(LyceumDbContext context, UserManager<User> userManager)
{
    public async Task<List<Teacher>> GetAllAsync(string? search = null, string? department = null)
    {
        var query = context.Teachers
            .Include(t => t.User)
            .Include(t => t.CourseTeachers)
                .ThenInclude(ct => ct.Course)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.ToLower();
            query = query.Where(t =>
                t.User.FullName.ToLower().Contains(q) ||
                t.TeacherCode.ToLower().Contains(q) ||
                (t.Department != null && t.Department.ToLower().Contains(q)));
        }

        if (!string.IsNullOrWhiteSpace(department))
            query = query.Where(t => t.Department == department);

        return await query.OrderBy(t => t.User.FullName).ToListAsync();
    }

    public async Task<Teacher?> GetByIdAsync(int id)
    {
        return await context.Teachers
            .Include(t => t.User)
            .Include(t => t.CourseTeachers)
                .ThenInclude(ct => ct.Course)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Teacher?> GetByUserIdAsync(int userId)
    {
        return await context.Teachers
            .Include(t => t.User)
            .Include(t => t.CourseTeachers)
                .ThenInclude(ct => ct.Course)
            .FirstOrDefaultAsync(t => t.UserId == userId);
    }

    public async Task<Teacher> CreateAsync(string fullName, string email, string username, string password,
        string? department, string? qualification, string? phone, string? officeRoom, string? bio)
    {
        var user = new User
        {
            UserName = username,
            FullName = fullName,
            Email = email,
            Role = UserRole.Teacher,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidOperationException(result.Errors.First().Description);

        var randomSuffix = new Random().Next(100, 999);
        var teacher = new Teacher
        {
            UserId = user.Id,
            TeacherCode = $"TCH-{DateTime.UtcNow.Year}-{randomSuffix}",
            Department = department,
            Qualification = qualification,
            Phone = phone,
            OfficeRoom = officeRoom,
            Bio = bio,
            JoiningDate = DateTime.UtcNow
        };

        context.Teachers.Add(teacher);
        await context.SaveChangesAsync();
        return teacher;
    }

    public async Task UpdateAsync(Teacher teacher)
    {
        var existing = await context.Teachers.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == teacher.Id)
            ?? throw new KeyNotFoundException("Teacher not found.");

        existing.Department = teacher.Department;
        existing.Qualification = teacher.Qualification;
        existing.Phone = teacher.Phone;
        existing.OfficeRoom = teacher.OfficeRoom;
        existing.Bio = teacher.Bio;
        existing.User.FullName = teacher.User.FullName;
        existing.User.Email = teacher.User.Email;
        existing.User.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
    }

    public async Task DeactivateAsync(int id)
    {
        var teacher = await context.Teachers.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new KeyNotFoundException("Teacher not found.");

        teacher.User.IsActive = !teacher.User.IsActive;
        teacher.User.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var teacher = await context.Teachers.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new KeyNotFoundException("Teacher not found.");

        var user = teacher.User;
        context.Teachers.Remove(teacher);
        await context.SaveChangesAsync();
        await userManager.DeleteAsync(user);
    }

    public async Task<int> GetCountAsync()
    {
        return await context.Teachers.CountAsync();
    }

    public async Task<List<string>> GetDepartmentsAsync()
    {
        return await context.Teachers
            .Where(t => t.Department != null)
            .Select(t => t.Department!)
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync();
    }
}
