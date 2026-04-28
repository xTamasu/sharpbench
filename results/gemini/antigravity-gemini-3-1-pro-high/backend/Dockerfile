FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["TaskManager.sln", "./"]
COPY ["src/TaskManager.Domain/TaskManager.Domain.csproj", "src/TaskManager.Domain/"]
COPY ["src/TaskManager.Application/TaskManager.Application.csproj", "src/TaskManager.Application/"]
COPY ["src/TaskManager.Infrastructure/TaskManager.Infrastructure.csproj", "src/TaskManager.Infrastructure/"]
COPY ["src/TaskManager.Api/TaskManager.Api.csproj", "src/TaskManager.Api/"]
COPY ["tests/TaskManager.Tests/TaskManager.Tests.csproj", "tests/TaskManager.Tests/"]

RUN dotnet restore

# Copy everything else and build
COPY . .
WORKDIR "/src/src/TaskManager.Api"
RUN dotnet build "TaskManager.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TaskManager.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Install EF Core CLI tools for migrations
FROM build AS ef
RUN dotnet tool install --global dotnet-ef --version 8.0.2
ENV PATH="$PATH:/root/.dotnet/tools"

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "TaskManager.Api.dll"]
