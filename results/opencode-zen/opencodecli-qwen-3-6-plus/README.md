# Task Management System

A production-ready full-stack task management system built with .NET 8, PostgreSQL, and React 18.

## Overview

This application provides a complete task management solution with user authentication, task CRUD operations, and commenting. It features a clean layered architecture on the backend and a modern React frontend with TypeScript.

**Key Features:**
- JWT-based authentication (register/login)
- Task management with status, priority, and assignment
- Comment system with edit/delete ownership enforcement
- Filterable task list
- Responsive UI with Tailwind CSS

## Prerequisites

- Docker and Docker Compose
- .NET 8 SDK (for running tests locally)
- Node.js 20+ (for running frontend tests locally)

## Quick Start

```bash
docker-compose up --build
```

After the containers start, the application is available at:
- **Frontend**: http://localhost:3000
- **API**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger

### Demo User

A demo user is seeded on first startup:
- **Email**: `demo@example.com`
- **Password**: `password`

## Running Tests

### Backend Tests (xUnit)

```bash
cd backend
dotnet test tests/TaskManager.Tests/TaskManager.Tests.csproj
```

### Frontend Tests (Vitest)

```bash
cd frontend
npm install
npm run test
```

## Environment Variables

| Variable | Default | Description |
|---|---|---|
| `POSTGRES_USER` | `taskmanager` | PostgreSQL database user |
| `POSTGRES_PASSWORD` | `taskmanager_secret` | PostgreSQL database password |
| `POSTGRES_DB` | `taskmanager` | PostgreSQL database name |
| `ASPNETCORE_ENVIRONMENT` | `Production` | ASP.NET Core environment |
| `JWT_SECRET` | `supersecretkey1234567890123456789012` | JWT signing key (min 32 chars) |
| `JWT_ISSUER` | `TaskManagerApi` | JWT token issuer |
| `JWT_AUDIENCE` | `TaskManagerClient` | JWT token audience |
| `JWT_EXPIRY_MINUTES` | `60` | JWT token expiration in minutes |

## Architecture

### Backend

```
TaskManager.Domain        — Entities and enums (no dependencies)
TaskManager.Application   — DTOs, validators, service interfaces and implementations
TaskManager.Infrastructure — EF Core DbContext, repositories, JWT service, migrations
TaskManager.Api           — Controllers, middleware, DI setup, entry point
TaskManager.Tests         — xUnit tests for all service methods
```

### Frontend

```
src/api/       — Axios client and API service functions
src/types/     — TypeScript type definitions
src/schemas/   — Zod validation schemas
src/pages/     — Page components (Login, Register, TaskList, TaskDetail)
src/components/ — Shared components (ProtectedRoute, TaskCommentSection)
```

## Assumptions & Decisions

1. **JWT Expiry**: Set to 60 minutes by default. Access-token only (no refresh token) to keep scope manageable.
2. **Password Hashing**: BCrypt with default work factor (11) via `BCrypt.Net-Next`.
3. **Pagination**: Not implemented — all tasks are returned in a single list. Filter by status, priority, and assignedToId is supported.
4. **Nginx Port Binding**: Frontend serves on port 80 inside the container, mapped to port 3000 on the host.
5. **API Port Binding**: API listens on port 8080 inside the container, mapped to port 5000 on the host.
6. **Database Health Check**: Uses `pg_isready` with 5-second intervals, 10 retries, and a 10-second start period.
7. **EF Core Migrations**: Run automatically on API startup via the Dockerfile entrypoint script (`dotnet ef database update`).
8. **CORS**: Not needed — Nginx proxies `/api/` requests to the backend container, so the frontend and API share the same origin from the browser's perspective.
9. **Demo User Password**: The seeded demo user has a pre-hashed password for `demo123`. In production, this should be replaced with a proper migration or seed process.
10. **Test Coverage Scope**: Backend tests cover all service methods (happy path, not-found, unauthorized, validation). Frontend tests cover page rendering, filter functionality, error states, and comment interactions.
11. **No Component Library**: Tailwind CSS is used directly for styling — no external UI component library.
12. **React Query**: TanStack Query v5 is used for all server state management with automatic cache invalidation on mutations.
13. **Form Validation**: React Hook Form + Zod on the frontend mirrors FluentValidation constraints on the backend (title max 200, description max 5000, comment body max 2000).
14. **File Naming**: All files use lowercase with hyphens (kebab-case) per project standards.
15. **Error Mapping**: `UnauthorizedAccessException` maps to HTTP 403, `KeyNotFoundException` maps to HTTP 404, `InvalidOperationException` maps to HTTP 400.
