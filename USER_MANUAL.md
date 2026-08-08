# Lyceum — User Manual

A web-based academic management platform for students, teachers, and administrators. Built with ASP.NET Core Blazor Server and SQL Server.

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

You can also log in with the email address instead of the username.

---

## Role-Based Access

Each role has its own dashboard and a set of allowed actions. After login you are redirected automatically to your portal.

---

## Admin Portal

### Dashboard — `/admin/dashboard`
- Overview of total users, students, teachers, and admins
- Donut chart showing user distribution by role
- User directory with search and role filter
- Add / edit / delete any user account directly from the grid

### Students — `/admin/students`
- Full list of all student accounts with enrollment date, phone, and active status
- **Add Student** — opens a form (full name, email, date of birth, gender, phone, address). Username is auto-generated from the email prefix; a temporary password is required.
- **Edit** — update any student's personal details. The form includes a **photo upload** (camera icon on the avatar): uploading a clear, front-facing photo automatically extracts a face descriptor and registers the student for AI attendance. A green "Face registered for AI attendance" label confirms success; a yellow warning means no face was detected and the photo should be re-uploaded.
- **Active toggle** — flip the switch to activate or deactivate an account without deleting it
- **Delete** — permanently removes the student and their user account
- **Import Students (CSV)** — bulk-import students from a `.csv` file (max 5 MB). See the CSV format section below.

> **Photo tips for AI attendance:** Use a well-lit, front-facing photo where the face is clearly visible. Glasses, hats, or extreme angles reduce detection accuracy. Photos are stored in `wwwroot/uploads/photos/`.

### Teachers — `/admin/teachers`
- Full faculty list with department, joining date, and active status
- **Add Teacher** — full name, email, department, qualification, phone, office room, biography. Username auto-generated from email.
- **Edit / Deactivate / Delete** — same pattern as students
- Filter by department and active status

### Courses — `/admin/courses`
- **Add Course** — name, code (must be unique), description, schedule text, room number, credits (1–10), max capacity, semester, academic year, active toggle, and teacher assignments (multi-select)
- **Edit Course** — all fields including replacing the assigned teachers
- **Active toggle** — enable or disable a course without deleting it
- **Delete** — removes the course permanently
- **Manage Enrollments** (green people icon) — opens an enrollment panel for the course:
  - Left side: currently enrolled students with a **Drop** button for each
  - Right side: available active students with an **Enroll** button; use the search box to filter by name or code
  - Changes take effect immediately

### Timetable — `/admin/timetable`
- Weekly calendar view (Monday–Friday) showing all scheduled classes
- Filter by semester and academic year using the dropdowns at the top
- **Add Entry** — select a course, teacher, day of week, start/end time, room number, semester, and academic year. The system prevents overlapping entries for the same teacher on the same day.
- **Edit / Delete** each slot directly from the calendar card

### Audit Logs — `/admin/audit-logs`
- Complete history of every Create, Update, Delete, and Login action in the system
- Filter by user role, action type, and date range
- Click **Inspect** on any row to see the full before/after JSON snapshot

### Settings — `/admin/settings`
- Institution name
- Current academic year and semester
- Grading scale (letter grade boundaries and GPA points)

---

## Teacher Portal

### Dashboard — `/teacher/dashboard`
- Profile header showing name, department, teacher code, and office room
- Professional bio card
- GPA distribution chart for all enrolled students across your courses
- Quick stats: number of assigned courses, total enrolled students, class average GPA
- Assigned courses grid and upcoming timetable sessions

### My Courses — `/teacher/courses`
- List of all courses you are assigned to with code, credits, room, and enrollment count

### Manual Attendance — `/teacher/attendance`

1. Select a course from the dropdown.
2. Pick a session date, start time, and end time.
3. The student roster loads automatically (only enrolled, active students).
4. Mark each student **Present**, **Absent**, or **Late** using the radio buttons. Everyone defaults to Present.
5. Click **Save Attendance** — a new attendance session is created in the database.

**Editing a past session:**
- The session history panel on the right lists all previous sessions with P / A / L tallies.
- Click the **edit** (pencil) icon on a session to load it into the roster for corrections.
- Click **Save Attendance** again to overwrite the records.
- Click **Cancel Edit** to discard changes.
- Click the **delete** icon to permanently remove a session and all its records.

### AI Attendance — `/teacher/auto-attendance`

Webcam-based face recognition attendance. The system recognises enrolled students automatically by matching their face against the photo registered by the admin.

**Before using AI Attendance:**
The admin must upload a clear, front-facing photo for each student via **Admin → Students → Edit**. Students with a registered face show a green badge count at the top of the webcam card. Students without a registered face show a ⚠ warning icon in the roster.

**Taking attendance:**

1. Select a course from the dropdown.
2. Optionally set a session date, start time, and end time.
3. The student roster loads on the right. Students without face data are flagged.
4. Click **Start Camera** — the browser will ask for webcam permission.
   - A pulsing blue dot and "Scanning every 1.2s" indicator shows the AI is actively scanning.
   - A live counter shows how many students have been recognised so far.
5. Have each student face the camera. When a face is matched:
   - A **green bounding box** appears on the video feed with the student's name.
   - The student's row in the roster flashes green and their status changes to **Present**.
   - Unknown faces show a grey bounding box with "?".
