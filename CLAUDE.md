
# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Lyceum is an ASP.NET Core Blazor-based educational management system for students, teachers, and administrators. It provides role-based dashboards, user management, and course/assignment tracking within a single integrated web application.

## Tech Stack

- **Framework**: ASP.NET Core 10.0 with Blazor (Interactive Server Components)
- **Language**: C# 12+ (with nullable reference types enabled)
- **Database**: SQL Server with Entity Framework Core 10.0.8
- **Authentication**: ASP.NET Core Identity + Custom Claims-based Auth State
- **UI**: Razor components with Bootstrap 5 and custom CSS design system
- **Build/Run**: `dotnet` CLI

## Build & Run Commands

### Prerequisites
- .NET 10.0 SDK
- SQL Server LocalDB (or SQL Express)
- Visual Studio 2022+ or VS Code with C# extensions

### Build
```bash
dotnet build
```

### Run (Development)
```bash
dotnet run
```
- Launches on `https://localhost:7081` (or `http://localhost:5058`)
- Automatically applies database migrations and seeds test data on first startup
- Browser auto-opens to the login page

### Run (IIS Express)
```bash
dotnet run --launch-profile "IIS Express"
```

### Database
- Connection string in `appsettings.json`: `Server=.\SQLEXPRESS;Database=LyceumDb;`
- Migrations auto-apply on app startup via `DbInitializer.InitializeAsync()`
- To manually migrate: `dotnet ef database update`
- To add a new migration: `dotnet ef migrations add MigrationName`

### Test Data
On first run, the `DbInitializer` seeds 5 test accounts:
- **Admin**: username `admin`, password `Admin@123`
- **Teacher**: username `teacher`, password `Teacher@123`
- **Student**: username `student`, password `Student@123`
- Plus two additional users: `jdoe`, `msmith` (both with password `Student@123`)

## Architecture & Key Patterns

### Authentication Flow
1. User logs in via `Login.razor` component
2. `UserService.AuthenticateAsync()` verifies credentials against hashed passwords in database
3. `CustomAuthStateProvider` stores username in `ProtectedLocalStorage` and creates a `ClaimsPrincipal` with claims:
   - `ClaimTypes.NameIdentifier` (user ID)
   - `ClaimTypes.Name` (username)
   - `ClaimTypes.Email`
   - `ClaimTypes.Role` (Student/Teacher/Admin enum as string)
   - Custom `FullName` claim
4. Razor components use `@attribute [Authorize(Roles="...")]` or `<AuthorizeView>` for access control
5. Home page redirects authenticated users to their role-specific dashboard

### User Model Architecture
`User` extends `IdentityUser<int>` with:
- **Common fields**: `FullName`, `Role` (enum), `CreatedAt`, `Email`, `Username`
- **Student fields**: `StudentId` (auto-generated STU-YYYY-XXXX), `GradeLevel`, `GPA`, `AttendanceRate`
- **Teacher fields**: `TeacherId` (auto-generated TCH-YYYY-XXX), `Department`, `Bio`, `OfficeRoom`

The `UserRole` enum (Student=0, Teacher=1, Admin=2) drives role-based visibility and access throughout the UI.

### Database & EF Core
- `LyceumDbContext` extends `IdentityDbContext<User, IdentityRole<int>, int>`
  - Inherits all Identity tables (AspNetUsers, AspNetRoles, AspNetUserRoles, etc.)
  - Custom properties on User are stored in AspNetUsers table
- Migrations are in `Migrations/` directory; first migration (`20260522132608_init.cs`) creates all Identity tables + custom User columns
- `DbInitializer.InitializeAsync()` (called in `Program.cs`) ensures migrations run and seeds data if empty

### Component Organization
**Pages** (`Components/Pages/`):
- **Public**: `Login.razor`, `Register.razor` (no auth required)
- **Dashboard**: `Home.razor` (redirects to role-specific dashboard)
- **Role-specific**: `Admin/AdminDashboard.razor`, `Teacher/TeacherDashboard.razor`, `Student/StudentDashboard.razor`
- **Error**: `Error.razor`, `NotFound.razor`

**Layout** (`Components/Layout/`):
- `MainLayout.razor`: Two-column layout (sidebar + main content) with conditional auth page detection
- `NavMenu.razor`: Role-based sidebar with three portal sections and dynamic menu links
- `ReconnectModal.razor`: Handles Blazor reconnection scenarios

**Imports**: `_Imports.razor` provides global namespaces (Lyceum.*, Microsoft.AspNetCore.*)

### UI Design System
Global CSS variables in `wwwroot/app.css`:
- **Primary**: `--primary: #3182ce` (blue)
- **Success**: `--success: #10b981` (green)
- **Warning**: `--warning: #f59e0b` (amber)
- **Danger**: `--danger: #ef4444` (red)
- **Neutrals**: Slate shades from `--dark-navy` to `--light-bg`
- **Fonts**: 'Outfit' (headings), 'Plus Jakarta Sans' (body)
- **Shadows**: Premium, card, and hover variants

Custom component classes:
- `.auth-card-modern`: Login/register card styling
- `.dashboard-card`: Stat cards with icon background
- `.kpi-card`: KPI metric display (Enrolled Courses, Assignments, etc.)
- `.header-profile-card`: User avatar + name in page header

