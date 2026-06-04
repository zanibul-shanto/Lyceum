using Lyceum.Models;
using Microsoft.EntityFrameworkCore;

namespace Lyceum.Services;

public class GradeService(LyceumDbContext context)
{
    public async Task<List<Grade>> GetByCourseAsync(int courseId)
    {
        return await context.Grades
            .Include(g => g.Student)
                .ThenInclude(s => s.User)
            .Include(g => g.Teacher)
                .ThenInclude(t => t.User)
            .Where(g => g.CourseId == courseId)
            .OrderBy(g => g.Student.User.FullName)
            .ToListAsync();
    }

    public async Task<List<Grade>> GetByStudentAsync(int studentId)
    {
        return await context.Grades
            .Include(g => g.Course)
            .Include(g => g.Teacher)
                .ThenInclude(t => t.User)
            .Where(g => g.StudentId == studentId)
            .OrderBy(g => g.Course.Name)
            .ToListAsync();
    }

    public async Task<Grade?> GetAsync(int studentId, int courseId)
    {
        return await context.Grades
            .Include(g => g.Student).ThenInclude(s => s.User)
            .Include(g => g.Course)
            .FirstOrDefaultAsync(g => g.StudentId == studentId && g.CourseId == courseId);
    }

    public async Task EnterGradeAsync(Grade grade)
    {
        ComputeGrade(grade);

        var existing = await context.Grades
            .FirstOrDefaultAsync(g => g.StudentId == grade.StudentId && g.CourseId == grade.CourseId);

        if (existing != null)
        {
            existing.AssignmentMarks = grade.AssignmentMarks;
            existing.MidtermMarks = grade.MidtermMarks;
            existing.FinalMarks = grade.FinalMarks;
            existing.TotalMarks = grade.TotalMarks;
            existing.LetterGrade = grade.LetterGrade;
            existing.GradePoints = grade.GradePoints;
            existing.TeacherId = grade.TeacherId;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            grade.EnteredAt = DateTime.UtcNow;
            context.Grades.Add(grade);
        }

        await context.SaveChangesAsync();
    }

    public async Task<double> CalculateGPAAsync(int studentId)
    {
        var grades = await context.Grades
            .Include(g => g.Course)
            .Where(g => g.StudentId == studentId)
            .ToListAsync();

        if (grades.Count == 0) return 0;

        var totalCredits = grades.Sum(g => g.Course.Credits);
        if (totalCredits == 0) return 0;

        var weightedSum = grades.Sum(g => g.GradePoints * g.Course.Credits);
        return Math.Round(weightedSum / totalCredits, 2);
    }

    public async Task<Dictionary<string, int>> GetGradeDistributionAsync(int courseId)
    {
        return await context.Grades
            .Where(g => g.CourseId == courseId && g.LetterGrade != null)
            .GroupBy(g => g.LetterGrade!)
            .Select(g => new { Grade = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Grade, g => g.Count);
    }

    private static void ComputeGrade(Grade grade)
    {
        grade.TotalMarks = Math.Round((grade.AssignmentMarks * 0.3) + (grade.MidtermMarks * 0.3) + (grade.FinalMarks * 0.4), 1);

        (grade.LetterGrade, grade.GradePoints) = grade.TotalMarks switch
        {
            >= 90 => ("A", 4.0),
            >= 80 => ("B", 3.0),
            >= 70 => ("C", 2.0),
            >= 60 => ("D", 1.0),
            _ => ("F", 0.0)
        };
    }
}
