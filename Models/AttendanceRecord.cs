namespace Lyceum.Models;

public enum AttendanceStatus
{
    Present = 0,
    Absent = 1,
    Late = 2
}

public class AttendanceRecord
{
    public int Id { get; set; }

    public int SessionId { get; set; }
    public AttendanceSession Session { get; set; } = null!;

    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

    public DateTime MarkedAt { get; set; } = DateTime.UtcNow;

    public bool OverriddenByTeacher { get; set; }

    public string? Notes { get; set; }
}
