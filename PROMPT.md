# Coding Benchmark: Full-Stack Task Management System — Greenfield

## Overview

Build a production-ready **Task Management System** from scratch. The entire system must start with a single command:

```bash
docker-compose up
```

Do not ask clarifying questions. Make all architectural decisions yourself and document them in an `## Assumptions & Decisions` section at the very end of your response.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Backend API | C# .NET 8, ASP.NET Core, Entity Framework Core |
| Database | PostgreSQL 16 |
| Frontend | React 18, TypeScript, Vite |
| Containerization | Docker, Docker Compose |
| Testing (Backend) | xUnit, Moq |
| Testing (Frontend) | Vitest, React Testing Library |

---

## Functional Requirements

### Entities

**User**
- `Id` (UUID)
- `Email` (unique)
- `PasswordHash`
- `DisplayName`
- `CreatedAt`

**Task**
- `Id` (UUID)
- `Title` (max 200 chars)
- `Description` (max 5000 chars, optional)
- `Status` (enum: `Todo`, `InProgress`, `Done`)
- `Priority` (enum: `Low`, `Medium`, `High`)
- `DueDate` (optional)
- `AssignedToId` (FK → User, optional)
- `CreatedById` (FK → User)
- `CreatedAt`, `UpdatedAt`

**TaskComment**
- `Id` (UUID)
- `TaskId` (FK → Task)
- `AuthorId` (FK → User)
- `Body` (max 2000 chars)
- `EditedAt` (nullable)
- `CreatedAt`

---

## API Endpoints

### Auth
| Method | Route | Description |
|---|---|---|
| POST | `/api/auth/register` | Register new user |
| POST | `/api/auth/login` | Login, returns JWT |

### Tasks
| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/tasks` | ✅ | List all tasks (support filter by `status`, `priority`, `assignedToId`) |
| POST | `/api/tasks` | ✅ | Create task |
| GET | `/api/tasks/{id}` | ✅ | Get single task with comments |
| PUT | `/api/tasks/{id}` | ✅ | Update task |
| DELETE | `/api/tasks/{id}` | ✅ | Delete task (only creator) |

### Comments
| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/api/tasks/{taskId}/comments` | ✅ | Add comment |
| PUT | `/api/tasks/{taskId}/comments/{id}` | ✅ | Edit own comment |
| DELETE | `/api/tasks/{taskId}/comments/{id}` | ✅ | Delete own comment |

---

## Backend Requirements

### Architecture
- Clean layered architecture: `Domain` → `Application` → `Infrastructure` → `Api`
- Repository pattern with a generic `IRepository<T>` and entity-specific extensions where needed
- Service layer for all business logic
- DTOs for all request/response shapes with **FluentValidation**
- Global exception handling middleware (maps exceptions to correct HTTP status codes)
- JWT authentication (access token only, configurable secret/expiry via `appsettings`)
- Passwords hashed with BCrypt
- EF Core code-first with an initial migration that seeds one demo user

### Business Rules
- Only the task creator can delete a task
- Only the comment author can edit or delete their comment
- Editing a comment sets `EditedAt` to the current UTC time
- Task `UpdatedAt` is always set on modification

### Unit Tests (xUnit + Moq)
Write unit tests for **all service methods**. Tests must cover:
- Happy path
- Unauthorized access attempts (wrong owner)
- Not-found cases
- Validation edge cases (e.g. empty body, status transitions)

Organize tests mirroring the service structure. Use the Arrange / Act / Assert pattern with comments.

---

## Frontend Requirements

### Pages & Features
- `/login` — Login form
- `/register` — Register form
- `/tasks` — Task list with filter bar (by status, priority)
- `/tasks/:id` — Task detail view with comment section

### Technical Requirements
- React Query (TanStack Query v5) for all server state
- Axios with a centralized `apiClient` (attaches JWT from localStorage automatically)
- React Hook Form + Zod for all forms
- Tailwind CSS for styling — clean, functional UI (no component library)
- Protected routes: redirect to `/login` if not authenticated
- Full TypeScript — no `any`

### Frontend Tests (Vitest + React Testing Library)
Write tests for:
- `LoginPage` — renders form, shows error on failed login
- `TaskList` — renders tasks, filters work
- `TaskCommentSection` — renders comments, submit adds a comment, own comments show edit/delete

---

## Docker Requirements

