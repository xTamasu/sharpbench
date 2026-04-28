# Full-Stack Task Management System

## Overview
A production-ready Task Management System built with .NET 8 (C#) and React 18 (TypeScript, Vite).

## Prerequisites
- Docker and Docker Compose

## How to Run
Run the entire stack with a single command:
```bash
docker-compose up --build
```
The application will be available at:
- Frontend: `http://localhost:3000`
- Backend API: `http://localhost:8080` (Internal Docker network routes via Nginx)

## How to Run Tests

### Backend Tests
Navigate to the backend tests directory and run `dotnet test`:
```bash
cd backend
dotnet test
```

### Frontend Tests
Navigate to the frontend directory and run `npm test`:
```bash
cd frontend
npm install
npm test
```

## Environment Variable Reference
| Variable | Description | Default |
|---|---|---|
| `POSTGRES_USER` | Database user | `taskuser` |
| `POSTGRES_PASSWORD` | Database password | `taskpassword` |
| `POSTGRES_DB` | Database name | `taskmanagerdb` |
| `ConnectionStrings__DefaultConnection` | Backend DB connection | `Host=db;Database=taskmanagerdb;Username=taskuser;Password=taskpassword` |
| `JwtSettings__Secret` | JWT secret | (Provided in docker-compose.yml) |
| `JwtSettings__Issuer` | JWT issuer | `TaskManagerApi` |
| `JwtSettings__Audience` | JWT audience | `TaskManagerClients` |
| `JwtSettings__ExpiryMinutes` | JWT expiration | `60` |
