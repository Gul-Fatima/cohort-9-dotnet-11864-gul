# Implementation Plan — Task Management Tool

**Cohort 9 · .NET Fullstack (.NET + ReactJS) · Gul Fatima**
**Repo:** `cohort-9-dotnet-11864-gul` · **Base branch:** `develop`

This plan turns `main.md` (project spec) and `git-workflow-cohort9.txt` (branch/PR process) into an actionable, phased build order.

---

## 0. Environment & Prerequisites

| Tool | Status on this machine | Action |
|------|------------------------|--------|
| .NET SDK | ❌ Not installed (`dotnet: command not found`) | Install **.NET 8 SDK (LTS)** from https://dotnet.microsoft.com/download |
| SQL Server | TBD (LocalDB or Express recommended) | Install **SQL Server LocalDB** (ships with VS) or SQL Server Express; fall back to `localhost` connection string |
| Node.js | ✅ v24.18.0 | None |
| npm | ✅ 11.16.0 | None |
| Git | ✅ 2.53.0 | None |
| CodeRabbit | ✅ configured (`.coderabbit.yaml`, reviews `main` + `develop`) | None |

> ⚠️ **Blocking prerequisite:** the .NET SDK must be installed before any backend work can build/run. The frontend (Node) can proceed independently.

---

## 1. Git Strategy (from git-workflow-cohort9.txt)

**One-time setup (this repo already cloned — only branch work remains):**

```bash
cd cohort-9-dotnet-11864-gul
git checkout main
git pull origin main
git checkout -b develop origin/develop   # origin/develop already exists on remote
git push origin develop                  # if not tracked locally yet
```

**Per-feature loop (repeat for every task below):**

```bash
git checkout develop
git pull origin develop
git checkout -b feature/<task-name>

# ...implement + test...

git add .
git commit -m "Add <task>"
git push origin feature/<task-name>
```

- Open PR on GitHub: **base repo** = `10pshine-cohort-9/cohort-9-dotnet-11864-gul` (the org repo this clone already points to) · **base branch** = `develop` · **compare** = `feature/<task-name>`.
- CodeRabbit auto-reviews → fix HIGH/CRITICAL findings → commit + push (updates same PR).
- Assign mentor for final review/merge, then clean up: `git checkout develop && git pull origin develop && git branch -d feature/<task-name>`.

---

## 2. Solution Architecture

```
cohort-9-dotnet-11864-gul/
├── TaskManagement.sln
├── src/
│   ├── TaskManagement.Api/            # ASP.NET Core Web API (controllers, DI, middleware, Program.cs)
│   ├── TaskManagement.Core/           # Entities, enums, DTOs, service/repo interfaces
│   ├── TaskManagement.Infrastructure/ # EF Core DbContext, migrations, repositories, seeding
│   └── TaskManagement.Services/       # Business logic: AuthService, TaskService, ProfileService
├── tests/
│   └── TaskManagement.Tests/          # xUnit tests (controllers, services, repositories)
├── frontend/                          # React (Vite) SPA
└── sonar-project.properties           # SonarQube config (C# + JavaScript)
```

**NuGet packages (Core/Infra/Services/Api as needed):**
`Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Tools`, `Microsoft.AspNetCore.Authentication.JwtBearer`, `System.IdentityModel.Tokens.Jwt`, `Serilog.AspNetCore`, `Serilog.Sinks.Console`, `Serilog.Sinks.File`, `Swashbuckle.AspNetCore`, `BCrypt.Net-Next` (password hashing), `Microsoft.EntityFrameworkCore.InMemory` (tests), `FluentAssertions` (tests, optional).

---

## 3. Data Model (from main.md "Suggested Database Entities")

```
User      : Id, Name, Email, PasswordHash, Role
Task      : Id, Title, Description, Status, Priority, DueDate, CategoryId, AssignedUserId, CreatedAt, UpdatedAt
Category  : Id, Name
```

**Enums:**
- `UserRole` → `Admin`, `User`
- `TaskStatus` → `Pending`, `InProgress`, `Completed`
- `TaskPriority` → `Low`, `Medium`, `High`

**Relationships:** `Task.CategoryId → Category`, `Task.AssignedUserId → User`. Seed data: 1 admin user + 1 regular user (hashed passwords), default categories (Work, Personal, Urgent).

---

## 4. API Surface

