#!/bin/bash
set -e

cd backend
cd src/TaskManager.Infrastructure
dotnet add package Microsoft.EntityFrameworkCore -v 8.0.2
dotnet add package Microsoft.EntityFrameworkCore.Design -v 8.0.2
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL -v 8.0.2
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer -v 8.0.2
cd ../TaskManager.Api
dotnet add package Microsoft.EntityFrameworkCore.Design -v 8.0.2
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer -v 8.0.2
cd ../../tests/TaskManager.Tests
dotnet add package Moq
cd ../..

rm src/TaskManager.Domain/Class1.cs || true
rm src/TaskManager.Application/Class1.cs || true
rm src/TaskManager.Infrastructure/Class1.cs || true
rm src/TaskManager.Api/WeatherForecast.cs src/TaskManager.Api/Controllers/WeatherForecastController.cs || true

cd ../frontend
npm create vite@latest . -- --template react-ts -y
npm install axios @tanstack/react-query react-hook-form @hookform/resolvers zod react-router-dom lucide-react
npm install -D tailwindcss postcss autoprefixer
npx tailwindcss init -p

echo "Setup complete"
