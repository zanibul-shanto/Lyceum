# AGENTS.md — Lyceum

## Build & Run

```bash
dotnet build
dotnet run                # https://localhost:7081 / http://localhost:5058
dotnet ef migrations add <Name>
dotnet ef database update  # auto-applied on startup, but useful manually
```

No test suite exists. Verify by running and exercising the affected page.

Database auto-migrates and seeds on first startup via `DbInitializer.InitializeAsync()` in `Program.cs`. To reset: drop `LyceumDb` in SQL Server and restart.

Database: SQL Server via `Server=.;Database=LyceumDb;Integrated Security=True;TrustServerCertificate=True` in `appsettings.json`.

## Important gotchas

- `/Migrations` and `/wwwroot/uploads` are **gitignored**. When adding new entities, run `dotnet ef migrations add <Name>` and commit the generated files — they will appear in git.
- The project uses `.slnx` (new XML solution format), not `.sln`.
- `BlazorDisableThrowNavigationException` is set to `true` in `.csproj` — navigation exceptions are suppressed.
- All services are scoped (`AddScoped`). `LyceumDbContext` is scoped too — pages may inject it directly when bypassing a service method (safe because same request scope).

## Authentication & Roles

Roles are stored as `UserRole` enum on `User.Role` (not in `AspNetUserRoles`). `LyceumUserClaimsPrincipalFactory` injects `ClaimTypes.Role` from `user.Role.ToString()`. All pages gate access via `<AuthorizeView Roles="Admin|Teacher|Student">` with a redundant `OnAfterRenderAsync` redirect check.

Login: POST `/api/auth/login` (form body: `username`, `password`). Accepts username or email.

## Test accounts

| Role | Username | Password |
|------|----------|----------|
| Admin | `admin` | `Admin@123` |
| Teacher | `teacher`, `msmith` | `Teacher@123` |
| Student | `student`, `jdoe`, `jsmith` | `Student@123` |

## Data model

```
User ──1:1──► Student ──M:N──► Course (via StudentEnrollment)
User ──1:1──► Teacher ──1:M──► Timetable ──M:1──► Course
Course ──1:M──► Subject, AttendanceSession, Grade, Timetable
```

Teacher–course assignment is managed through `Timetable` entries (each entry assigns one teacher to a course time slot). There is no separate `CourseTeacher` junction.

`User : IdentityUser<int>` — `Username` is a `[NotMapped]` property aliasing `IdentityUser.UserName`, not a separate column. `Student` and `Teacher` are separate models with 1:1 FK to `User`; do not add their fields to `User`.

## Service layer

All in `Services/`, namespace `Lyceum.Services`. Key services:
- `StudentService` / `TeacherService` — CRUD; call `UserManager` for user creation/deletion
- `CourseService` — CRUD; course is purely definitional (name, code, credits, capacity). Scheduling and teacher assignment handled via `TimetableService`.
- `EnrollmentService` — enroll/drop students per course
- `AttendanceService` — sessions + records; `MarkAttendanceAsync` deletes then re-inserts all records
- `GradeService` — upsert; `ComputeGrade` applies 30/30/40 (assignment/midterm/final)
- `TimetableService` — `CheckConflictAsync` prevents teacher double-booking
- `ReportService` — PDF via QuestPDF; download uses `JSRuntime.InvokeVoidAsync("downloadFileFromBytes", filename, base64)`

## Pages & dialogs

Pages under `Components/Pages/{Admin,Teacher,Student}/`. Dialogs (modal forms) are separate `.razor` files in the same folder, opened via `DialogService.OpenAsync<TComponent>()` and closed with `DialogService.Close(result)`.

## Face recognition (AI attendance)

- All photos saved to `wwwroot/uploads/photos/` with GUID filename (only one photo folder)
- JS helpers in `Components/App.razor`: `loadFaceApiModels()`, `extractFaceDescriptor(dataUrl)` (returns null if no face), `startAutoAttendance()`, `stopAutoAttendance()`
- `Student.FaceDescriptor` stores JSON-serialised `float[128]` or null
- `SessionType.Manual = 0`, `SessionType.AI = 1` — always pass `SessionType.AI` from `AutoAttendance.razor`
- `AttendanceService.SessionExistsAsync(courseId, date)` checks for duplicate sessions

## Behavioral guidelines

**Think before coding** — state assumptions explicitly, present tradeoffs, push back on overcomplicated approaches, stop when confused and ask.

**Simplicity first** — minimum code that solves the problem. No speculative abstractions, no features beyond what was asked, no error handling for impossible scenarios.

**Surgical changes** — touch only what the task requires. Match existing style. Don't improve adjacent code. If unrelated dead code exists, mention it — don't delete it.

**Goal-driven execution** — define verifiable success criteria before making changes. For bugs, identify the exact failing path first.
