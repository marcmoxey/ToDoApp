# TodoApp

A full-stack todo application built as a learning project to explore modern web development technologies across the entire stack  from backend API to frontend UI, containerisation, cloud hosting, and automated deployments.

---

## What I Learned Building This

This project was built specifically to gain hands-on experience with:

- **.NET 6 Web API** — building a RESTful API with controllers, services, dependency injection, and structured logging
- **Entity Framework Core** — code-first database design, migrations, and querying with Npgsql
- **PostgreSQL** — hosted on Supabase, switching from SQL Server and understanding the differences
- **React + TypeScript** — building a frontend with hooks, routing, component architecture, and type safety
- **Supabase** — JWT-based authentication and managed PostgreSQL database
- **Docker** — containerising both the API and frontend, multi-stage builds, and Docker Compose for local development
- **Azure** — deploying containers to Azure Container Apps
- **GitHub Actions** — CI/CD pipelines that run tests and deploy automatically on push to main
- **xUnit + Moq** — unit testing the service layer with EF Core InMemory and the controller layer with mocked services

---

## Tech Stack

| Layer | Technology |
|---|---|
| Backend API | .NET 6 Web API |
| ORM | Entity Framework Core 6 |
| Database | PostgreSQL (Supabase) |
| Authentication | Supabase Auth (JWT) |
| Frontend | React 18 + TypeScript (Vite) |
| Containerisation | Docker + Docker Compose |
| Cloud Hosting | Azure Container Apps |
| CI/CD | GitHub Actions |
| Testing | xUnit, Moq, EF Core InMemory |

---

## Project Structure

```
TodoApp/
├── TodoAPI/                  # .NET 6 Web API
│   ├── Controllers/          # TodoController
│   ├── Extensions/           # Service registration extensions
│   └── Dockerfile
├── TodoLibrary/              # Class library (shared logic)
│   ├── Contracts/            # Request/response models
│   ├── DataAccess/           # TodoContext (EF Core DbContext)
│   ├── Models/               # TodoModel entity
│   └── Services/             # TodoService + ITodoService
├── TodoAPI.Test/             # Unit tests
│   ├── Controllers/          # TodoControllerTests (Moq)
│   └── Services/             # TodoServiceTests (EF InMemory)
├── frontend/                 # React + TypeScript
│   ├── src/
│   │   ├── components/       # Navbar, TodoList, TodoItem, CreateTodoForm
│   │   ├── pages/            # Login, Dashboard, EditTodo
│   │   ├── types/            # Shared TypeScript interfaces
│   │   └── supabaseClient.ts # Supabase client setup
│   ├── nginx.conf            # Nginx config for routing + API proxy
│   └── Dockerfile
├── docker-compose.yml        # Local development setup
└── .github/
    └── workflows/
        ├── deploy-api.yml        # API CI/CD pipeline
        ├── deploy-frontend.yml   # Frontend CI/CD pipeline
        └── pr-check.yml          # PR validation
```

---

## Features

- **Authentication** — login with email and password via Supabase Auth
- **Create todos** — add new tasks from the dashboard
- **View todos** — see all todos assigned to the logged-in user
- **Complete todos** — mark a todo as done with a checkbox
- **Edit todos** — navigate to a separate edit page with the task pre-populated
- **Delete todos** — remove a todo permanently
- **Protected routes** — unauthenticated users are redirected to login
- **Welcome message** — displays the logged-in user's email on the dashboard

---

## API Endpoints

All endpoints require a valid Supabase JWT token in the `Authorization: Bearer` header.

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/todo` | Create a new todo |
| `GET` | `/api/todo` | Get all todos for the logged-in user |
| `GET` | `/api/todo/{id}` | Get a single todo |
| `PUT` | `/api/todo/{id}` | Update a todo's task |
| `PUT` | `/api/todo/{id}/complete` | Mark a todo as complete |
| `DELETE` | `/api/todo/{id}` | Delete a todo |
| `GET` | `/health` | Health check |

---

## Getting Started

### Prerequisites

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- [Node.js 20+](https://nodejs.org)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- A [Supabase](https://supabase.com) project

### 1. Clone the repo

```bash
git clone https://github.com/marcmoxey/TodoApp.git
cd TodoApp
```

### 2. Set up environment variables

Create a `.env` file at the repo root:

```env
SUPABASE_URL=db.your-project.supabase.co
DB_PASSWORD=your-supabase-db-password
```

Create a `frontend/.env` file:

```env
VITE_SUPABASE_URL=https://your-project.supabase.co
VITE_SUPABASE_KEY=your-supabase-anon-key
```

### 3. Run database migrations

```bash
dotnet ef database update --project TodoLibrary --startup-project TodoAPI
```

### 4. Run with Docker Compose

```bash
docker compose up --build
```

| Service | URL |
|---|---|
| Frontend | http://localhost:3000 |
| API Swagger | http://localhost:8080/swagger |

### 5. Run frontend locally with hot reload

```bash
cd frontend
npm install
npm run dev
```

Visit `http://localhost:5173` — API calls are proxied to `localhost:8080` via Vite's dev server proxy.

---

## Running Tests

```bash
dotnet test
```

The test suite covers:

- **TodoServiceTests** — 28 tests using EF Core InMemory. Tests all CRUD operations and verifies users can only access their own todos.
- **TodoControllerTests** — 18 tests using Moq. Tests HTTP response codes and verifies the controller correctly passes data to the service.

---

## CI/CD

Three GitHub Actions workflows run automatically:

| Workflow | Trigger | What it does |
|---|---|---|
| `pr-check.yml` | Pull request to `main` | Builds the solution and runs all tests |
| `deploy-api.yml` | Push to `main` (API files changed) | Runs tests → builds Docker image → pushes to ACR → deploys to Container Apps |
| `deploy-frontend.yml` | Push to `main` (frontend files changed) | Builds and deploys frontend container |

Deployments only happen if all tests pass.

---

## Architecture

```
Browser
  │
  ▼
Azure Container Apps
  ├── frontend (Nginx)
  │     ├── serves React app
  │     └── proxies /api/* → dotnet-api
  │
  └── dotnet-api (.NET 6)
        └── connects to Supabase PostgreSQL

External Services
  └── Supabase
        ├── Auth (JWT tokens)
        └── PostgreSQL database
```

---

## Key Decisions & Things I Learned

**Why Nginx proxy instead of CORS?**
Using Nginx to proxy `/api` requests means the browser only ever talks to one origin so CORS configuration is not needed in the .NET API. This is cleaner than managing allowed origins across two places.

**Why EF InMemory for service tests and Moq for controller tests?**
The service tests use EF InMemory because the database logic is exactly what is being tested  mocking DbSet with Moq is notoriously painful. The controller tests use Moq because data access is irrelevant at that layer only the HTTP response behaviour matters.

**Why Supabase for both auth and the database?**
Since Supabase was already being used for JWT authentication, using its built-in PostgreSQL database eliminated the need to host a separate Postgres container in Azure, reducing hosting costs significantly.

**Why Docker Compose for local dev but not a local database?**
The database lives in Supabase in both local and production environments. This means local and production are identical in terms of data access  no surprises when deploying because the same database is used throughout development.

---

## License

MIT
