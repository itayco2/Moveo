#!/bin/bash
# Migration script for Render deployment

echo "Running database migrations..."

# Install EF Core tools if not already installed
dotnet tool install --global dotnet-ef --version 9.0.0

# Run migrations
dotnet ef database update --connection "$ConnectionStrings__DefaultConnection"

echo "Migrations completed!"
