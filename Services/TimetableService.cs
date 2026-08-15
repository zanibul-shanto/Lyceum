using Lyceum.Models;
using Microsoft.EntityFrameworkCore;

namespace Lyceum.Services;

public class TimetableService(LyceumDbContext context)
{
    public async Task<List<Timetable>> GetAllAsync(string? semester = null, string? academicYear = null)
    {
        var query = context.Timetables
            .Include(t => t.Course)
            .Include(t => t.Teacher)
                .ThenInclude(t => t.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(semester))
            query = query.Where(t => t.Semester == semester);
        if (!string.IsNullOrWhiteSpace(academicYear))
            query = query.Where(t => t.AcademicYear == academicYear);

        return await query.OrderBy(t => t.DayOfWeek).ThenBy(t => t.StartTime).ToListAsync();
    }

    public async Task<List<Timetable>> GetByTeacherAsync(int teacherId)
    {
        return await context.Timetables
            .Include(t => t.Course)
            .Include(t => t.Teacher).ThenInclude(t => t.User)
            .Where(t => t.TeacherId == teacherId)
            .OrderBy(t => t.DayOfWeek).ThenBy(t => t.StartTime)
            .ToListAsync();
    }

    public async Task<List<Timetable>> GetByStudentAsync(int studentId)
    {
        var enrolledCourseIds = await context.StudentEnrollments
            .Where(e => e.StudentId == studentId && e.Status == EnrollmentStatus.Active)
            .Select(e => e.CourseId)
            .ToListAsync();

        return await context.Timetables
            .Include(t => t.Course)
            .Include(t => t.Teacher).ThenInclude(t => t.User)
            .Where(t => enrolledCourseIds.Contains(t.CourseId))
            .OrderBy(t => t.DayOfWeek).ThenBy(t => t.StartTime)
            .ToListAsync();
    }

    public async Task CreateAsync(Timetable entry)
    {
        var conflict = await CheckConflictAsync(entry);
        if (conflict != null)
            throw new InvalidOperationException($"Time conflict with {conflict.Course.Name} on {conflict.DayOfWeek} at {conflict.StartTime}-{conflict.EndTime}");

        context.Timetables.Add(entry);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Timetable entry)
    {
        var conflict = await CheckConflictAsync(entry);
        if (conflict != null)
            throw new InvalidOperationException($"Time conflict with {conflict.Course.Name} on {conflict.DayOfWeek} at {conflict.StartTime}-{conflict.EndTime}");

        var existing = await context.Timetables.FindAsync(entry.Id)
            ?? throw new KeyNotFoundException("Timetable entry not found.");

        existing.CourseId = entry.CourseId;
        existing.TeacherId = entry.TeacherId;
        existing.DayOfWeek = entry.DayOfWeek;
        existing.StartTime = entry.StartTime;
        existing.EndTime = entry.EndTime;
        existing.RoomNumber = entry.RoomNumber;
        existing.Semester = entry.Semester;
        existing.AcademicYear = entry.AcademicYear;

        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entry = await context.Timetables.FindAsync(id)
            ?? throw new KeyNotFoundException("Timetable entry not found.");

        context.Timetables.Remove(entry);
        await context.SaveChangesAsync();
    }

    private async Task<Timetable?> CheckConflictAsync(Timetable entry)
    {
        return await context.Timetables
            .Include(t => t.Course)
            .Where(t => t.Id != entry.Id &&
                        t.TeacherId == entry.TeacherId &&
                        t.DayOfWeek == entry.DayOfWeek &&
                        t.Semester == entry.Semester &&
                        t.AcademicYear == entry.AcademicYear &&
                        t.StartTime < entry.EndTime &&
                        t.EndTime > entry.StartTime)
            .FirstOrDefaultAsync();
    }
}
