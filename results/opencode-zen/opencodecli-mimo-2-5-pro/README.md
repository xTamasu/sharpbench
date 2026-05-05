# Task Management System

A production-ready full-stack Task Management System built with .NET 8, React 18, PostgreSQL 16, and Docker.

## Overview

This application provides a complete task management solution with user authentication, task CRUD operations, commenting system, and filtering capabilities. The entire system starts with a single command using Docker Compose.

## Prerequisites

- Docker and Docker Compose installed
- No local .NET or Node.js installation required (everything runs in containers)

## Quick Start

```bash
# Clone the repository and navigate to the project root
docker-compose up
```

The application will be available at:
- **Frontend**: http://localhost:3000
- **API**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger

## Demo Credentials

After startup, a demo user is seeded automatically:
- **Email**: demo@taskmanager.com
- **Password**: Demo123!

## Environment Variables

| Variable | Default | Description |
|---|---|---|
| `POSTGRES_USER` | `taskmanager` | PostgreSQL username |
| `POSTGRES_PASSWORD` | `taskmanager_pass` | PostgreSQL password |
| `POSTGRES_DB` | `taskmanager` | PostgreSQL database name |
| `JWT_SECRET` | `SuperSecretKeyForJwtTokensAtLeast32Chars!` | JWT signing secret (min 32 chars) |
| `JWT_ISSUER` | `TaskManager` | JWT issuer claim |
| `JWT_AUDIENCE` | `TaskManagerApp` | JWT audience claim |
| `JWT_EXPIRY_MINUTES` | `60` | JWT token expiry in minutes |

## Architecture

### Backend (C# .NET 8)

- **Domain Layer**: Entity definitions, enums, repository interfaces
- **Application Layer**: Business logic services, DTOs, FluentValidation validators
- **Infrastructure Layer**: EF Core DbContext, repository implementations, migrations
- **API Layer**: Controllers, JWT authentication, global exception handling middleware

### Frontend (React 18 + TypeScript)

- **Vite** for fast development and builds
- **TanStack Query v5** for server state management
- **React Hook Form + Zod** for form handling and validation
- **Tailwind CSS** for styling
- **Axios** with centralized API client and JWT auto-attachment

### Docker Services

- **db**: PostgreSQL 16 with health check
- **api**: .NET 8 API (waits for db health check, runs migrations on startup)
- **frontend**: React app served via Nginx with `/api` proxy to backend

## Running Tests

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

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login, returns JWT

### Tasks (requires authentication)
- `GET /api/tasks` - List tasks (filter by `status`, `priority`, `assignedToId`)
- `POST /api/tasks` - Create task
- `GET /api/tasks/{id}` - Get task with comments
- `PUT /api/tasks/{id}` - Update task
- `DELETE /api/tasks/{id}` - Delete task (creator only)

### Comments (requires authentication)
- `POST /api/tasks/{taskId}/comments` - Add comment
- `PUT /api/tasks/{taskId}/comments/{id}` - Edit own comment
- `DELETE /api/tasks/{taskId}/comments/{id}` - Delete own comment

## Business Rules

- Only the task creator can delete a task
- Only the comment author can edit or delete their comment
- Editing a comment sets `EditedAt` to the current UTC time
- Task `UpdatedAt` is always set on modification
- Passwords are hashed with BCrypt
- JWT tokens are used for authentication (configurable expiry)

## Project Structure

```
/
├── docker-compose.yml
├── README.md
├── backend/
│   ├── Dockerfile
│   ├── TaskManager.sln
│   ├── src/
│   │   ├── TaskManager.Domain/
│   │   │   ├── Entities/
│   │   │   ├── Enums/
│   │   │   └── Interfaces/
│   │   ├── TaskManager.Application/
│   │   │   ├── DTOs/
│   │   │   ├── Interfaces/
│   │   │   ├── Services/
│   │   │   └── Validators/
│   │   ├── TaskManager.Infrastructure/
│   │   │   ├── Data/
│   │   │   ├── Migrations/
│   │   │   └── Repositories/
│   │   └── TaskManager.Api/
│   │       ├── Controllers/
│   │       └── Middleware/
│   └── tests/
│       └── TaskManager.Tests/
└── frontend/
    ├── Dockerfile
    ├── nginx.conf
    ├── package.json
    └── src/
        ├── api/
        ├── components/
        ├── hooks/
        ├── pages/
        ├── types/
        └── __tests__/
```