| Method | Endpoint | Access | Purpose |
|--------|----------|--------|---------|
| POST | `/api/auth/register` | Public | User registration |
| POST | `/api/auth/login` | Public | Login → JWT (with role claim) |
| GET | `/api/users/me` | Authorized | Fetch profile |
| GET | `/api/tasks` | Authorized | List + filter (status, priority, category, assigned user, due date, search) |
| GET | `/api/tasks/{id}` | Authorized | Task detail |
| POST | `/api/tasks` | Authorized | Create task |
| PUT | `/api/tasks/{id}` | Authorized | Update task |
| DELETE | `/api/tasks/{id}` | Authorized | Delete task |
| GET | `/api/tasks/dashboard/stats` | Authorized | Completed / In-Progress / Pending counts (admin sees all users) |
| GET | `/api/categories` | Authorized | Category list for filters/forms |

**Authorization rules:** Admin manages all tasks/users; regular user manages own tasks only (checked in services/repositories via current user claim).

---

## 5. Phased Build Order (each phase = one feature branch + PR)

### Phase 1 — Backend scaffold
- `feature/backend-scaffold`
- Install .NET SDK first. Create solution + 4 projects, wire DI, CORS, Swagger, `appsettings.json`.
- Empty-but-runnable API on `develop`.

### Phase 2 — Data layer (EF Core + SQL Server)
- `feature/data-layer`
- Entities, enums, `AppDbContext`, initial migration, seeding (admin/user/categories).
- Verify migration applies to SQL Server.

### Phase 3 — Auth API (JWT)
- `feature/auth-api`
- `AuthService`: register (hash password with BCrypt), login (validate → issue JWT with `role` claim).
- `POST /auth/register`, `POST /auth/login`, JWT bearer setup, `[Authorize(Roles = ...)]` enforcement.
- Serilog logs registrations & logins.

### Phase 4 — Task CRUD + Dashboard API
- `feature/task-api`
- `TaskService` + repository: create, list (with all filters), detail, update, delete, dashboard stats.
- Role-aware ownership rules. Serilog logs task create/update/delete.
- `GET /api/users/me` profile endpoint.

### Phase 5 — Global exception handling + logging polish
- `feature/logging-exceptions`
- Global exception-handling middleware → meaningful `ProblemDetails` responses, no crashes.
- Serilog configured for console + file; all exceptions logged.

### Phase 6 — Unit tests (xUnit)
- `feature/unit-tests`
- AuthService (register/login/hash/token), TaskService CRUD + ownership rules, controllers (mocked services), repository (EF InMemory).
- `dotnet test` green.

### Phase 7 — SonarQube
- `feature/sonarqube`
- `sonar-project.properties` for C# + JS, scanner instructions, quality-gate notes in README.

### Phase 8 — Frontend scaffold + auth screens
- `feature/frontend-auth`
- Vite + React + React Router + Axios; auth context storing JWT (localStorage); login/signup pages; protected routes; redirect to dashboard on login.

### Phase 9 — Frontend task screens
- `feature/frontend-tasks`
- Dashboard (task count cards), Task List (filters, create-new button, detail links), Task Detail, New/Edit Task form.
- Axios interceptor attaching Bearer token; role-aware UI.

### Phase 10 — Profile + integration polish
- `feature/frontend-profile`
- User Profile page with logout; error states; responsive styling; end-to-end manual test of the whole flow.

### Phase 11 — Optional extras (as time permits)
- `feature/signalr` (real-time task updates), `feature/export-import` (CSV export/import), advanced search — each a separate branch/PR per the workflow.

---

## 6. Frontend Structure (React + Vite)

```
frontend/
├── src/
│   ├── api/            # axios instance, auth/task endpoints
│   ├── context/        # AuthContext (login/logout/token)
│   ├── components/     # Navbar, ProtectedRoute, TaskForm, TaskFilters, StatCard...
│   ├── pages/          # Login, Signup, Dashboard, TaskList, TaskDetail, NewTask, Profile
│   ├── App.jsx         # routes
│   └── main.jsx
```

---

## 7. Deliverables Checklist (from main.md)

- [ ] ASP.NET Core Web API
- [ ] React.js frontend
- [ ] SQL Server database + EF Core integration + migrations
- [ ] JWT authentication + role-based authorization (Admin / Regular User)
- [ ] Task CRUD + dashboard stats
- [ ] Serilog logging (logins, registrations, task events, exceptions)
- [ ] Global exception handling
- [ ] xUnit test suite (controllers, services, repositories)
- [ ] SonarQube configuration (C# + JS)
- [ ] Git repo with proper branching strategy + PRs to `develop`
- [ ] README with setup/run instructions

---

## 8. Risks / Notes

- **.NET SDK missing** — install before Phase 1; frontend can start regardless.
- **SQL Server availability** — confirm LocalDB/Express or provide Docker fallback if unavailable.
- **PR base branch** — per the workflow notes, confirm with mentor that `develop` (not `main`) is the PR target; CodeRabbit already reviews both.
- Password hashing must never store plaintext; tokens must carry the role claim for authorization.
