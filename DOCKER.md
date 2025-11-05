# Docker Setup for BlogApp

This guide explains how to run the entire BlogApp stack using Docker Compose.

## Prerequisites

- Docker (version 20.10 or higher)
- Docker Compose (version 2.0 or higher)

## Quick Start

Run the entire application stack with a single command:

```bash
docker compose up
```

This will start:
- **Redis** (port 6379) - Database
- **Backend API** (port 8080) - ASP.NET Core Web API
- **Frontend** (port 3000) - React application served by nginx

## Access the Application

Once all services are running:

- **Frontend**: http://localhost:3000
- **Backend API**: http://localhost:8080
- **Swagger UI**: http://localhost:8080/swagger

## Configuration

### Environment Variables

You can configure the application using environment variables. Copy `.env.example` to `.env`:

```bash
cp .env.example .env
```

Then edit `.env` with your values:

- `STRIPE_SECRET_KEY` - Your Stripe secret key (optional, for payment features)
- `STRIPE_WEBHOOK_SECRET` - Your Stripe webhook secret (optional)

### Default Configuration

The following defaults are used:

- Redis: `redis:6379` (Docker network hostname)
- JWT Key: Pre-configured in docker-compose.yml
- JWT Issuer: `blogapp.api`
- JWT Audience: `blogapp.react`

## Docker Commands

### Start all services
```bash
docker compose up
```

### Start all services in detached mode (background)
```bash
docker compose up -d
```

### Stop all services
```bash
docker compose down
```

### Stop all services and remove volumes (clears database)
```bash
docker compose down -v
```

### View logs
```bash
docker compose logs -f
```

### View logs for specific service
```bash
docker compose logs -f backend
docker compose logs -f frontend
docker compose logs -f redis
```

### Rebuild services
```bash
docker compose up --build
```

### Rebuild specific service
```bash
docker compose build backend
docker compose build frontend
```

## Services

### Redis
- Image: `redis:7-alpine`
- Port: `6379`
- Data persistence: Uses named volume `redis-data`
- Health check: Pings Redis every 5 seconds

### Backend (ASP.NET Core API)
- Built from: `BlogApp.Api/Dockerfile`
- Port: `8080`
- Depends on: Redis (waits for health check)
- Auto-restarts on failure

### Frontend (React + nginx)
- Built from: `frontend/Dockerfile`
- Port: `3000` (mapped to nginx port 80)
- Proxies `/api/*` requests to backend
- Serves static React build
- Auto-restarts on failure

## Network

All services communicate over a custom Docker bridge network called `blogapp-network`. This allows services to reference each other by service name (e.g., `backend`, `redis`, `frontend`).

## Volumes

- `redis-data`: Persists Redis data between container restarts

## Troubleshooting

### Services not starting
```bash
# Check service status
docker compose ps

# Check logs for errors
docker compose logs
```

### Port already in use
If ports 3000, 6379, or 8080 are already in use, you can modify them in `docker-compose.yml`:

```yaml
ports:
  - "NEW_PORT:CONTAINER_PORT"
```

### Clear everything and start fresh
```bash
docker compose down -v
docker compose up --build
```

### Redis connection issues
Ensure Redis is healthy:
```bash
docker compose ps redis
```

If unhealthy, check Redis logs:
```bash
docker compose logs redis
```

## Development

To make changes:

1. Stop the containers: `docker compose down`
2. Make your code changes
3. Rebuild and restart: `docker compose up --build`

For faster development iteration, you can run only specific services in Docker while developing others locally.
