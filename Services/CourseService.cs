using Lyceum.Models;
using Microsoft.EntityFrameworkCore;

namespace Lyceum.Services;

public class CourseService(LyceumDbContext context)
{
    public async Task<List<Course>> GetAllAsync()
    {
        return await context.Courses
            .Include(c => c.Subjects)
            .Include(c => c.Enrollments)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<Course?> GetByIdAsync(int id)
    {
        return await context.Courses
            .Include(c => c.Subjects)
            .Include(c => c.Enrollments)
                .ThenInclude(e => e.Student)
                    .ThenInclude(s => s.User)
            .Include(c => c.Timetables)
                .ThenInclude(t => t.Teacher)
                    .ThenInclude(t => t.User)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task CreateAsync(Course course)
    {
        bool codeExists = await context.Courses
            .AnyAsync(c => c.CourseCode == course.CourseCode);
        if (codeExists)
            throw new InvalidOperationException($"Course code '{course.CourseCode}' is already in use.");

        course.CreatedAt = DateTime.UtcNow;

        context.Courses.Add(course);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Course course)
    {
        bool codeExists = await context.Courses
            .AnyAsync(c => c.CourseCode == course.CourseCode && c.Id != course.Id);
        if (codeExists)
            throw new InvalidOperationException($"Course code '{course.CourseCode}' is already in use.");

        var existing = await context.Courses
            .FirstOrDefaultAsync(c => c.Id == course.Id)
            ?? throw new KeyNotFoundException("Course not found.");

        existing.Name = course.Name;
        existing.CourseCode = course.CourseCode;
        existing.Description = course.Description;
        existing.Credits = course.Credits;
        existing.MaxCapacity = course.MaxCapacity;
        existing.IsActive = course.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var course = await context.Courses.FindAsync(id)
            ?? throw new KeyNotFoundException("Course not found.");

        context.Courses.Remove(course);
        await context.SaveChangesAsync();
    }

    public async Task ToggleActiveAsync(int id)
    {
        var course = await context.Courses.FindAsync(id)
            ?? throw new KeyNotFoundException("Course not found.");

        course.IsActive = !course.IsActive;
        course.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    public async Task<int> GetCountAsync(bool? activeOnly = null)
    {
        var query = context.Courses.AsQueryable();
        if (activeOnly.HasValue)
            query = query.Where(c => c.IsActive == activeOnly.Value);
        return await query.CountAsync();
    }
}
