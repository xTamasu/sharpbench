# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and projects
COPY ["TaskManager.sln", "./"]
COPY ["src/TaskManager.Domain/TaskManager.Domain.csproj", "src/TaskManager.Domain/"]
COPY ["src/TaskManager.Application/TaskManager.Application.csproj", "src/TaskManager.Application/"]
COPY ["src/TaskManager.Infrastructure/TaskManager.Infrastructure.csproj", "src/TaskManager.Infrastructure/"]
COPY ["src/TaskManager.Api/TaskManager.Api.csproj", "src/TaskManager.Api/"]
COPY ["tests/TaskManager.Tests/TaskManager.Tests.csproj", "tests/TaskManager.Tests/"]

# Restore
RUN dotnet restore

# Copy source code
COPY ["src/", "src/"]
COPY ["tests/", "tests/"]

# Build
RUN dotnet build -c Release --no-restore

# Publish
RUN dotnet publish "src/TaskManager.Api/TaskManager.Api.csproj" -c Release -o /app/publish --no-build

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

# Wait for database and run migrations
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

EXPOSE 8080
ENTRYPOINT ["dotnet", "TaskManager.Api.dll"]