### Service Layer
**`UserService`** (dependency-injected via `builder.Services.AddScoped<UserService>()`)
- `AuthenticateAsync(usernameOrEmail, password)`: Login validation
- `RegisterAsync(user, password)`: Create account (auto-assigns StudentId or TeacherId if applicable)
- `GetAllUsersAsync()`, `GetUsersByRoleAsync(role)`: User queries
- `UpdateUserAsync(user)`, `DeleteUserAsync(id)`: User management

**`CustomAuthStateProvider`** (Scoped)
- `GetAuthenticationStateAsync()`: Restores auth state from protected storage
- `MarkUserAsAuthenticated(user)`: Login callback
- `MarkUserAsLoggedOut()`: Logout callback

**`LyceumDbContext`** (Scoped)
- Standard EF Core DbContext for database access

### Routing
Routes defined in `Routes.razor`:
- `<Router>` with `<CascadingAuthenticationState>` wraps all pages
- `<AuthorizeRouteView>` protects routes based on role claims
- `NotFound` component shown for missing routes
- Login/register pages skip auth check via `MainLayout` detection

## Common Development Tasks

### Adding a New User Role
1. Add enum value to `UserRole` in `Models/User.cs`
2. Add role-specific fields to `User` class if needed
3. Create migration: `dotnet ef migrations add AddRoleToUser`
4. Create new dashboard page: `Components/Pages/{RoleName}/{RoleName}Dashboard.razor`
5. Add `<AuthorizeView Roles="RoleName">` block in new page
6. Add sidebar menu section in `NavMenu.razor` with new role's nav links
7. Update `Home.razor` redirect logic to handle new role

### Adding a New Page/Feature
1. Create `.razor` file in `Components/Pages/` or subdirectory
2. Add `@page "/route"` directive
3. Wrap content in `<AuthorizeView Roles="RoleList">` if role-restricted
4. Inject required services: `@inject UserService`, `@inject NavigationManager`, etc.
5. Add sidebar link in `NavMenu.razor` for authenticated users
6. Use existing CSS classes (dashboard-card, kpi-card, btn-primary-custom) for consistency

### Extending User Model
1. Add property to `User` class in `Models/User.cs` with appropriate data type and validation attributes
2. Create migration: `dotnet ef migrations add DescriptiveReason`
3. Run `dotnet run` to apply migration
4. Update `RegisterAsync()` and `UpdateUserAsync()` in `UserService` if field should be set during registration/editing

### Modifying Authentication/Authorization
- Auth state provider: `Services/CustomAuthStateProvider.cs`
- User credential validation: `Services/UserService.AuthenticateAsync()`
- Claims creation: `CustomAuthStateProvider.CreateIdentity()` (update if adding custom claims)
- Role checks: Update role names in `@attribute [Authorize(Roles="...")]` or component role conditions

## File Locations Reference

| Purpose | Location |
|---------|----------|
| Entry point | `Program.cs` |
| User model | `Models/User.cs` |
| Auth provider | `Services/CustomAuthStateProvider.cs` |
| User CRUD/auth | `Services/UserService.cs` |
| Database context | `Services/LyceumDbContext.cs` |
| DB init & seeds | `Services/DbInitializer.cs` |
| Global styles | `wwwroot/app.css` |
| Config (dev) | `appsettings.json` / `appsettings.Development.json` |
| Launch profiles | `Properties/launchSettings.json` |
| Database migrations | `Migrations/` |
| Admin dashboard | `Components/Pages/Admin/AdminDashboard.razor` |
| Teacher dashboard | `Components/Pages/Teacher/TeacherDashboard.razor` |
| Student dashboard | `Components/Pages/Student/StudentDashboard.razor` |
| Sidebar nav | `Components/Layout/NavMenu.razor` |
| Main layout | `Components/Layout/MainLayout.razor` |

## Notes for Future Work

- **ProtectedLocalStorage** usage: Auth state relies on browser protected storage, which throws exceptions during pre-rendering. The `CustomAuthStateProvider` catches these safely.
- **Password policy**: Currently permissive (6+ chars, no complexity requirements). Update `Program.cs` password options for production.
- **Database migrations**: Migrations auto-apply on startup. Keep `DbInitializer` updated when adding features that require seeding.
- **Role-based redirects**: Home page routes authenticated users to their dashboard; update the switch statement if adding roles.
- **Accessibility**: Components use semantic HTML and Bootstrap Icon `<i>` tags; ensure alt text or ARIA labels when adding new icons.

## Behavioral Guidelines (Andrej Karpathy Skills)

**1. Think Before Coding**
State assumptions explicitly. If uncertain, ask. Surface confusion and tradeoffs rather than making silent decisions about ambiguous requirements.

**2. Simplicity First**
Minimum code that solves the problem — nothing speculative. Avoid unrequested features, premature abstraction, or unnecessary error handling. If 200 lines could be 50, rewrite it.

**3. Surgical Changes**
Touch only what you must. Clean up only your own mess. When editing, preserve existing style and avoid refactoring unrelated code. Remove only imports/functions that your changes made obsolete. If you notice unrelated dead code, mention it — don't delete it.

**4. Goal-Driven Execution**
Define success criteria. Loop until verified. Transform vague tasks into testable checkpoints — write tests first, then implement to pass them.
