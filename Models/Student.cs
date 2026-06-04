using System.ComponentModel.DataAnnotations;

namespace Lyceum.Models;

public class Student
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    [Required, StringLength(20)]
    public string StudentCode { get; set; } = "";

    public DateTime? DateOfBirth { get; set; }

    [StringLength(10)]
    public string? Gender { get; set; }

    [StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(300)]
    public string? Address { get; set; }

    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

    [StringLength(500)]
    public string? PhotoUrl { get; set; }

    public string? FaceDescriptor { get; set; }

    // Navigation
    public ICollection<StudentEnrollment> Enrollments { get; set; } = [];
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = [];
    public ICollection<Grade> Grades { get; set; } = [];
}
