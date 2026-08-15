using Lyceum.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Lyceum.Services;

public class ReportService(GradeService gradeService, AttendanceService attendanceService, StudentService studentService, LyceumDbContext context)
{
    public async Task<byte[]> GenerateStudentGradeReportAsync(int studentId)
    {
        var student = await studentService.GetByIdAsync(studentId)
            ?? throw new KeyNotFoundException("Student not found.");
        var grades = await gradeService.GetByStudentAsync(studentId);
        var gpa = await gradeService.CalculateGPAAsync(studentId);

        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text("Lyceum University").FontSize(18).Bold().FontColor(Colors.Blue.Darken3);
                    col.Item().Text("Student Grade Report").FontSize(14).SemiBold();
                    col.Item().PaddingTop(5).Text($"Student: {student.User.FullName} ({student.StudentCode})").FontSize(11);
                    col.Item().Text($"Generated: {DateTime.Now:MMMM dd, yyyy}").FontSize(9).FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingVertical(15).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Course").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Assignment").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Midterm").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Final").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Total").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Grade").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("GPA").FontColor(Colors.White).Bold();
                        });

                        foreach (var grade in grades)
                        {
                            var bg = grades.IndexOf(grade) % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                            table.Cell().Background(bg).Padding(5).Text(grade.Course.Name);
                            table.Cell().Background(bg).Padding(5).Text($"{grade.AssignmentMarks:0.0}");
                            table.Cell().Background(bg).Padding(5).Text($"{grade.MidtermMarks:0.0}");
                            table.Cell().Background(bg).Padding(5).Text($"{grade.FinalMarks:0.0}");
                            table.Cell().Background(bg).Padding(5).Text($"{grade.TotalMarks:0.0}");
                            table.Cell().Background(bg).Padding(5).Text(grade.LetterGrade ?? "-");
                            table.Cell().Background(bg).Padding(5).Text($"{grade.GradePoints:0.0}");
                        }
                    });

                    col.Item().PaddingTop(20).Row(row =>
                    {
                        row.RelativeItem().Text($"Overall GPA: {gpa:0.00}").FontSize(12).Bold();
                        row.RelativeItem().AlignRight().Text($"Total Courses: {grades.Count}").FontSize(11);
                    });
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateStudentAttendanceReportAsync(int studentId)
    {
        var student = await studentService.GetByIdAsync(studentId)
            ?? throw new KeyNotFoundException("Student not found.");
        var records = await attendanceService.GetStudentAttendanceAsync(studentId);
        var overallPct = await attendanceService.GetOverallAttendancePercentageAsync(studentId);

        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text("Lyceum University").FontSize(18).Bold().FontColor(Colors.Blue.Darken3);
                    col.Item().Text("Student Attendance Report").FontSize(14).SemiBold();
                    col.Item().PaddingTop(5).Text($"Student: {student.User.FullName} ({student.StudentCode})").FontSize(11);
                    col.Item().Text($"Overall Attendance: {overallPct}%").FontSize(11).Bold();
                    col.Item().Text($"Generated: {DateTime.Now:MMMM dd, yyyy}").FontSize(9).FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingVertical(15).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Date").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Course").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Status").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Notes").FontColor(Colors.White).Bold();
                        });

                        foreach (var record in records)
                        {
                            var bg = records.IndexOf(record) % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                            table.Cell().Background(bg).Padding(5).Text(record.Session.SessionDate.ToString("MMM dd, yyyy"));
                            table.Cell().Background(bg).Padding(5).Text(record.Session.Course.Name);
                            table.Cell().Background(bg).Padding(5).Text(record.Status.ToString());
                            table.Cell().Background(bg).Padding(5).Text(record.Notes ?? "-");
                        }
                    });
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateClassResultReportAsync(int courseId)
    {
        var grades = await gradeService.GetByCourseAsync(courseId);
        if (grades.Count == 0) throw new InvalidOperationException("No grades found for this course.");

        var courseName = grades.First().Course.Name;

        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text("Lyceum University").FontSize(18).Bold().FontColor(Colors.Blue.Darken3);
                    col.Item().Text($"Class Result Report — {courseName}").FontSize(14).SemiBold();
                    col.Item().Text($"Generated: {DateTime.Now:MMMM dd, yyyy}").FontSize(9).FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingVertical(15).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Student").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Assignment").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Midterm").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Final").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Total").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Grade").FontColor(Colors.White).Bold();
                        });

                        foreach (var grade in grades)
                        {
                            var bg = grades.IndexOf(grade) % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                            table.Cell().Background(bg).Padding(5).Text(grade.Student.User.FullName);
                            table.Cell().Background(bg).Padding(5).Text($"{grade.AssignmentMarks:0.0}");
                            table.Cell().Background(bg).Padding(5).Text($"{grade.MidtermMarks:0.0}");
                            table.Cell().Background(bg).Padding(5).Text($"{grade.FinalMarks:0.0}");
                            table.Cell().Background(bg).Padding(5).Text($"{grade.TotalMarks:0.0}");
                            table.Cell().Background(bg).Padding(5).Text(grade.LetterGrade ?? "-");
                        }
                    });

                    col.Item().PaddingTop(15).Text($"Total Students: {grades.Count}").FontSize(11).Bold();
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateClassAttendanceReportAsync(int courseId)
    {
        var course = await context.Courses.FindAsync(courseId)
            ?? throw new KeyNotFoundException("Course not found.");

        var sessions = await attendanceService.GetSessionsByCourseAsync(courseId);

        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text("Lyceum University").FontSize(18).Bold().FontColor(Colors.Blue.Darken3);
                    col.Item().Text($"Class Attendance Report — {course.Name} ({course.CourseCode})").FontSize(14).SemiBold();
                    col.Item().Text($"Generated: {DateTime.Now:MMMM dd, yyyy}").FontSize(9).FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingVertical(15).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1.5f);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Student").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Present").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Absent").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Late").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Attendance %").FontColor(Colors.White).Bold();
                        });

                        // Flatten all records to compute statistics per student
                        var allRecords = sessions.SelectMany(s => s.Records).ToList();
                        var studentGroups = allRecords.GroupBy(r => new { r.StudentId, Name = r.Student?.User?.FullName ?? "Unknown" }).ToList();

                        foreach (var group in studentGroups)
                        {
                            var present = group.Count(r => r.Status == AttendanceStatus.Present);
                            var absent = group.Count(r => r.Status == AttendanceStatus.Absent);
                            var late = group.Count(r => r.Status == AttendanceStatus.Late);
                            var total = group.Count();
                            var pct = total > 0 ? Math.Round((double)(present + late) / total * 100, 1) : 100.0;

                            var bg = studentGroups.IndexOf(group) % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                            table.Cell().Background(bg).Padding(5).Text(group.Key.Name);
                            table.Cell().Background(bg).Padding(5).Text(present.ToString());
                            table.Cell().Background(bg).Padding(5).Text(absent.ToString());
                            table.Cell().Background(bg).Padding(5).Text(late.ToString());
                            table.Cell().Background(bg).Padding(5).Text($"{pct}%");
                        }
                    });

                    col.Item().PaddingTop(15).Text($"Total Sessions: {sessions.Count}").FontSize(11).Bold();
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }
}
