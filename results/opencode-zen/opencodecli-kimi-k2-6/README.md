# Task Manager

A full-stack Task Management System built with .NET 8, PostgreSQL, React 18, and Docker.

## Project Overview

This application provides a complete task management solution with:

- **User Authentication**: JWT-based auth with registration and login
- **Task Management**: Create, read, update, and delete tasks with filtering
- **Comments**: Add, edit, and delete comments on tasks
- **Role-Based Access**: Only task creators can delete tasks; only comment authors can edit/delete their comments

## Prerequisites

- [Docker](https://docs.docker.com/get-docker/) (v20.10+)
- [Docker Compose](https://docs.docker.com/compose/install/) (v2.0+)

## How to Run

Start the entire stack with a single command:

```bash
docker-compose up
```

The application will be available at:

- **Frontend**: http://localhost:3000
- **Backend API**: http://localhost:5000
- **API Documentation (Swagger)**: http://localhost:5000/swagger

### Demo Account

A demo user is seeded on first startup:

- **Email**: `demo@example.com`
- **Password**: `demo123`

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

## Project Structure

```
/
├── docker-compose.yml          # Docker Compose configuration
├── README.md                   # This file
├── backend/                    # .NET 8 Backend
│   ├── Dockerfile
│   ├── TaskManager.sln
│   ├── entrypoint.sh
│   └── src/
│       ├── TaskManager.Domain/         # Entities, Enums, Interfaces
│       ├── TaskManager.Application/    # DTOs, Services, Validators
│       ├── TaskManager.Infrastructure/ # EF Core, Repositories, Migrations
│       └── TaskManager.Api/            # Controllers, Middleware, Program.cs
│   └── tests/
│       └── TaskManager.Tests/          # xUnit + Moq unit tests
└── frontend/                   # React 18 + Vite Frontend
    ├── Dockerfile
    ├── nginx.conf
    ├── package.json
    ├── vite.config.ts
    └── src/
        ├── api/                # Axios client and API services
        ├── components/         # Reusable UI components
        ├── hooks/              # React Query hooks and auth helpers
        ├── pages/              # Page-level components
        ├── types/              # TypeScript type definitions
        └── tests/              # Vitest + React Testing Library tests
```

## Environment Variables

The following environment variables can be configured via `docker-compose.yml`:

| Variable | Default | Description |
|---|---|---|
| `POSTGRES_DB` | `taskmanager` | PostgreSQL database name |
| `POSTGRES_USER` | `postgres` | PostgreSQL username |
| `POSTGRES_PASSWORD` | `postgres` | PostgreSQL password |
| `ASPNETCORE_ENVIRONMENT` | `Production` | .NET environment |
| `JWT_SECRET` | `your-super-secret-jwt-key...` | JWT signing secret |
| `JWT_ISSUER` | `TaskManagerApi` | JWT issuer |
| `JWT_AUDIENCE` | `TaskManagerClient` | JWT audience |
| `JWT_EXPIRY_MINUTES` | `60` | JWT token expiry time |

## Architecture

### Backend

The backend follows Clean Architecture with four layers:

1. **Domain**: Contains entities, enums, and repository interfaces
2. **Application**: Contains DTOs, service interfaces, service implementations, and FluentValidation validators
3. **Infrastructure**: Contains EF Core DbContext, generic repository implementation, and migrations
4. **Api**: Contains ASP.NET Core controllers, middleware, and application configuration

### Frontend

The frontend uses modern React patterns:

- **React Query**: Server state management with automatic caching and invalidation
- **React Hook Form + Zod**: Type-safe form handling and validation
- **Tailwind CSS**: Utility-first styling
- **React Router**: Client-side routing with protected routes

## API Endpoints

### Authentication

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/auth/register` | Register a new user |
| POST | `/api/auth/login` | Login and receive JWT |

### Tasks

| Method | Endpoint | Auth Required | Description |
|---|---|---|---|
| GET | `/api/tasks` | Yes | List tasks (with optional filters) |
| POST | `/api/tasks` | Yes | Create a new task |
| GET | `/api/tasks/{id}` | Yes | Get task details with comments |
| PUT | `/api/tasks/{id}` | Yes | Update a task |
| DELETE | `/api/tasks/{id}` | Yes | Delete a task (creator only) |

### Comments

| Method | Endpoint | Auth Required | Description |
|---|---|---|---|
| POST | `/api/tasks/{taskId}/comments` | Yes | Add a comment |
| PUT | `/api/tasks/{taskId}/comments/{id}` | Yes | Edit own comment |
| DELETE | `/api/tasks/{taskId}/comments/{id}` | Yes | Delete own comment |
