#!/bin/bash
set -e

echo "========================================="
echo "  Wonga Auth - Build Script"
echo "========================================="

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m'

step() { echo -e "\n${GREEN}[STEP]${NC} $1"; }
fail() { echo -e "${RED}[FAIL]${NC} $1"; exit 1; }

# 1. Backend - Restore & Build
step "Restoring backend dependencies..."
dotnet restore backend/WongaAuth.sln || fail "Backend restore failed"

step "Building backend..."
dotnet build backend/WongaAuth.sln --no-restore -c Release || fail "Backend build failed"

# 2. Backend - Run unit tests
step "Running unit tests..."
dotnet test backend/tests/WongaAuth.UnitTests/WongaAuth.UnitTests.csproj --no-build -c Release --verbosity normal || fail "Unit tests failed"

# 3. Backend - Run integration tests
step "Running integration tests..."
dotnet test backend/tests/WongaAuth.IntegrationTests/WongaAuth.IntegrationTests.csproj --no-build -c Release --verbosity normal || fail "Integration tests failed"

# 4. Frontend - Install & Build
step "Installing frontend dependencies..."
cd frontend
npm ci || npm install || fail "Frontend install failed"

step "Building frontend..."
npm run build || fail "Frontend build failed"
cd ..

# 5. Docker
step "Building Docker images..."
docker compose build || fail "Docker build failed"

echo ""
echo "========================================="
echo -e "  ${GREEN}Build completed successfully!${NC}"
echo "========================================="
echo ""
echo "Run 'docker compose up' to start the application."
echo "  Frontend: http://localhost:3000"
echo "  Backend:  http://localhost:5000"
echo "  Swagger:  http://localhost:5000/swagger"
