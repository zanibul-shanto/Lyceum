using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Lyceum.Models;

public enum UserRole
{
    Student = 0,
    Teacher = 1,
    Admin = 2
}

public class User : IdentityUser<int>
{
    [NotMapped]
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    public string Username
    {
        get => UserName ?? string.Empty;
        set => UserName = value;
    }

    [Required(ErrorMessage = "Full Name is required")]
    [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; } = UserRole.Student;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Student Specific Fields
    public string? StudentId { get; set; }
    public string? GradeLevel { get; set; }
    public double? GPA { get; set; }
    public double? AttendanceRate { get; set; }

    // Teacher Specific Fields
    public string? TeacherId { get; set; }
    public string? Department { get; set; }
    public string? Bio { get; set; }
    public string? OfficeRoom { get; set; }
}
