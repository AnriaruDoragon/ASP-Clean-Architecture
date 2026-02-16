# Getting Started

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Task](https://taskfile.dev/) (task runner)
- [mkcert](https://github.com/FiloSottile/mkcert) *(optional, for local HTTPS)*

## Using as Template

### Installation

```bash
git clone https://github.com/AnriaruDoragon/ASP-Clean-Architecture
dotnet new install ./ASPCleanArchitecture
```

### Create a New Project

```bash
# Default: all features included (with auto-setup)
dotnet new aspclean -n MyApp --allow-scripts yes

# Without auto-setup (manual post-creation steps)
dotnet new aspclean -n MyApp

# Without Docker files
dotnet new aspclean -n MyApp --IncludeDocker false --allow-scripts yes

# Without example Product feature
dotnet new aspclean -n MyApp --IncludeExamples false --allow-scripts yes

# Minimal: no examples, no docker, no tests
dotnet new aspclean -n MyApp --IncludeExamples false --IncludeDocker false --IncludeTests false --allow-scripts yes
```

> **Note:** `--allow-scripts yes` automatically runs post-creation scripts (restore, copy .env, create migration).
> Without it, you'll be prompted to confirm each action or can run them manually.

### Template Parameters

| Parameter           | Default | Description                   |
|---------------------|---------|-------------------------------|
| `--IncludeExamples` | `true`  | Include Products CRUD example |
| `--IncludeDocker`   | `true`  | Include Docker/compose files  |
| `--IncludeTests`    | `true`  | Include test projects         |

### After Creating a Project

We recommend installing [Taskfile](https://taskfile.dev/docs/installation) to use predefined tasks.

With `--allow-scripts yes`, steps 1-3 run automatically.

```bash
cd MyApp

# 1. Setup environment (auto)
cp .env.example .env

# 2. Restore packages (auto)
task restore

# 3. Create initial migration (auto)
task migration:add -- Init

# 4. (Optional) Setup local HTTPS
task certs:setup:windows  # Windows (run as Administrator)
task certs:setup          # Linux/Mac (uses sudo)

# 5. Start development
task docker:up
```

## Development Setup

### First-Time Setup

1. **Copy environment file and restore tools:**
   ```bash
   cp .env.example .env
   dotnet tool restore
   ```

2. **(Optional) Local HTTPS domain setup:**
   ```bash
   # Generate certificates and configure hosts (requires mkcert)
   # Run as admin/sudo to auto-add hosts entry
   task certs:setup:windows  # Windows (run as Administrator)
   task certs:setup          # Linux/Mac (uses sudo)
   ```

### Development Workflow

**Windows/Mac** (recommended): Run DB + Redis in Docker, .NET locally via IDE:
```bash
task docker:up   # Start infrastructure (DB, Redis, Traefik)
task watch       # Run API with hot-reload
```

**Linux**: Run everything in Docker:
```bash
task docker:up   # Starts full stack including API container
```

### Development Endpoints

| Endpoint          | URL                                                 |
|-------------------|-----------------------------------------------------|
| API (direct)      | http://localhost:5141                               |
| API (via Traefik) | https://api.app.localhost *(requires hosts entry)*  |
| Scalar API docs   | http://localhost:5141/scalar/v1                     |
| Traefik Dashboard | http://localhost:8080                               |

### Redis Caching (Optional)

Redis is available but not started by default. To enable:

```bash
# Set in .env
CACHING__PROVIDER=Redis

# Start with cache profile
docker compose --profile cache up -d
```

## Docker Commands

| Command                | Description                                   |
|------------------------|-----------------------------------------------|
| `task docker:up`       | Smart start (Windows/Mac: infra, Linux: full) |
| `task docker:up:infra` | Start infrastructure only                     |
| `task docker:up:full`  | Start full stack with API container           |
| `task docker:down`     | Stop all containers                           |
| `task docker:logs`     | View container logs                           |
| `task docker:clean`    | Remove containers and volumes                 |

## Build & Test Commands

```bash
# Build
task build              # Debug build
task build:release      # Release build

# Run
task run                # Run API
task watch              # Run with hot-reload

# Test
task test               # All tests
task test:unit          # Unit tests only
task test:integration   # Integration tests

# EF Core Migrations
task migration:add -- Name    # Create migration
task db:update                # Apply migrations
task db:shell                 # PostgreSQL shell

# Utilities
task format             # Format code (dotnet format + CSharpier)
task format:check       # Check formatting without changes
task info               # Show environment info
```

View all commands: `task --list-all`