### Services in `docker-compose.yml`
- `db` — PostgreSQL 16, data persisted via named volume
- `api` — .NET 8 API, depends on `db`, runs migrations on startup
- `frontend` — React app served via Nginx

### Rules
- No hardcoded secrets — use environment variables with sane defaults in `docker-compose.yml`
- The API must wait for the database to be ready before starting (use a health check or retry logic)
- Frontend Nginx config must proxy `/api` requests to the backend container (no CORS issues)
- After `docker-compose up`, the app must be reachable at `http://localhost:3000`

---

## Documentation Requirements

- Every file must have a brief file-level comment explaining its purpose
- Non-obvious logic must have inline comments (auth flow, ownership checks, query invalidation)
- `README.md` at the root with:
  - Project overview
  - Prerequisites
  - How to run (`docker-compose up`)
  - How to run tests (backend + frontend separately)
  - Environment variable reference table

---

## Deliverables — Complete File Tree

Produce **every file** needed to run the system. The expected structure is roughly:

```
/
├── docker-compose.yml
├── README.md
├── backend/
│   ├── Dockerfile
│   ├── TaskManager.sln
│   ├── src/
│   │   ├── TaskManager.Domain/
│   │   ├── TaskManager.Application/
│   │   ├── TaskManager.Infrastructure/
│   │   └── TaskManager.Api/
│   └── tests/
│       └── TaskManager.Tests/
└── frontend/
    ├── Dockerfile
    ├── nginx.conf
    ├── package.json
    ├── tsconfig.json
    ├── vite.config.ts
    └── src/
```

Do not omit files with "similar to above" or "same pattern as X". Every file must be fully written out.

---

## Assumptions & Decisions

At the very end of your response, include a section listing every decision you made that wasn't explicitly specified (e.g. JWT expiry duration, pagination strategy, Nginx port bindings, test coverage scope, etc.).

---

## ✅ Pre-Submission Verification (Mandatory)

Before your response is complete, you must perform a self-review pass. Go through every item below, check it explicitly, and fix any issues you find — then print the checklist with your results at the very end of your response.

Do not mark an item as ✅ unless you are certain it is correct.

### Docker & Infrastructure
- [ ] `docker-compose.yml` defines all three services: `db`, `api`, `frontend`
- [ ] The `db` service has a health check so the API waits for Postgres to be ready
- [ ] The `api` service has `depends_on` with `condition: service_healthy` on `db`
- [ ] The API `Dockerfile` runs EF Core migrations on startup before the app starts
- [ ] The Nginx config proxies `/api/` to the backend container — verify the `proxy_pass` URL uses the correct container name and port
- [ ] All environment variables referenced in code exist in `docker-compose.yml`
- [ ] No service binds to a port already used by another service
- [ ] After `docker-compose up`, the app is reachable at `http://localhost:3000`

### Backend
- [ ] The `.sln` file references all projects
- [ ] Every project is referenced correctly in its `.csproj` (no missing NuGet packages)
- [ ] `AppDbContext` includes `DbSet<>` for every entity
- [ ] The EF Core migration `Up()` matches the entity definitions exactly (column names, types, FKs, nullability)
- [ ] Every service is registered in `Program.cs` DI container
- [ ] JWT secret, issuer, and audience are read from config — not hardcoded in logic
- [ ] All controllers have `[Authorize]` where required
- [ ] Global exception handler maps `UnauthorizedAccessException` → 403, `KeyNotFoundException` → 404

### Frontend
- [ ] `package.json` includes all imported packages (`react-query`, `axios`, `react-hook-form`, `zod`, `tailwindcss`, etc.)
- [ ] `apiClient.ts` attaches the JWT token from localStorage to every request
- [ ] All React Query mutations call `invalidateQueries` after success
- [ ] Protected routes redirect to `/login` when no token is present
- [ ] Zod schemas enforce the same max-length constraints as the backend (`body` max 2000 chars, `title` max 200 chars)
- [ ] No `any` types — verify each file

### Tests
- [ ] Backend test project references the application projects correctly
- [ ] Every service method has at least one test
- [ ] Ownership violation cases are tested (wrong user tries to edit/delete)
- [ ] Frontend tests mock `apiClient` — they do not make real HTTP calls

### Final Check
- [ ] `README.md` explains how to run `docker-compose up` and how to run tests
- [ ] `## Assumptions & Decisions` section is present and complete
- [ ] No file says "same as above", "similar pattern", or is left as a placeholder