# Wonga Developer Assessment - Authentication System

A full-stack user authentication application built with **React**, **C# ASP.NET Core**, **PostgreSQL**, and **Docker**.

## Architecture

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│   Frontend   │────▶│   Backend    │────▶│  PostgreSQL   │
│  React + TS  │     │  ASP.NET 8   │     │    16-alpine  │
│  (port 3000) │     │  (port 5000) │     │  (port 5432)  │
└──────────────┘     └──────────────┘     └──────────────┘
     nginx              JWT Auth            EF Core
   SPA + proxy        BCrypt hash          Auto-migrate
```

## Features

- **User Registration** with validation (first name, last name, email, password)
- **User Login** with JWT token-based authentication
- **Protected User Details Page** accessible only to authenticated users
- **Password Hashing** using BCrypt
- **Auto Database Migration** on API startup
- **Dockerized** entire stack with Docker Compose

## Tech Stack

| Layer     | Technology                          |
|-----------|-------------------------------------|
| Frontend  | React 18, TypeScript, Vite, Axios   |
| Backend   | ASP.NET Core 8, Entity Framework Core |
| Database  | PostgreSQL 16                       |
| Auth      | JWT Bearer Tokens, BCrypt           |
| Container | Docker, Docker Compose              |
| Testing   | xUnit, Moq, WebApplicationFactory  |

## Prerequisites

- [Docker Desktop](https://docs.docker.com/get-started/get-docker/) installed and running

## Quick Start

### Option 1: Docker Compose (Recommended)

```bash
# Clone the repository
git clone <repository-url>
cd wonga-assessment

# Start all services
docker compose up --build
```

That's it! The application will be available at:

| Service  | URL                            |
|----------|--------------------------------|
| Frontend | http://localhost:3000           |
| Backend  | http://localhost:5000           |
| Swagger  | http://localhost:5000/swagger   |

### Option 2: Run Locally (Development)

**Backend:**
```bash
# Ensure PostgreSQL is running on localhost:5432
cd backend
dotnet restore
dotnet run --project src/WongaAuth.Api
```

**Frontend:**
```bash
cd frontend
npm install
npm run dev
```

## API Endpoints

| Method | Endpoint            | Auth     | Description         |
|--------|---------------------|----------|---------------------|
| POST   | `/api/auth/register`| Public   | Register a new user |
| POST   | `/api/auth/login`   | Public   | Login and get JWT   |
| GET    | `/api/auth/me`      | Required | Get user details    |

### Register
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@example.com",
    "password": "Password123"
  }'
```

### Login
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com",
    "password": "Password123"
  }'
```

### Get User Details (Protected)
```bash
curl http://localhost:5000/api/auth/me \
  -H "Authorization: Bearer <your-jwt-token>"
```

## Running Tests

### Unit Tests
```bash
dotnet test backend/tests/WongaAuth.UnitTests
```

### Integration Tests
```bash
dotnet test backend/tests/WongaAuth.IntegrationTests
```

### All Tests
```bash
dotnet test backend/WongaAuth.sln
```

## Build Script

A build script is included that runs the full CI pipeline locally:

```bash
chmod +x build.sh
./build.sh
```

This will:
1. Restore and build the backend
2. Run unit tests
3. Run integration tests
4. Install and build the frontend
5. Build Docker images

## Project Structure

```
wonga-assessment/
├── docker-compose.yml          # Orchestrates all services
├── build.sh                    # CI build script
├── README.md
├── backend/
│   ├── Dockerfile
│   ├── WongaAuth.sln
│   ├── src/WongaAuth.Api/
│   │   ├── Controllers/        # API endpoints
│   │   ├── Models/             # Entity & DTOs
│   │   ├── Services/           # Business logic
│   │   ├── Data/               # EF Core DbContext
│   │   └── Program.cs          # App configuration
│   └── tests/
│       ├── WongaAuth.UnitTests/
│       └── WongaAuth.IntegrationTests/
└── frontend/
    ├── Dockerfile
    ├── nginx.conf
    └── src/
        ├── api/                # API client
        ├── context/            # Auth state management
        ├── components/         # Protected route
        └── pages/              # Login, Register, User Details
```

## How It Works

1. **Registration**: User fills out the form → Frontend sends POST to `/api/auth/register` → Backend validates input, hashes password with BCrypt, stores user in PostgreSQL, and returns a JWT token → Frontend stores token and redirects to user details page.

2. **Login**: User enters credentials → Frontend sends POST to `/api/auth/login` → Backend verifies email/password against the database → Returns JWT token on success → Frontend stores token and redirects.

3. **User Details**: Frontend attaches JWT token via Authorization header → Backend validates the token, extracts user ID → Returns user details from the database → Frontend displays the information.

4. **Route Protection**: The frontend `ProtectedRoute` component checks authentication state. Unauthenticated users are redirected to the login page. The JWT token is persisted in localStorage for session continuity.
