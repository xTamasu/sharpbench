# Task Manager System

A production-ready full-stack task management system built with C# .NET 8, React 18, PostgreSQL, and Docker.

## Overview

This is a comprehensive task management application that allows users to:
- Create, read, update, and delete tasks
- Manage task status (Todo, In Progress, Done) and priority (Low, Medium, High)
- Add comments to tasks with full edit/delete capabilities
- Filter tasks by status and priority
- User authentication with JWT tokens

## Architecture

### Backend
- **Framework**: ASP.NET Core 8
- **Database**: PostgreSQL 16
- **ORM**: Entity Framework Core
- **Pattern**: Clean Layered Architecture (Domain → Application → Infrastructure → Api)
- **Authentication**: JWT (24-hour expiry)
- **Password Security**: BCrypt hashing

### Frontend
- **Framework**: React 18 with TypeScript
- **Build Tool**: Vite
- **State Management**: TanStack Query (React Query v5)
- **Forms**: React Hook Form + Zod
- **HTTP Client**: Axios
- **Styling**: Tailwind CSS
- **Routing**: React Router v6

### Infrastructure
- **Containerization**: Docker & Docker Compose
- **Web Server**: Nginx (reverse proxy for frontend)
- **Database**: PostgreSQL with persistent volume

## Prerequisites

- Docker and Docker Compose installed
- (Optional) .NET 8 SDK for local backend development
- (Optional) Node.js 20+ for local frontend development

## Quick Start

### Start the entire system with one command:

```bash
docker-compose up
```

The application will be available at `http://localhost:3000`

### Demo Account

- **Email**: `demo@example.com`
- **Password**: `Demo123!`

## Running Tests

### Backend Tests

```bash
cd backend
dotnet test tests/TaskManager.Tests/TaskManager.Tests.csproj
```

### Frontend Tests

```bash
cd frontend
npm install
npm test
```

## Environment Variables

### Backend (docker-compose.yml)

| Variable | Default | Description |
|----------|---------|-------------|
| `ASPNETCORE_ENVIRONMENT` | Development | ASP.NET environment |
| `ConnectionStrings__DefaultConnection` | (PostgreSQL connection) | Database connection string |
| `JwtSettings__Secret` | (32-char key) | JWT signing secret |
| `JwtSettings__Issuer` | TaskManager | JWT issuer claim |
| `JwtSettings__Audience` | TaskManagerClient | JWT audience claim |
| `JwtSettings__ExpiryMinutes` | 1440 | Token expiry in minutes (24 hours) |

### Frontend (docker-compose.yml)

| Variable | Default | Description |
|----------|---------|-------------|
| `VITE_API_BASE_URL` | http://localhost:5000/api | API base URL |

## API Endpoints

### Authentication
- `POST /api/auth/register` — Register new user
- `POST /api/auth/login` — Login and receive JWT token

### Tasks (all require `Authorization: Bearer <token>`)
- `GET /api/tasks` — List tasks (supports `?status=`, `?priority=`, `?assignedToId=` filters)
- `POST /api/tasks` — Create task
- `GET /api/tasks/{id}` — Get task with comments
- `PUT /api/tasks/{id}` — Update task (creator only)
- `DELETE /api/tasks/{id}` — Delete task (creator only)

### Comments (all require `Authorization: Bearer <token>`)
- `GET /api/tasks/{taskId}/comments` — Get task comments
- `POST /api/tasks/{taskId}/comments` — Add comment
- `PUT /api/tasks/{taskId}/comments/{id}` — Edit comment (author only)
- `DELETE /api/tasks/{taskId}/comments/{id}` — Delete comment (author only)

## Project Structure

```
.
├── docker-compose.yml          # Docker Compose configuration
├── README.md                   # This file
├── backend/
│   ├── Dockerfile              # Backend container image
│   ├── TaskManager.sln         # Solution file
│   ├── src/
│   │   ├── TaskManager.Domain/       # Entities, enums, interfaces
│   │   ├── TaskManager.Application/  # DTOs, Services, Validators
│   │   ├── TaskManager.Infrastructure/ # DbContext, Repositories, Migrations
│   │   └── TaskManager.Api/          # Controllers, Middleware, Program.cs
│   └── tests/
│       └── TaskManager.Tests/  # xUnit tests
└── frontend/
    ├── Dockerfile              # Frontend container image
    ├── nginx.conf              # Nginx reverse proxy config
    ├── vite.config.ts          # Vite configuration
    ├── tailwind.config.js      # Tailwind CSS configuration
    ├── package.json            # Node dependencies
    ├── tsconfig.json           # TypeScript configuration
    └── src/
        ├── pages/              # React pages (Login, Register, TaskList, TaskDetail)
        ├── components/         # React components (ProtectedRoute)
        ├── hooks/              # Custom hooks (useAuth, useTasks, useComments)
        ├── services/           # API client (axios instance)
        ├── types/              # TypeScript type definitions
        ├── tests/              # Vitest unit tests
        ├── App.tsx             # Main app component with routing
        └── main.tsx            # Application entry point
```

