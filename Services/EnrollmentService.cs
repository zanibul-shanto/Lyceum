using Lyceum.Models;
using Microsoft.EntityFrameworkCore;

namespace Lyceum.Services;

public class EnrollmentService(LyceumDbContext context)
{
    public async Task<List<StudentEnrollment>> GetByCourseAsync(int courseId)
    {
        return await context.StudentEnrollments
            .Include(e => e.Student)
                .ThenInclude(s => s.User)
            .Where(e => e.CourseId == courseId)
            .OrderBy(e => e.Student.User.FullName)
            .ToListAsync();
    }

    public async Task<List<StudentEnrollment>> GetByStudentAsync(int studentId)
    {
        return await context.StudentEnrollments
            .Include(e => e.Course)
            .Where(e => e.StudentId == studentId)
            .OrderBy(e => e.Course.Name)
            .ToListAsync();
    }

    public async Task EnrollAsync(int studentId, int courseId)
    {
        bool exists = await context.StudentEnrollments.AnyAsync(e =>
            e.StudentId == studentId && e.CourseId == courseId);
        if (exists)
            throw new InvalidOperationException("Student is already enrolled in this course.");

        context.StudentEnrollments.Add(new StudentEnrollment
        {
            StudentId = studentId,
            CourseId = courseId,
            Status = EnrollmentStatus.Active
        });
        await context.SaveChangesAsync();
    }

    public async Task EnrollMultipleAsync(List<int> studentIds, int courseId)
    {
        var existing = await context.StudentEnrollments
            .Where(e => e.CourseId == courseId && studentIds.Contains(e.StudentId))
            .Select(e => e.StudentId)
            .ToListAsync();

        var newEnrollments = studentIds
            .Where(sid => !existing.Contains(sid))
            .Select(sid => new StudentEnrollment
            {
                StudentId = sid,
                CourseId = courseId,
                Status = EnrollmentStatus.Active
            });

        context.StudentEnrollments.AddRange(newEnrollments);
        await context.SaveChangesAsync();
    }

    public async Task DropAsync(int enrollmentId)
    {
        var enrollment = await context.StudentEnrollments.FindAsync(enrollmentId)
            ?? throw new KeyNotFoundException("Enrollment not found.");

        enrollment.Status = EnrollmentStatus.Dropped;
        await context.SaveChangesAsync();
    }

    public async Task RemoveAsync(int enrollmentId)
    {
        var enrollment = await context.StudentEnrollments.FindAsync(enrollmentId)
            ?? throw new KeyNotFoundException("Enrollment not found.");

        context.StudentEnrollments.Remove(enrollment);
        await context.SaveChangesAsync();
    }

    public async Task<int> GetActiveCountAsync()
    {
        return await context.StudentEnrollments.CountAsync(e => e.Status == EnrollmentStatus.Active);
    }
}
