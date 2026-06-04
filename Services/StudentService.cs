using Lyceum.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Lyceum.Services;

public class StudentService(LyceumDbContext context, UserManager<User> userManager)
{
    public async Task<List<Student>> GetAllAsync(string? search = null, int? courseId = null)
    {
        var query = context.Students
            .Include(s => s.User)
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.Course)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.ToLower();
            query = query.Where(s =>
                s.User.FullName.ToLower().Contains(q) ||
                s.StudentCode.ToLower().Contains(q) ||
                (s.User.Email != null && s.User.Email.ToLower().Contains(q)));
        }

        if (courseId.HasValue)
        {
            query = query.Where(s => s.Enrollments.Any(e => e.CourseId == courseId.Value && e.Status == EnrollmentStatus.Active));
        }

        return await query.OrderByDescending(s => s.EnrollmentDate).ToListAsync();
    }

    public async Task<Student?> GetByIdAsync(int id)
    {
        return await context.Students
            .Include(s => s.User)
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.Course)
            .Include(s => s.Grades)
                .ThenInclude(g => g.Course)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Student?> GetByUserIdAsync(int userId)
    {
        return await context.Students
            .Include(s => s.User)
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.Course)
            .Include(s => s.Grades)
                .ThenInclude(g => g.Course)
            .FirstOrDefaultAsync(s => s.UserId == userId);
    }

    public async Task<Student> CreateAsync(string fullName, string email, string username, string password,
        DateTime? dateOfBirth, string? gender, string? phone, string? address)
    {
        var user = new User
        {
            UserName = username,
            FullName = fullName,
            Email = email,
            Role = UserRole.Student,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidOperationException(result.Errors.First().Description);

        var randomSuffix = new Random().Next(1000, 9999);
        var student = new Student
        {
            UserId = user.Id,
            StudentCode = $"STU-{DateTime.UtcNow.Year}-{randomSuffix}",
            DateOfBirth = dateOfBirth,
            Gender = gender,
            Phone = phone,
            Address = address,
            EnrollmentDate = DateTime.UtcNow
        };

        context.Students.Add(student);
        await context.SaveChangesAsync();
        return student;
    }

    public async Task UpdateAsync(Student student)
    {
        var existing = await context.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == student.Id)
            ?? throw new KeyNotFoundException("Student not found.");

        existing.DateOfBirth = student.DateOfBirth;
        existing.Gender = student.Gender;
        existing.Phone = student.Phone;
        existing.Address = student.Address;
        existing.PhotoUrl = student.PhotoUrl;
        existing.User.FullName = student.User.FullName;
        existing.User.Email = student.User.Email;
        existing.User.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
    }

    public async Task DeactivateAsync(int id)
    {
        var student = await context.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new KeyNotFoundException("Student not found.");

        student.User.IsActive = !student.User.IsActive;
        student.User.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var student = await context.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new KeyNotFoundException("Student not found.");

        var user = student.User;
        context.Students.Remove(student);
        await context.SaveChangesAsync();
        await userManager.DeleteAsync(user);
    }

    public async Task<int> GetCountAsync()
    {
        return await context.Students.CountAsync();
    }
}