## Key Features

### Backend
- ✅ Clean layered architecture with dependency injection
- ✅ Repository pattern for data access
- ✅ Service layer for business logic
- ✅ Global exception handling middleware
- ✅ FluentValidation for request validation
- ✅ JWT authentication with configurable expiry
- ✅ BCrypt password hashing
- ✅ EF Core code-first migrations with seeded demo user
- ✅ Comprehensive xUnit tests covering all service methods

### Frontend
- ✅ Protected routes redirecting to login
- ✅ JWT token automatic attachment to requests
- ✅ React Query for efficient server state management
- ✅ React Hook Form + Zod for robust form validation
- ✅ Fully typed with TypeScript (no `any` types)
- ✅ Tailwind CSS for responsive design
- ✅ Vitest + React Testing Library for component tests

### DevOps
- ✅ Multi-stage Docker builds for optimized images
- ✅ Health checks for service dependencies
- ✅ Environment variable configuration
- ✅ PostgreSQL persistent volume
- ✅ Nginx reverse proxy with API routing
- ✅ Automatic database migration on startup

## Development

### Local Backend Development

```bash
cd backend
dotnet restore
dotnet build
dotnet run --project src/TaskManager.Api
```

### Local Frontend Development

```bash
cd frontend
npm install
npm run dev
```

Frontend dev server runs on `http://localhost:5173` with hot reload.

## Testing

### Backend Test Structure

Tests are organized by service layer and follow the Arrange/Act/Assert pattern:
- `AuthServiceTests` — Login, register, token generation
- `TaskServiceTests` — CRUD operations, ownership verification
- `CommentServiceTests` — Comment management, authorization

Run all tests:
```bash
cd backend
dotnet test
```

### Frontend Test Examples

- `LoginPage.test.tsx` — Form rendering, validation, error handling

Run tests:
```bash
cd frontend
npm test
```

## Deployment Considerations

- JWT secret should be stored in environment variables (never hardcoded)
- Use strong, randomly-generated secrets in production
- Enable HTTPS in production (use reverse proxy like nginx)
- Configure database backups and disaster recovery
- Set up monitoring and logging (e.g., ELK stack)
- Use container orchestration (Kubernetes, Docker Swarm) for scaling
- Implement rate limiting on API endpoints
- Regular security audits and dependency updates

## Assumptions & Decisions

1. **JWT Expiry**: Set to 24 hours. Adjust `JwtSettings__ExpiryMinutes` in docker-compose.yml for different duration.

2. **No Pagination**: The task list endpoint returns all tasks without pagination. For large datasets, implement cursor-based or offset-based pagination.

3. **No Real-time Features**: No WebSocket implementation. Comments and tasks are fetched on demand via React Query.

4. **Single Access Token**: Only access tokens are implemented (no refresh tokens). For production, consider adding refresh token rotation.

5. **Demo User Seeding**: Database migration automatically creates a demo user (`demo@example.com` / `Demo123!`). Remove from migration for production.

6. **CORS Policy**: Backend allows all origins (`AllowAnyOrigin`). Restrict to specific domains in production.

7. **No Role-Based Access**: All authenticated users have equal permissions. Implement role-based authorization (Admin, User) if needed.

8. **Task Assignment**: Tasks can be assigned to any user, but no permission check is performed. Only the creator can update/delete.

9. **Comment Threading**: Comments are flat (no nested replies). Implement threaded comments if required.

10. **Frontend Error Handling**: Basic error messages are displayed. Implement detailed error logging to a service like Sentry for production.

11. **State Management**: React Query manages server state. For complex client state, consider Redux or Zustand.

12. **Validation Consistency**: Backend and frontend use identical constraints (max lengths, formats). Keep them in sync when updating business rules.

13. **Database Constraints**: Deletion of task cascades to comments. Deletion of user is restricted (fails if tasks exist). Adjust cascade rules if needed.

14. **API Response Format**: All errors return JSON with `{ message: "error description" }`. Standardize response envelopes for consistency.

15. **Frontend Build**: Tailwind CSS is purged to production bundle. No UI component library is used; design is intentionally minimal and functional.
