using Lyceum.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Lyceum.Services;

public class LyceumDbContext(DbContextOptions<LyceumDbContext> options) : IdentityDbContext<User, IdentityRole<int>, int>(options)
{
    public DbSet<Course> Courses { get; set; } = default!;
    public DbSet<CourseTeacher> CourseTeachers { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CourseTeacher>()
            .HasKey(ct => new { ct.CourseId, ct.TeacherId });

        modelBuilder.Entity<CourseTeacher>()
            .HasOne(ct => ct.Course)
            .WithMany(c => c.CourseTeachers)
            .HasForeignKey(ct => ct.CourseId);

        modelBuilder.Entity<CourseTeacher>()
            .HasOne(ct => ct.Teacher)
            .WithMany()
            .HasForeignKey(ct => ct.TeacherId);

        modelBuilder.Entity<Course>()
            .HasIndex(c => c.CourseCode)
            .IsUnique();
    }
}
