#!/bin/bash
# Entrypoint script: runs EF Core migrations then starts the API.
set -e

echo "Running database migrations..."
./efbundle --connection "$ConnectionStrings__DefaultConnection"

echo "Starting API..."
exec dotnet TaskManager.Api.dll
