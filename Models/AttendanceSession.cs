using System.ComponentModel.DataAnnotations;

namespace Lyceum.Models;

public enum SessionType
{
    Manual = 0,
    AI = 1
}

public class AttendanceSession
{
    public int Id { get; set; }

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public int TeacherId { get; set; }
    public Teacher Teacher { get; set; } = null!;

    public DateTime SessionDate { get; set; }

    public SessionType SessionType { get; set; } = SessionType.Manual;

    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }

    // Navigation
    public ICollection<AttendanceRecord> Records { get; set; } = [];
}
