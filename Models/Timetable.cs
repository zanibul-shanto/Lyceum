using System.ComponentModel.DataAnnotations;

namespace Lyceum.Models;

public enum DayOfWeekEnum
{
    Monday = 0,
    Tuesday = 1,
    Wednesday = 2,
    Thursday = 3,
    Friday = 4,
    Saturday = 5,
    Sunday = 6
}

public class Timetable
{
    public int Id { get; set; }

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public int TeacherId { get; set; }
    public Teacher Teacher { get; set; } = null!;

    public DayOfWeekEnum DayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    [StringLength(50)]
    public string? RoomNumber { get; set; }

    [StringLength(20)]
    public string? Semester { get; set; }

    [StringLength(20)]
    public string? AcademicYear { get; set; }
}
