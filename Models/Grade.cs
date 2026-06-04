using System.ComponentModel.DataAnnotations;

namespace Lyceum.Models;

public class Grade
{
    public int Id { get; set; }

    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public int TeacherId { get; set; }
    public Teacher Teacher { get; set; } = null!;

    [Range(0, 100)]
    public double AssignmentMarks { get; set; }

    [Range(0, 100)]
    public double MidtermMarks { get; set; }

    [Range(0, 100)]
    public double FinalMarks { get; set; }

    public double TotalMarks { get; set; }

    [StringLength(5)]
    public string? LetterGrade { get; set; }

    public double GradePoints { get; set; }

    public DateTime EnteredAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
