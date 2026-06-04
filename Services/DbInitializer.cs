using Lyceum.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Lyceum.Services;

public static class DbInitializer
{
    public static async Task InitializeAsync(LyceumDbContext context, UserManager<User> userManager)
    {
        await context.Database.MigrateAsync();

        // Seed users, students, teachers, courses, enrollments, timetable, settings
        if (!await userManager.Users.AnyAsync())
        {
            // --- Admin ---
            var admin = new User
            {
                UserName = "admin",
                FullName = "System Administrator",
                Email = "admin@lyceum.edu",
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-3)
            };
            await userManager.CreateAsync(admin, "Admin@123");

            // --- Teachers ---
            var teacherUser1 = new User
            {
                UserName = "teacher",
                FullName = "Prof. Diana Vance",
                Email = "diana.vance@lyceum.edu",
                Role = UserRole.Teacher,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-2)
            };
            await userManager.CreateAsync(teacherUser1, "Teacher@123");

            var teacher1 = new Teacher
            {
                UserId = teacherUser1.Id,
                TeacherCode = "TCH-2026-088",
                Department = "Computer Science & Engineering",
                Qualification = "Ph.D. Computer Science",
                Bio = "Diana is an Associate Professor of Computer Science with 12+ years of teaching experience. She specializes in Software Engineering and Database Design.",
                OfficeRoom = "Science Hall, Room 402",
                JoiningDate = DateTime.UtcNow.AddYears(-5)
            };
            context.Teachers.Add(teacher1);

