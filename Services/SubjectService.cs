using Lyceum.Models;
using Microsoft.EntityFrameworkCore;

namespace Lyceum.Services;

public class SubjectService(LyceumDbContext context)
{
    public async Task<List<Subject>> GetByCourseAsync(int courseId)
    {
        return await context.Subjects
            .Where(s => s.CourseId == courseId)
            .OrderBy(s => s.SubjectCode)
            .ToListAsync();
    }

    public async Task<Subject?> GetByIdAsync(int id)
    {
        return await context.Subjects
            .Include(s => s.Course)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task CreateAsync(Subject subject)
    {
        bool exists = await context.Subjects.AnyAsync(s =>
            s.CourseId == subject.CourseId && s.SubjectCode == subject.SubjectCode);
        if (exists)
            throw new InvalidOperationException($"Subject code '{subject.SubjectCode}' already exists in this course.");

        context.Subjects.Add(subject);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Subject subject)
    {
        var existing = await context.Subjects.FindAsync(subject.Id)
            ?? throw new KeyNotFoundException("Subject not found.");

        existing.SubjectName = subject.SubjectName;
        existing.SubjectCode = subject.SubjectCode;
        existing.Description = subject.Description;
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var subject = await context.Subjects.FindAsync(id)
            ?? throw new KeyNotFoundException("Subject not found.");

        context.Subjects.Remove(subject);
        await context.SaveChangesAsync();
    }
}
