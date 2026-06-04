using System.ComponentModel.DataAnnotations;

namespace Lyceum.Models;

public class Teacher
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    [Required, StringLength(20)]
    public string TeacherCode { get; set; } = "";

    [StringLength(100)]
    public string? Department { get; set; }

    [StringLength(100)]
    public string? Qualification { get; set; }

    public DateTime JoiningDate { get; set; } = DateTime.UtcNow;

    [StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(500)]
    public string? PhotoUrl { get; set; }

    [StringLength(500)]
    public string? Bio { get; set; }

    [StringLength(50)]
    public string? OfficeRoom { get; set; }

    // Navigation
    public ICollection<CourseTeacher> CourseTeachers { get; set; } = [];
    public ICollection<AttendanceSession> AttendanceSessions { get; set; } = [];
    public ICollection<Grade> GradesEntered { get; set; } = [];
    public ICollection<Timetable> Timetables { get; set; } = [];
}