            var teacherUser2 = new User
            {
                UserName = "msmith",
                FullName = "Dr. Michael Smith",
                Email = "michael.smith@lyceum.edu",
                Role = UserRole.Teacher,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-5)
            };
            await userManager.CreateAsync(teacherUser2, "Teacher@123");

            var teacher2 = new Teacher
            {
                UserId = teacherUser2.Id,
                TeacherCode = "TCH-2026-015",
                Department = "Mathematics",
                Qualification = "Ph.D. Mathematics",
                Bio = "Michael's research focuses on abstract algebra and complex systems. He teaches Calculus III and Linear Algebra.",
                OfficeRoom = "Newton Wing, Room 102",
                JoiningDate = DateTime.UtcNow.AddYears(-8)
            };
            context.Teachers.Add(teacher2);

            // --- Students ---
            var studentUser1 = new User
            {
                UserName = "student",
                FullName = "Alex Chen",
                Email = "alex.chen@lyceum.edu",
                Role = UserRole.Student,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            };
            await userManager.CreateAsync(studentUser1, "Student@123");

            var student1 = new Student
            {
                UserId = studentUser1.Id,
                StudentCode = "STU-2026-1049",
                DateOfBirth = new DateTime(2003, 5, 15),
                Gender = "Male",
                Phone = "555-0101",
                Address = "123 Campus Drive, University City",
                EnrollmentDate = DateTime.UtcNow.AddMonths(-6)
            };
            context.Students.Add(student1);

            var studentUser2 = new User
            {
                UserName = "jdoe",
                FullName = "John Doe",
                Email = "john.doe@lyceum.edu",
                Role = UserRole.Student,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            };
            await userManager.CreateAsync(studentUser2, "Student@123");

            var student2 = new Student
            {
                UserId = studentUser2.Id,
                StudentCode = "STU-2026-1050",
                DateOfBirth = new DateTime(2004, 2, 20),
                Gender = "Male",
                Phone = "555-0102",
                Address = "456 Elm Street, University City",
                EnrollmentDate = DateTime.UtcNow.AddMonths(-4)
            };
            context.Students.Add(student2);

            var studentUser3 = new User
            {
                UserName = "jsmith",
                FullName = "Jane Smith",
                Email = "jane.smith@lyceum.edu",
                Role = UserRole.Student,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            };
            await userManager.CreateAsync(studentUser3, "Student@123");

            var student3 = new Student
            {
                UserId = studentUser3.Id,
                StudentCode = "STU-2026-1051",
                DateOfBirth = new DateTime(2003, 11, 8),
                Gender = "Female",
                Phone = "555-0103",
                Address = "789 Oak Avenue, University City",
                EnrollmentDate = DateTime.UtcNow.AddMonths(-5)
            };
            context.Students.Add(student3);

            await context.SaveChangesAsync();

            // --- Courses ---
            var course1 = new Course
            {
                Name = "Introduction to Computer Science",
                CourseCode = "CS101",
                Description = "Fundamentals of programming, algorithms, and data structures.",
                Credits = 4,
                MaxCapacity = 40,
                Semester = "Fall",
                AcademicYear = "2026-2027",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-3)
            };

            var course2 = new Course
            {
                Name = "Calculus III",
                CourseCode = "MATH301",
                Description = "Multivariable calculus, vector fields, and surface integrals.",
                Credits = 3,
                MaxCapacity = 35,
                Semester = "Fall",
                AcademicYear = "2026-2027",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-3)
            };

            var course3 = new Course
            {
                Name = "Database Systems",
                CourseCode = "CS301",
                Description = "Relational databases, SQL, normalization, and transaction management.",
                Credits = 3,
                MaxCapacity = 30,
                Semester = "Fall",
                AcademicYear = "2026-2027",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-2)
            };

            context.Courses.AddRange(course1, course2, course3);
            await context.SaveChangesAsync();

            // --- Subjects ---
            context.Subjects.AddRange(
                new Subject { CourseId = course1.Id, SubjectName = "Programming Fundamentals", SubjectCode = "CS101-A" },
                new Subject { CourseId = course1.Id, SubjectName = "Data Structures", SubjectCode = "CS101-B" },
                new Subject { CourseId = course2.Id, SubjectName = "Vector Calculus", SubjectCode = "MATH301-A" },
                new Subject { CourseId = course3.Id, SubjectName = "SQL & Relational Design", SubjectCode = "CS301-A" },
                new Subject { CourseId = course3.Id, SubjectName = "Transaction Processing", SubjectCode = "CS301-B" }
            );

            // --- Course-Teacher Assignments ---
            context.CourseTeachers.AddRange(
                new CourseTeacher { CourseId = course1.Id, TeacherId = teacher1.Id },
                new CourseTeacher { CourseId = course2.Id, TeacherId = teacher2.Id },
                new CourseTeacher { CourseId = course3.Id, TeacherId = teacher1.Id }
            );

            // --- Student Enrollments ---
            context.StudentEnrollments.AddRange(
                new StudentEnrollment { StudentId = student1.Id, CourseId = course1.Id },
                new StudentEnrollment { StudentId = student1.Id, CourseId = course2.Id },
                new StudentEnrollment { StudentId = student1.Id, CourseId = course3.Id },
                new StudentEnrollment { StudentId = student2.Id, CourseId = course1.Id },
                new StudentEnrollment { StudentId = student2.Id, CourseId = course2.Id },
                new StudentEnrollment { StudentId = student3.Id, CourseId = course1.Id },
                new StudentEnrollment { StudentId = student3.Id, CourseId = course3.Id }
            );

            // --- Timetable ---
            context.Timetables.AddRange(
                new Timetable { CourseId = course1.Id, TeacherId = teacher1.Id, DayOfWeek = DayOfWeekEnum.Monday, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 30), RoomNumber = "Science Hall 101", Semester = "Fall", AcademicYear = "2026-2027" },
                new Timetable { CourseId = course1.Id, TeacherId = teacher1.Id, DayOfWeek = DayOfWeekEnum.Wednesday, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 30), RoomNumber = "Science Hall 101", Semester = "Fall", AcademicYear = "2026-2027" },
                new Timetable { CourseId = course2.Id, TeacherId = teacher2.Id, DayOfWeek = DayOfWeekEnum.Tuesday, StartTime = new TimeOnly(11, 0), EndTime = new TimeOnly(12, 30), RoomNumber = "Newton Wing 201", Semester = "Fall", AcademicYear = "2026-2027" },
                new Timetable { CourseId = course2.Id, TeacherId = teacher2.Id, DayOfWeek = DayOfWeekEnum.Thursday, StartTime = new TimeOnly(11, 0), EndTime = new TimeOnly(12, 30), RoomNumber = "Newton Wing 201", Semester = "Fall", AcademicYear = "2026-2027" },
                new Timetable { CourseId = course3.Id, TeacherId = teacher1.Id, DayOfWeek = DayOfWeekEnum.Tuesday, StartTime = new TimeOnly(14, 0), EndTime = new TimeOnly(15, 30), RoomNumber = "Science Hall 303", Semester = "Fall", AcademicYear = "2026-2027" },
                new Timetable { CourseId = course3.Id, TeacherId = teacher1.Id, DayOfWeek = DayOfWeekEnum.Friday, StartTime = new TimeOnly(14, 0), EndTime = new TimeOnly(15, 30), RoomNumber = "Science Hall 303", Semester = "Fall", AcademicYear = "2026-2027" }
            );

            // --- Attendance Sessions & Records ---
            var session1 = new AttendanceSession
            {
                CourseId = course1.Id,
                TeacherId = teacher1.Id,
                SessionDate = DateTime.UtcNow.AddDays(-7),
                SessionType = SessionType.Manual,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(10, 30)
            };
            var session2 = new AttendanceSession
            {
                CourseId = course1.Id,
                TeacherId = teacher1.Id,
                SessionDate = DateTime.UtcNow.AddDays(-5),
                SessionType = SessionType.Manual,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(10, 30)
            };
            context.AttendanceSessions.AddRange(session1, session2);
            await context.SaveChangesAsync();

            context.AttendanceRecords.AddRange(
                new AttendanceRecord { SessionId = session1.Id, StudentId = student1.Id, Status = AttendanceStatus.Present },
                new AttendanceRecord { SessionId = session1.Id, StudentId = student2.Id, Status = AttendanceStatus.Present },
                new AttendanceRecord { SessionId = session1.Id, StudentId = student3.Id, Status = AttendanceStatus.Late },
                new AttendanceRecord { SessionId = session2.Id, StudentId = student1.Id, Status = AttendanceStatus.Present },
                new AttendanceRecord { SessionId = session2.Id, StudentId = student2.Id, Status = AttendanceStatus.Absent },
                new AttendanceRecord { SessionId = session2.Id, StudentId = student3.Id, Status = AttendanceStatus.Present }
            );

            // --- Grades ---
            context.Grades.AddRange(
                new Grade { StudentId = student1.Id, CourseId = course1.Id, TeacherId = teacher1.Id, AssignmentMarks = 88, MidtermMarks = 92, FinalMarks = 90, TotalMarks = 90, LetterGrade = "A", GradePoints = 4.0 },
                new Grade { StudentId = student2.Id, CourseId = course1.Id, TeacherId = teacher1.Id, AssignmentMarks = 75, MidtermMarks = 70, FinalMarks = 78, TotalMarks = 74.3, LetterGrade = "C", GradePoints = 2.0 },
                new Grade { StudentId = student1.Id, CourseId = course2.Id, TeacherId = teacher2.Id, AssignmentMarks = 95, MidtermMarks = 88, FinalMarks = 91, TotalMarks = 91.3, LetterGrade = "A", GradePoints = 4.0 },
                new Grade { StudentId = student3.Id, CourseId = course1.Id, TeacherId = teacher1.Id, AssignmentMarks = 82, MidtermMarks = 85, FinalMarks = 80, TotalMarks = 82.3, LetterGrade = "B", GradePoints = 3.0 }
            );

            // --- System Settings ---
            context.SystemSettings.AddRange(
                new SystemSetting { SettingKey = "AcademicYear", SettingValue = "2026-2027" },
                new SystemSetting { SettingKey = "CurrentSemester", SettingValue = "Fall" },
                new SystemSetting { SettingKey = "GradingScale", SettingValue = "{\"A\":{\"min\":90,\"max\":100,\"gpa\":4.0},\"B\":{\"min\":80,\"max\":89,\"gpa\":3.0},\"C\":{\"min\":70,\"max\":79,\"gpa\":2.0},\"D\":{\"min\":60,\"max\":69,\"gpa\":1.0},\"F\":{\"min\":0,\"max\":59,\"gpa\":0.0}}" },
                new SystemSetting { SettingKey = "InstitutionName", SettingValue = "Lyceum University" }
            );

            await context.SaveChangesAsync();
        }
    }
}
