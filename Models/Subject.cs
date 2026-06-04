using System.ComponentModel.DataAnnotations;

namespace Lyceum.Models;

public class Subject
{
    public int Id { get; set; }

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    [Required, StringLength(100)]
    public string SubjectName { get; set; } = "";

    [Required, StringLength(20)]
    public string SubjectCode { get; set; } = "";

    [StringLength(500)]
    public string? Description { get; set; }
}
