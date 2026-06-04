using Lyceum.Models;
using Microsoft.EntityFrameworkCore;

namespace Lyceum.Services;

public class AttendanceService(LyceumDbContext context)
{
    public async Task<AttendanceSession> CreateSessionAsync(int courseId, int teacherId, DateTime sessionDate, TimeOnly? startTime, TimeOnly? endTime)
    {
        var session = new AttendanceSession
        {
            CourseId = courseId,
            TeacherId = teacherId,
            SessionDate = sessionDate,
            SessionType = SessionType.Manual,
            StartTime = startTime,
            EndTime = endTime
        };
        context.AttendanceSessions.Add(session);
        await context.SaveChangesAsync();
        return session;
    }

    public async Task<List<AttendanceSession>> GetSessionsByCourseAsync(int courseId)
    {
        return await context.AttendanceSessions
            .Include(s => s.Teacher)
                .ThenInclude(t => t.User)
            .Include(s => s.Records)
                .ThenInclude(r => r.Student)
                    .ThenInclude(s => s.User)
            .Where(s => s.CourseId == courseId)
            .OrderByDescending(s => s.SessionDate)
            .ToListAsync();
    }

    public async Task<AttendanceSession?> GetSessionAsync(int sessionId)
    {
        return await context.AttendanceSessions
            .Include(s => s.Course)
            .Include(s => s.Teacher).ThenInclude(t => t.User)
            .Include(s => s.Records)
                .ThenInclude(r => r.Student)
                    .ThenInclude(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == sessionId);
    }

    public async Task MarkAttendanceAsync(int sessionId, List<(int StudentId, AttendanceStatus Status)> records)
    {
        var existing = await context.AttendanceRecords
            .Where(r => r.SessionId == sessionId)
            .ToListAsync();
        context.AttendanceRecords.RemoveRange(existing);

        var newRecords = records.Select(r => new AttendanceRecord
        {
            SessionId = sessionId,
            StudentId = r.StudentId,
            Status = r.Status,
            MarkedAt = DateTime.UtcNow
        });
        context.AttendanceRecords.AddRange(newRecords);
        await context.SaveChangesAsync();
    }

    public async Task UpdateRecordAsync(int recordId, AttendanceStatus status, string? notes)
    {
        var record = await context.AttendanceRecords.FindAsync(recordId)
            ?? throw new KeyNotFoundException("Attendance record not found.");

        record.Status = status;
        record.Notes = notes;
        record.OverriddenByTeacher = true;
        record.MarkedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    public async Task<List<AttendanceRecord>> GetStudentAttendanceAsync(int studentId, int? courseId = null)
    {
        var query = context.AttendanceRecords
            .Include(r => r.Session)
                .ThenInclude(s => s.Course)
            .Where(r => r.StudentId == studentId);

        if (courseId.HasValue)
            query = query.Where(r => r.Session.CourseId == courseId.Value);

        return await query.OrderByDescending(r => r.Session.SessionDate).ToListAsync();
    }

    public async Task<Dictionary<int, double>> GetAttendancePercentageByCoursesAsync(int studentId)
    {
        var records = await context.AttendanceRecords
            .Include(r => r.Session)
            .Where(r => r.StudentId == studentId)
            .ToListAsync();

        return records
            .GroupBy(r => r.Session.CourseId)
            .ToDictionary(
                g => g.Key,
                g => g.Count() == 0 ? 0 : Math.Round((double)g.Count(r => r.Status != AttendanceStatus.Absent) / g.Count() * 100, 1)
            );
    }

    public async Task<double> GetOverallAttendancePercentageAsync(int studentId)
    {
        var records = await context.AttendanceRecords
            .Where(r => r.StudentId == studentId)
            .ToListAsync();

        if (records.Count == 0) return 100;
        return Math.Round((double)records.Count(r => r.Status != AttendanceStatus.Absent) / records.Count * 100, 1);
    }

    public async Task DeleteSessionAsync(int sessionId)
    {
        var session = await context.AttendanceSessions
            .Include(s => s.Records)
            .FirstOrDefaultAsync(s => s.Id == sessionId)
            ?? throw new KeyNotFoundException("Session not found.");

        context.AttendanceRecords.RemoveRange(session.Records);
        context.AttendanceSessions.Remove(session);
        await context.SaveChangesAsync();
    }
}
