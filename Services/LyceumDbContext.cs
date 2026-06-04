using Lyceum.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Lyceum.Services;

public class LyceumDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public LyceumDbContext(DbContextOptions<LyceumDbContext> options) : base(options)
    {
    }

    public DbSet<Student> Students { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<CourseTeacher> CourseTeachers { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<StudentEnrollment> StudentEnrollments { get; set; }
    public DbSet<AttendanceSession> AttendanceSessions { get; set; }
    public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
    public DbSet<Grade> Grades { get; set; }
    public DbSet<Timetable> Timetables { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<SystemSetting> SystemSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User -> Student (1:1)
        modelBuilder.Entity<Student>()
            .HasOne(s => s.User)
            .WithOne(u => u.Student)
            .HasForeignKey<Student>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Student>()
            .HasIndex(s => s.UserId).IsUnique();

        modelBuilder.Entity<Student>()
            .HasIndex(s => s.StudentCode).IsUnique();

        // User -> Teacher (1:1)
        modelBuilder.Entity<Teacher>()
            .HasOne(t => t.User)
            .WithOne(u => u.Teacher)
            .HasForeignKey<Teacher>(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Teacher>()
            .HasIndex(t => t.UserId).IsUnique();

        modelBuilder.Entity<Teacher>()
            .HasIndex(t => t.TeacherCode).IsUnique();

        // CourseTeacher (M:N composite key)
        modelBuilder.Entity<CourseTeacher>()
            .HasKey(ct => new { ct.CourseId, ct.TeacherId });

        modelBuilder.Entity<CourseTeacher>()
            .HasOne(ct => ct.Course)
            .WithMany(c => c.CourseTeachers)
            .HasForeignKey(ct => ct.CourseId);

        modelBuilder.Entity<CourseTeacher>()
            .HasOne(ct => ct.Teacher)
            .WithMany(t => t.CourseTeachers)
            .HasForeignKey(ct => ct.TeacherId);

        // Course unique index
        modelBuilder.Entity<Course>()
            .HasIndex(c => c.CourseCode)
            .IsUnique();

        // Subject
        modelBuilder.Entity<Subject>()
            .HasOne(s => s.Course)
            .WithMany(c => c.Subjects)
            .HasForeignKey(s => s.CourseId);

        modelBuilder.Entity<Subject>()
            .HasIndex(s => new { s.CourseId, s.SubjectCode })
            .IsUnique();

        // StudentEnrollment
        modelBuilder.Entity<StudentEnrollment>()
            .HasOne(e => e.Student)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<StudentEnrollment>()
            .HasOne(e => e.Course)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CourseId);

        modelBuilder.Entity<StudentEnrollment>()
            .HasIndex(e => new { e.StudentId, e.CourseId })
            .IsUnique();

        // AttendanceSession
        modelBuilder.Entity<AttendanceSession>()
            .HasOne(a => a.Course)
            .WithMany(c => c.AttendanceSessions)
            .HasForeignKey(a => a.CourseId);

        modelBuilder.Entity<AttendanceSession>()
            .HasOne(a => a.Teacher)
            .WithMany(t => t.AttendanceSessions)
            .HasForeignKey(a => a.TeacherId)
            .OnDelete(DeleteBehavior.NoAction);

        // AttendanceRecord
        modelBuilder.Entity<AttendanceRecord>()
            .HasOne(r => r.Session)
            .WithMany(s => s.Records)
            .HasForeignKey(r => r.SessionId);

        modelBuilder.Entity<AttendanceRecord>()
            .HasOne(r => r.Student)
            .WithMany(s => s.AttendanceRecords)
            .HasForeignKey(r => r.StudentId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<AttendanceRecord>()
            .HasIndex(r => new { r.SessionId, r.StudentId })
            .IsUnique();

        // Grade
        modelBuilder.Entity<Grade>()
            .HasOne(g => g.Student)
            .WithMany(s => s.Grades)
            .HasForeignKey(g => g.StudentId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Grade>()
            .HasOne(g => g.Course)
            .WithMany(c => c.Grades)
            .HasForeignKey(g => g.CourseId);

        modelBuilder.Entity<Grade>()
            .HasOne(g => g.Teacher)
            .WithMany(t => t.GradesEntered)
            .HasForeignKey(g => g.TeacherId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Grade>()
            .HasIndex(g => new { g.StudentId, g.CourseId })
            .IsUnique();

        // Timetable
        modelBuilder.Entity<Timetable>()
            .HasOne(t => t.Course)
            .WithMany(c => c.Timetables)
            .HasForeignKey(t => t.CourseId);

        modelBuilder.Entity<Timetable>()
            .HasOne(t => t.Teacher)
            .WithMany(t => t.Timetables)
            .HasForeignKey(t => t.TeacherId)
            .OnDelete(DeleteBehavior.NoAction);

        // AuditLog
        modelBuilder.Entity<AuditLog>()
            .HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // SystemSetting
        modelBuilder.Entity<SystemSetting>()
            .HasIndex(s => s.SettingKey)
            .IsUnique();

        modelBuilder.Entity<SystemSetting>()
            .HasOne(s => s.UpdatedByUser)
            .WithMany()
            .HasForeignKey(s => s.UpdatedBy)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
