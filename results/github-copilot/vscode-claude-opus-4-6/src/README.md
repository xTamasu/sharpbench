# Task Management System

A production-ready full-stack task management application built with .NET 8, React 18, and PostgreSQL.

## Overview

This system allows users to register, create tasks, assign them, track status/priority, and collaborate through comments. It features JWT authentication, role-based ownership controls, and a clean responsive UI.

## Prerequisites

- [Docker](https://docs.docker.com/get-docker/) (v20+)
- [Docker Compose](https://docs.docker.com/compose/install/) (v2+)

## How to Run

```bash
docker-compose up
```

After all services start, open your browser at **http://localhost:3000**.

### Demo Account

- **Email:** demo@taskmanager.com
- **Password:** Demo123!

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

## Architecture

```
backend/
├── src/
│   ├── TaskManager.Domain/          # Entities, Enums, Repository interfaces
│   ├── TaskManager.Application/     # DTOs, Services, Validators
│   ├── TaskManager.Infrastructure/  # EF Core DbContext, Repositories, Migrations
│   └── TaskManager.Api/             # Controllers, Middleware, Program.cs
└── tests/
    └── TaskManager.Tests/           # xUnit + Moq unit tests

frontend/
└── src/
    ├── api/           # Axios client and API functions
    ├── components/    # Reusable components (ProtectedRoute, TaskCommentSection)
    ├── pages/         # Route pages (Login, Register, TaskList, TaskDetail)
    ├── schemas/       # Zod validation schemas
    ├── types/         # TypeScript type definitions
    └── __tests__/     # Vitest + RTL tests
```

## Environment Variable Reference

| Variable | Default | Description |
|---|---|---|
| `POSTGRES_DB` | `taskmanager` | PostgreSQL database name |
| `POSTGRES_USER` | `taskuser` | PostgreSQL username |
| `POSTGRES_PASSWORD` | `taskpass123` | PostgreSQL password |
| `JWT_SECRET` | `SuperSecretKeyForDevelopment...` | JWT signing secret (change in production!) |
| `JWT_ISSUER` | `TaskManagerApi` | JWT issuer claim |
| `JWT_AUDIENCE` | `TaskManagerClient` | JWT audience claim |
| `JWT_EXPIRY_MINUTES` | `60` | JWT token expiry in minutes |

## Services

| Service | Port | Description |
|---|---|---|
| `frontend` | 3000 | React SPA served via Nginx |
| `api` | 5000 | .NET 8 Web API |
| `db` | 5432 | PostgreSQL 16 |

## API Endpoints

### Auth (no authentication required)
- `POST /api/auth/register` — Register a new user
- `POST /api/auth/login` — Login and receive JWT

### Tasks (JWT required)
- `GET /api/tasks` — List tasks (filter: `status`, `priority`, `assignedToId`)
- `POST /api/tasks` — Create a task
- `GET /api/tasks/{id}` — Get task with comments
- `PUT /api/tasks/{id}` — Update a task
- `DELETE /api/tasks/{id}` — Delete task (creator only)

### Comments (JWT required)
- `POST /api/tasks/{taskId}/comments` — Add comment
- `PUT /api/tasks/{taskId}/comments/{id}` — Edit own comment
- `DELETE /api/tasks/{taskId}/comments/{id}` — Delete own comment
