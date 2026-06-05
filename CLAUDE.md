# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```bash
dotnet build
dotnet run                              # https://localhost:7081
dotnet ef migrations add <Name>         # add a migration
dotnet ef database update               # apply manually (auto-runs on startup)
```

No test suite exists. Verify changes by running the app and exercising the affected page.

Database auto-migrates and seeds on first startup via `DbInitializer.InitializeAsync()` called in `Program.cs`. To reset: drop `LyceumDb` in SQL Server and restart.

## Tech Stack

- ASP.NET Core 10 Blazor Server (Interactive Server render mode)
- Entity Framework Core 10 + SQL Server
- ASP.NET Core Identity (`IdentityUser<int>`, cookie auth, 7-day expiry)
- **Radzen Blazor** for all UI components (`RadzenDataGrid`, `RadzenDropDown`, `RadzenButton`, `DialogService`, `NotificationService`, etc.)
- Bootstrap 5 + Bootstrap Icons for layout and icons
- **face-api.js 0.22** (CDN) for in-browser face detection and recognition (AI attendance)
- QuestPDF for PDF generation, CsvHelper for bulk import

## Architecture

### Authentication

Login posts to `/api/auth/login` (a minimal API endpoint in `Program.cs`). `SignInManager.PasswordSignInAsync` sets the cookie. The auth state provider is `LyceumAuthStateProvider` which extends `RevalidatingServerAuthenticationStateProvider` (30-min revalidation, validates security stamp).

**Critical:** Roles are stored as a custom `UserRole` enum on `User.Role`, not in `AspNetUserRoles`. `LyceumUserClaimsPrincipalFactory` overrides `GenerateClaimsAsync` to inject `ClaimTypes.Role` from `user.Role.ToString()` so the cookie carries the role claim. All pages gate access via `<AuthorizeView Roles="Admin|Teacher|Student">` and a redundant `OnAfterRenderAsync` redirect check.

### User / Identity Model

`User : IdentityUser<int>` adds `FullName`, `Role` (enum), `IsActive`, `CreatedAt`, `UpdatedAt`. `Username` is a `[NotMapped]` property that aliases `IdentityUser.UserName` — it is not a separate DB column.

`Student` and `Teacher` are **separate model classes** with a 1:1 FK to `User` (`UserId`). They are created by `StudentService.CreateAsync` / `TeacherService.CreateAsync` immediately after `UserManager.CreateAsync`. Do not add student/teacher fields to `User`.

### Data Model relationships

```
User ──1:1──► Student ──M:N──► Course  (via StudentEnrollment)
User ──1:1──► Teacher ──M:N──► Course  (via CourseTeacher, composite PK)
Course ──1:M──► Subject
Course ──1:M──► AttendanceSession ──1:M──► AttendanceRecord
Course ──1:M──► Grade
Course ──1:M──► Timetable
```

`CourseTeacher` uses composite PK `(CourseId, TeacherId)` — no surrogate key. `CourseService.UpdateAsync` replaces all junction rows atomically. Pass `List<int>` of **Teacher.Id** (not UserId) to `CreateAsync`/`UpdateAsync`.

### Service Layer

All services are scoped (`AddScoped`) and injected via constructor (primary constructor syntax). `LyceumDbContext` is also scoped — pages may inject it directly when bypassing a service method (e.g. `ManualAttendance.razor`), which works safely because all share the same request scope.

Key services and their responsibilities:

| Service | Responsibility |
|---------|---------------|
| `StudentService` | Student CRUD; calls `UserManager` for user creation/deletion |
| `TeacherService` | Teacher CRUD; same pattern |
| `CourseService` | Course CRUD + teacher assignment via `CourseTeacher` junction |
| `EnrollmentService` | Enroll / drop students per course |
| `AttendanceService` | Sessions + records; `MarkAttendanceAsync` deletes then re-inserts all records for a session |
| `GradeService` | Grade entry (upsert); `ComputeGrade` applies 30/30/40 weighting |
| `TimetableService` | Schedule CRUD; `CheckConflictAsync` prevents teacher double-booking |
| `ReportService` | PDF generation via QuestPDF |
| `AuditLogService` | Append-only audit trail |
| `SystemSettingService` | Key-value config store (AcademicYear, CurrentSemester, GradingScale) |
| `CsvImportService` | Bulk student creation from CSV stream |

### Pages & Dialogs

Pages live under `Components/Pages/{Admin,Teacher,Student}/`. Dialogs (modal forms) are separate `.razor` files in the same folder, opened via `DialogService.OpenAsync<TComponent>()` and closed with `DialogService.Close(result)`.

Admin dialogs: `StudentDialog`, `TeacherDialog`, `CourseDialog`, `EnrollmentDialog`, `TimetableDialog`, `SubjectDialog`, `UserDialog`.  
Teacher dialogs: `GradeDialog`.

### PDF Download

`window.downloadFileFromBytes(filename, base64)` is defined inline in `Components/App.razor`. All PDF download calls must use:
```csharp
await JSRuntime.InvokeVoidAsync("downloadFileFromBytes", filename, base64);
```

### Photo Upload & Face Recognition

All student photos (uploaded via `StudentDialog` or the profile page) are saved to `wwwroot/uploads/photos/` using a GUID filename. There is only one photo folder — do not create or reference `uploads/profiles/`.

After saving the file, call `extractFaceDescriptor(dataUrl)` (defined in `App.razor`) to extract a 128-float face descriptor in the browser. Store the result as a JSON string in `Student.FaceDescriptor`. If null is returned, show a warning — no face was detected.

`Student` has two face-related fields:
- `PhotoUrl` — path to the image file (e.g. `/uploads/photos/{guid}.jpg`)
- `FaceDescriptor` — JSON-serialised `float[128]`, or null if no face detected

**AI Attendance JS functions** (all defined in `App.razor`):
- `loadFaceApiModels()` — lazy-loads TinyFaceDetector + FaceLandmarks + FaceRecognition models from CDN
- `extractFaceDescriptor(dataUrl)` — returns `float[]` or null; used during photo upload
- `startAutoAttendance(videoId, canvasId, descriptorsJson, namesJson, dotNetRef)` — starts webcam loop, draws face bounding boxes on canvas, invokes `OnFaceDetected(studentId)` on the Blazor component
- `stopAutoAttendance()` — stops the interval and the media stream

`SessionType` enum has two values: `Manual = 0` (created via `/teacher/attendance`) and `AI = 1` (created via `/teacher/auto-attendance`). Always pass `SessionType.AI` when saving from `AutoAttendance.razor`. `AttendanceService.SessionExistsAsync(courseId, date)` checks for duplicate sessions.

### CSS

Global design tokens and component classes are in `wwwroot/app.css`. Reuse existing classes:
- `.dashboard-card` — card with shadow
- `.kpi-card` / `.kpi-number` / `.kpi-label` — stat widgets
- `.premium-page-header` — page title bar (Student/Teacher portals)
- `.badge-role`, `.badge-admin`, `.badge-teacher`, `.badge-student` — role pills
- `.page-title-modern`, `.page-subtitle-modern` — heading styles
- `.form-control-custom` — styled form inputs

## Key Files

| Purpose | Path |
|---------|------|
| App entry point + auth endpoints | `Program.cs` |
| Role claim injection | `Services/LyceumUserClaimsPrincipalFactory.cs` |
| Auth state provider | `Services/LyceumAuthStateProvider.cs` |
| EF DbContext + model config | `Services/LyceumDbContext.cs` |
| Seed data | `Services/DbInitializer.cs` |
| HTML shell + JS helpers (incl. face-api.js) | `Components/App.razor` |
| Global Razor imports | `Components/_Imports.razor` |
| Sidebar navigation | `Components/Layout/NavMenu.razor` |
| AI attendance page | `Components/Pages/Teacher/AutoAttendance.razor` |
| Student photo + face descriptor upload | `Components/Pages/Admin/StudentDialog.razor` |
| Profile photo upload (all roles) | `Components/Pages/Profile/ProfilePage.razor` |

## Behavioral Guidelines

### Think Before Coding
State assumptions explicitly. Ask when ambiguous rather than guessing. Push back when a simpler approach exists.

### Simplicity First
Minimum code that solves the problem. No abstractions for single-use code, no error handling for impossible scenarios.

### Surgical Changes
Touch only what the task requires. Match existing style. Mention unrelated dead code rather than deleting it.

### Goal-Driven Execution
Define a verifiable success condition before making changes. For bugs, identify the exact failing path first.
