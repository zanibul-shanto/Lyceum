using System.ComponentModel.DataAnnotations;

namespace Lyceum.Models;

public enum EnrollmentStatus
{
    Active = 0,
    Dropped = 1
}

public class StudentEnrollment
{
    public int Id { get; set; }

    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;
}
