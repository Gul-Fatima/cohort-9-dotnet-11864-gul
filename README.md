# cohort-9-dotnet-11864-gul

Cohort 9 — .NET Fullstack (.NET + ReactJS) assignment for Gul Fatima.

**Task Management Tool** — a web-based task management system with JWT
authentication, role-based authorization (Admin vs User), task CRUD with
filters, dashboard statistics, EF Core + SQL Server, xUnit tests and
SonarQube code-quality configuration.

## Tech Stack

| Layer     | Technology                                   |
|-----------|----------------------------------------------|
| Backend   | ASP.NET Core 8 Web API                       |
| Frontend  | React 19 + Vite                              |
| ORM       | Entity Framework Core 8 (code-first)         |
| Database  | SQL Server / SQL Express (`localhost\SQLEXPRESS`) |
| Auth      | JWT (bearer) + BCrypt password hashing       |
| Testing   | xUnit + EF Core InMemory                     |
| Quality   | SonarQube (`sonar-project.properties` + SonarAnalyzer) |
| Versioning| Git with feature branches → `develop`        |

## Project layout

```
src/
  TaskManagement.Api            Web API host (controllers, JWT wiring, Swagger)
  TaskManagement.Core           Entities, enums, DTOs, ApiException
  TaskManagement.Infrastructure AppDbContext, migrations, seeding
  TaskManagement.Services       Business logic (auth, tasks, categories, users)
tests/
  TaskManagement.Tests          xUnit unit tests
frontend/                       React SPA
```

## Backend (ASP.NET Core)

```bash
# terminal 1 — API on http://localhost:5000 (Swagger at /swagger)
cd src/TaskManagement.Api
dotnet run
```

On startup the API applies pending migrations and seeds demo data
(categories, two accounts, sample tasks) — no manual DB setup needed.

### Demo accounts

| Role  | Email               | Password  |
|-------|---------------------|-----------|
| Admin | admin@taskmgmt.com  | Admin@123 |
| User  | user@example.com    | User@123  |

### API endpoints (all under `/api`)

| Method | Endpoint                  | Access        | Description                 |
|--------|---------------------------|---------------|-----------------------------|
| POST   | `/auth/register`          | public        | Create account → `{token, user}` |
| POST   | `/auth/login`             | public        | Login → `{token, user}`     |
| GET    | `/users/me`               | authenticated | Current user profile        |
| GET    | `/users`                  | admin         | User directory (assignee dropdown) |
| GET    | `/tasks`                  | authenticated | List (filters: status, priority, categoryId, assignedUserId, search, dueDate) |
| GET    | `/tasks/dashboard/stats`  | authenticated | Completed / in-progress / pending counts (role-scoped) |
| GET    | `/tasks/{id}`             | authenticated | Single task                 |
| POST   | `/tasks`                  | authenticated | Create task                 |
| PUT    | `/tasks/{id}`             | owner/admin   | Update task                 |
| DELETE | `/tasks/{id}`             | owner/admin   | Delete task                 |
| GET    | `/categories`             | authenticated | Category list               |

**Role rules:** admins see/manage every task; regular users only see and
manage tasks assigned to them (others → 404 on read, 403 on write).

## Frontend (React)

```bash
# terminal 2 — http://localhost:5173
cd frontend
npm install
npm run dev
```

The frontend talks to the real backend by default (`frontend/.env` sets
`VITE_USE_MOCK=false`); Vite proxies `/api` → `http://localhost:5000`.
Set `VITE_USE_MOCK=true` to run against the built-in localStorage mock
instead (handy for demos without the backend running).

## Tests

```bash
dotnet test TaskManagement.sln
```

23 xUnit tests covering auth (register/login/validation), task service
(role scoping, filters, CRUD, dashboard stats) and categories — using EF
Core's InMemory provider so no database is required.

## SonarQube

`sonar-project.properties` at the repo root configures the scanner; the
`SonarAnalyzer.CSharp` analyzer (via `Directory.Build.props`) runs the
same rules locally during `dotnet build`.

```bash
dotnet build TaskManagement.sln
sonar-scanner -Dsonar.login=<token>
```

## Branching strategy

Per the cohort git workflow: feature branches are cut from `develop`, PRs
are raised against the `develop` branch of
`10pshine-cohort-9/cohort-9-dotnet-11864-gul`, reviewed by CodeRabbit,
then assigned to the mentor for merge.

See [`IMPLEMENTATION_PLAN.md`](IMPLEMENTATION_PLAN.md) for the full build plan.
