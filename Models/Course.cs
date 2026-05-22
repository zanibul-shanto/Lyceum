using System.ComponentModel.DataAnnotations;

namespace Lyceum.Models;

public class Course
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = "";

    [Required, StringLength(20)]
    public string CourseCode { get; set; } = "";

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? Schedule { get; set; }

    [StringLength(50)]
    public string? RoomNo { get; set; }

    [Range(1, 10)]
    public int Credits { get; set; } = 3;

    [Range(1, 500)]
    public int MaxCapacity { get; set; } = 30;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ICollection<CourseTeacher> CourseTeachers { get; set; } = [];
}

public class CourseTeacher
{
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public int TeacherId { get; set; }
    public User Teacher { get; set; } = null!;
}
