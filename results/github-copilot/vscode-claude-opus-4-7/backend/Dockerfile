# backend/Dockerfile
# Multi-stage build for the ASP.NET Core 8 API.
# Stage 1: restore + publish using the .NET SDK image.
# Stage 2: minimal ASP.NET runtime image; on startup we apply EF Core
#          migrations (idempotent) and then launch the API.

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first to leverage Docker layer caching for restore.
COPY TaskManager.sln ./
COPY src/TaskManager.Domain/TaskManager.Domain.csproj          src/TaskManager.Domain/
COPY src/TaskManager.Application/TaskManager.Application.csproj src/TaskManager.Application/
COPY src/TaskManager.Infrastructure/TaskManager.Infrastructure.csproj src/TaskManager.Infrastructure/
COPY src/TaskManager.Api/TaskManager.Api.csproj                 src/TaskManager.Api/
COPY tests/TaskManager.Tests/TaskManager.Tests.csproj           tests/TaskManager.Tests/

RUN dotnet restore TaskManager.sln

# Copy the rest of the source and publish only the API project.
COPY . .
RUN dotnet publish src/TaskManager.Api/TaskManager.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Migrations are applied at runtime by the API itself (see Program.cs)
# so we don't need a separate entrypoint script.
ENTRYPOINT ["dotnet", "TaskManager.Api.dll"]
