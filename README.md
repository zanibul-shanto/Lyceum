# Lyceum — Student Management System

A web-based academic management platform for students, teachers, and administrators. Supports role-based dashboards, course management, attendance tracking, grade entry, timetable scheduling, and PDF report generation.

---

## Tech Stack

| Layer          | Technology                              |
|----------------|-----------------------------------------|
| Framework      | ASP.NET Core 10.0 — Blazor Server       |
| Language       | C# 12                                   |
| Database       | SQL Server + Entity Framework Core 10   |
| Authentication | ASP.NET Core Identity + Cookie Auth     |
| UI             | Radzen Blazor + Bootstrap 5             |
| Face Recognition | face-api.js 0.22 (TinyFaceDetector + FaceRecognitionNet) |
| PDF Reports    | QuestPDF                                |
| CSV Import     | CsvHelper                               |

---

## Getting Started

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB, SQL Express, or full)

### Run

```bash
git clone <repo-url>
cd Lyceum
dotnet run
```

Opens at **`https://localhost:7081`** (HTTP: `http://localhost:5058`).  
Database migrations and seed data are applied automatically on first launch.

### Database connection

Edit `appsettings.json` to point to your SQL Server instance:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=LyceumDb;Integrated Security=True;TrustServerCertificate=True"
}
```

---

## Test Accounts

| Role    | Username  | Password      |
|---------|-----------|---------------|
| Admin   | `admin`   | `Admin@123`   |
| Teacher | `teacher` | `Teacher@123` |
| Teacher | `msmith`  | `Teacher@123` |
| Student | `student` | `Student@123` |
| Student | `jdoe`    | `Student@123` |
| Student | `jsmith`  | `Student@123` |

Login accepts either username or email address.

---

## Features

### Admin
- User management (students, teachers, admins) — add, edit, deactivate, delete
- **Student photo upload** — upload a face photo per student to register them for AI attendance; face descriptor extracted automatically in the browser via face-api.js
- Bulk student import via CSV
- Course management — create courses, assign teachers, set semester/year
- Student enrollment management — enroll or drop students per course
- Timetable planner — weekly schedule with conflict detection
- System settings — institution name, academic year, grading scale
- Audit log viewer — full history of every system action with before/after JSON

### Teacher
- View assigned courses and enrolled student rosters
- Manual attendance — create sessions, mark Present / Absent / Late, edit past sessions
- **AI attendance** — webcam-based face recognition; automatically marks enrolled students Present as they face the camera, with live bounding-box overlays showing matched names; manual override available per student; duplicate session protection
- Grade entry — assignment (30%), midterm (30%), final (40%) with live grade calculation
- PDF reports — class attendance summary and class result sheet per course

### Student
- Personal dashboard — enrolled courses, GPA, credits, attendance rate
- Course cards with syllabus topics and instructor info
- Attendance history with per-course percentage breakdown
- Grades table with cumulative GPA
- Download personal grade report as PDF

---

## Project Structure

```
Lyceum/
├── Components/
│   ├── Pages/
│   │   ├── Admin/          # Admin pages and dialogs
│   │   ├── Teacher/        # Teacher pages and dialogs
│   │   └── Student/        # Student pages
│   └── Layout/             # MainLayout, NavMenu
├── Models/                 # Entity classes (User, Student, Course, Grade, …)
├── Services/               # EF Core DbContext + all business logic services
├── Migrations/             # Auto-generated EF Core migrations
└── wwwroot/                # CSS, fonts, static assets
```

---

## Key Commands

```bash
# Run in development
dotnet run

# Build
dotnet build

# Add a new migration
dotnet ef migrations add <MigrationName>

# Apply migrations manually
dotnet ef database update

# Reset — drop the database then re-run the app to reseed
```

---

## Documentation

See **[USER_MANUAL.md](USER_MANUAL.md)** for a full walkthrough of every feature by role.
