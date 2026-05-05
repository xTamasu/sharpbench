#!/bin/bash
set -e

# Wait for database to be ready using a simple TCP check
echo "Waiting for database..."
while ! nc -z db 5432; do
  sleep 1
done
echo "Database is ready."

# Start the API (migrations run automatically in Program.cs)
echo "Starting API..."
exec dotnet TaskManager.Api.dll
