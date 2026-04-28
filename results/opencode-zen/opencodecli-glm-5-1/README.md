# TaskManager — Full-Stack Task Management System

A production-ready task management system built with C# .NET 8, React 18, TypeScript, and PostgreSQL.

## Prerequisites

- Docker & Docker Compose
- (Optional) .NET 8 SDK for running backend tests locally
- (Optional) Node.js 20+ for running frontend tests locally

## How to Run

```bash
docker-compose up
```

The application will be available at **http://localhost:3000**.

A demo user is seeded on first run:
- **Email:** `demo@taskmanager.com`
- **Password:** `demo123`

## How to Run Tests

### Backend Tests

```bash
cd backend
dotnet test
```

### Frontend Tests

```bash
cd frontend
npm install
npm test
```

## Environment Variables

| Variable | Default | Description |
|---|---|---|
| `POSTGRES_USER` | `taskmanager` | PostgreSQL username |
| `POSTGRES_PASSWORD` | `taskmanager_secret` | PostgreSQL password |
| `POSTGRES_DB` | `taskmanagerdb` | PostgreSQL database name |
| `JWT_SECRET` | `super_secret_jwt_key_at_least_32_chars_long` | JWT signing key |
| `JWT_ISSUER` | `TaskManager` | JWT issuer |
| `JWT_AUDIENCE` | `TaskManagerUsers` | JWT audience |
| `JWT_EXPIRY_MINUTES` | `60` | JWT token expiry in minutes |

## Project Structure

```
/
├── docker-compose.yml
├── README.md
├── backend/
│   ├── Dockerfile
│   ├── TaskManager.sln
│   ├── src/
│   │   ├── TaskManager.Domain/          # Entities, enums, interfaces
│   │   ├── TaskManager.Application/      # DTOs, validators, services
│   │   ├── TaskManager.Infrastructure/   # EF Core, repositories, auth
│   │   └── TaskManager.Api/             # Controllers, middleware, Program.cs
│   └── tests/
│       └── TaskManager.Tests/           # xUnit + Moq unit tests
└── frontend/
    ├── Dockerfile
    ├── nginx.conf
    ├── package.json
    ├── vite.config.ts
    ├── vitest.config.ts
    ├── tsconfig.json
    └── src/
        ├── api/                         # Axios client & API functions
        ├── auth/                        # Auth context & hooks
        ├── types/                       # TypeScript type definitions
        ├── pages/                       # Route page components
        ├── components/                  # Shared UI components
        └── __tests__/                   # Vitest + React Testing Library tests
```

## API Endpoints

### Auth (Public)
- `POST /api/auth/register` — Register a new user
- `POST /api/auth/login` — Login and receive JWT

### Tasks (Authenticated)
- `GET /api/tasks` — List all tasks (filter by `status`, `priority`, `assignedToId`)
- `POST /api/tasks` — Create a task
- `GET /api/tasks/{id}` — Get task detail with comments
- `PUT /api/tasks/{id}` — Update a task
- `DELETE /api/tasks/{id}` — Delete a task (creator only)

### Comments (Authenticated)
- `POST /api/tasks/{taskId}/comments` — Add a comment
- `PUT /api/tasks/{taskId}/comments/{id}` — Edit own comment
- `DELETE /api/tasks/{taskId}/comments/{id}` — Delete own comment