6. You can manually override any status using the **P / A / L** buttons at any time.
7. Click **Stop Camera** when done, then **Save Attendance**.

**Duplicate protection:** If a session already exists for the selected course and date, saving is blocked with an error message. Choose a different date or delete the existing session first.

> AI attendance sessions are tagged as **AI** type in the database, distinct from manually created sessions.

### Grades — `/teacher/grades`

1. Select a course.
2. The roster shows all enrolled students with their current marks (or "–" if not yet entered).
3. Click **Edit** next to any student to open the grade dialog:
   - Enter **Assignment Marks** (30% weight, 0–100)
   - Enter **Midterm Marks** (30% weight, 0–100)
   - Enter **Final Exam Marks** (40% weight, 0–100)
   - The **Computed Total** and **Letter Grade** update live as you type
4. Click **Apply Grades** to save. Re-entering grades for the same student/course updates the existing record.

**Grading scale:**

| Letter | Total Marks | GPA Points |
|--------|-------------|------------|
| A      | 90 – 100    | 4.0        |
| B      | 80 – 89     | 3.0        |
| C      | 70 – 79     | 2.0        |
| D      | 60 – 69     | 1.0        |
| F      | Below 60    | 0.0        |

### Schedule — `/teacher/schedule`
- Your personal weekly timetable showing all sessions across all assigned courses

### Reports — `/teacher/reports`

1. Select a course.
2. Click **Generate Class Attendance Report** — downloads a PDF with a per-student breakdown of Present / Absent / Late counts and attendance percentage.
3. Click **Generate Class Result Report** — downloads a PDF with all students' assignment, midterm, final, total marks, and letter grades.

---

## Profile Page — `/profile`

Available to all roles (Admin, Teacher, Student).

- **Basic Information** — update your full name, email, and role-specific fields (phone, gender, address for students; phone, department, bio for teachers)
- **Change Password** — enter your current password plus the new password (confirmed twice)
- **Profile Photo** — click the camera icon on the avatar to upload a new photo (JPG, PNG, WEBP, GIF — max 5 MB)
  - For **students**: uploading a new photo automatically re-extracts the face descriptor and updates AI attendance registration. A success notification confirms "face registered"; a warning means no face was detected in the photo.
  - For teachers and admins: the photo is saved and displayed in the profile only.

---

## Student Portal

### Dashboard — `/student/dashboard`
- KPI cards: enrolled courses, total credits, cumulative GPA, overall attendance rate
- Course progress cards showing attendance percentage per course with a visual progress bar
- Bar chart comparing attendance across all your courses
- Recent grades table with a link to the full grades page

### My Courses — `/student/courses`
- Cards for each enrolled course showing: course code, name, description, schedule, room, instructor name(s), and syllabus topics

### Attendance — `/student/attendance`
- Overall attendance rate KPI
- Per-course attendance bars with colour coding:
  - 🟢 Green — ≥ 90%
  - 🔵 Blue — ≥ 75%
  - 🟡 Yellow — ≥ 60%
  - 🔴 Red — below 60%
- Full history table: date, course, status (Present / Absent / Late), and any teacher notes

### Grades — `/student/grades`
- Cumulative GPA card and total enrolled credits
- Grading scheme reference card
- Table showing assignment, midterm, final, total marks, letter grade, and GPA points per course
- **Download Grade Report** — generates and downloads a personal PDF grade report

### Schedule — `/student/schedule`
- Your weekly timetable based on the courses you are enrolled in

---

## CSV Import Format (Students)

The CSV file must have a header row. Supported columns:

| Column        | Required | Notes                        |
|---------------|----------|------------------------------|
| `FullName`    | Yes      |                              |
| `Email`       | Yes      | Must be unique               |
| `Password`    | Yes      | Minimum 6 characters         |
| `DateOfBirth` | No       | Format: `YYYY-MM-DD`         |
| `Gender`      | No       | `Male`, `Female`, or `Other` |
| `Phone`       | No       |                              |
| `Address`     | No       |                              |

**Example:**

```csv
FullName,Email,Password,DateOfBirth,Gender,Phone
Alice Johnson,alice@example.com,Alice@123,2003-04-12,Female,555-0201
Bob Lee,bob@example.com,Bob@123,2002-11-30,Male,
```

After import, a success notification shows how many students were created. Any rows that failed (duplicate email, missing required field, etc.) are listed in a warning notification.

---

## Database

- Connection string is in `appsettings.json` — defaults to `Server=.;Database=LyceumDb;Integrated Security=True`
- Migrations apply automatically on startup — no manual steps needed
- To reset the database: drop `LyceumDb` in SQL Server and restart the app; it will be recreated and re-seeded with test data

---

## Project Structure

```
Lyceum/
├── Components/
│   ├── Pages/
│   │   ├── Admin/          # Admin pages and dialogs
│   │   ├── Teacher/        # Teacher pages and dialogs
│   │   └── Student/        # Student pages
│   └── Layout/             # Sidebar, main layout
├── Models/                 # Entity models (User, Student, Course, …)
├── Services/               # Business logic and EF Core services
├── Migrations/             # EF Core database migrations
└── wwwroot/                # Static assets (CSS, fonts)
```
