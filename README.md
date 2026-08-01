# cohort-9-dotnet-11864-gul

Cohort 9 — .NET Fullstack (.NET + ReactJS) assignment for Gul Fatima.

**Task Management Tool** — a web-based task management system with user
authentication (JWT), role-based authorization, task CRUD, dashboard
statistics, Serilog logging, global exception handling, xUnit tests, and
SonarQube code quality analysis.

## Tech Stack

| Layer     | Technology                          |
|-----------|-------------------------------------|
| Backend   | ASP.NET Core Web API (in progress)  |
| Frontend  | React.js + Vite                     |
| ORM       | Entity Framework Core (in progress) |
| Database  | SQL Server (in progress)            |
| Logging   | Serilog (in progress)               |
| Testing   | xUnit (in progress)                 |
| Quality   | SonarQube (in progress)             |
| Versioning| Git with feature branches → `develop` |

## Frontend (React)

Located in [`frontend/`](frontend/).

```bash
cd frontend
npm install
npm run dev        # http://localhost:5173
```

The UI ships with a built-in **mock API** (localStorage-backed) so it is fully
usable before the backend is built. To switch to the real ASP.NET Core API:

```bash
# create .env from .env.example, then:
VITE_USE_MOCK=false
```

The Vite dev server proxies `/api` → `http://localhost:5000`.

### Demo accounts (mock API)

| Role  | Email            | Password   |
|-------|------------------|------------|
| Admin | admin@example.com | Admin@123  |
| User  | user@example.com  | User@123   |

## Branching strategy

Per the cohort git workflow: feature branches are cut from `develop`, PRs are
raised against the `develop` branch of `10pshine-cohort-9/cohort-9-dotnet-11864-gul`,
reviewed by CodeRabbit, then assigned to the mentor for merge.

See [`IMPLEMENTATION_PLAN.md`](IMPLEMENTATION_PLAN.md) for the full build plan.
