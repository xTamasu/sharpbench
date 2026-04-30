# Task Management System

A production-ready full-stack task management application with users, tasks, and threaded comments.

## Tech Stack

- **Backend:** C# .NET 8, ASP.NET Core, Entity Framework Core, PostgreSQL 16
- **Frontend:** React 18, TypeScript, Vite, TanStack Query, React Hook Form + Zod, Tailwind CSS
- **Auth:** JWT (HS256) with BCrypt password hashing
- **Containerization:** Docker + Docker Compose, Nginx serving the SPA and proxying `/api` to the backend
- **Testing:** xUnit + Moq (backend), Vitest + React Testing Library (frontend)

## Prerequisites

- [Docker](https://www.docker.com/) 20+ and Docker Compose v2
- (Optional, for running tests outside containers) [.NET 8 SDK](https://dotnet.microsoft.com/) and [Node.js 20+](https://nodejs.org/)

## How to Run

From the repository root:

```bash
docker-compose up --build
```

Then open <http://localhost:3000>.

A demo user is seeded automatically:

| Email | Password |
|---|---|
| `demo@taskmanager.local` | `Password123!` |

The backend exposes Swagger at <http://localhost:8080/swagger>.

## How to Run Tests

### Backend

```bash
cd backend
dotnet test
```

### Frontend

```bash
cd frontend
npm install
npm test
```

## Environment Variables

| Variable | Default | Used By | Purpose |
|---|---|---|---|
| `POSTGRES_USER` | `taskmanager` | db, api | PostgreSQL username |
| `POSTGRES_PASSWORD` | `taskmanager_dev_pw` | db, api | PostgreSQL password |
| `POSTGRES_DB` | `taskmanager` | db, api | PostgreSQL database name |
| `JWT_SECRET` | `CHANGE_ME_super_long_dev_only_secret_key_at_least_32_chars_long_123` | api | HS256 signing key (must be ≥ 32 chars) |
| `JWT_ISSUER` | `TaskManager` | api | JWT `iss` claim |
| `JWT_AUDIENCE` | `TaskManagerClients` | api | JWT `aud` claim |
| `JWT_EXPIRY_MINUTES` | `120` | api | Access token lifetime |

## Project Layout

```
/
├── docker-compose.yml
├── backend/
│   ├── Dockerfile
│   ├── TaskManager.sln
│   ├── src/
│   │   ├── TaskManager.Domain/         # Entities + enums
│   │   ├── TaskManager.Application/    # DTOs, services, validators, interfaces
│   │   ├── TaskManager.Infrastructure/ # EF Core, repositories, JWT, password hashing
│   │   └── TaskManager.Api/            # Controllers, middleware, Program.cs
│   └── tests/TaskManager.Tests/
└── frontend/
    ├── Dockerfile
    ├── nginx.conf
    ├── package.json, vite.config.ts, tsconfig.json, etc.
    └── src/
```

See the `## Assumptions & Decisions` section of the original generation message for design choices.
