# FamilyLedger

FamilyLedger is a local household finance demo built from a .NET 8 API, PostgreSQL, a browser UI, an admin console, a raw API Test Lab, and an optional Codex container workflow.

This README is the single setup and usage document for the repository.

## Project Layout

Backend:
- `src/FamilyLedger.API` for HTTP API, auth, Swagger, and admin endpoints.
- `src/FamilyLedger.Application` for services, DTOs, mappings, and validation.
- `src/FamilyLedger.Domain` for entities and enums.
- `src/FamilyLedger.Infrastructure` for EF Core data access.

Frontend:
- `src/FamilyLedger.Web` for landing page, user flows, admin console, and Test Lab.

Infrastructure:
- `docker-compose.yml` for API, web, Postgres, DB init, and Adminer.
- `docker-compose.codex.yml` for the Codex sidecar container.
- `schema.local.sql` for the local database schema.

## URLs

When the stack is running:
- UI: `http://localhost:8081`
- Swagger: `http://localhost:5000/swagger`
- API base: `http://localhost:5000/api/v1`
- Adminer: `http://localhost:8082`
- PostgreSQL: `localhost:5432`

Main routes:
- `#/landing`
- `#/user/register`
- `#/user/login`
- `#/user/app`
- `#/admin/login`
- `#/admin/app`
- `#/test`

## Important Local Behavior

The current `db-init` service runs `schema.local.sql` during startup.
That file drops and recreates the application tables.

That means:
- `docker compose up` recreates the application schema.
- previous users and transactions are removed.
- browser local storage can still hold an old token until logout or refresh.

For persistent data, change the DB init flow before using this outside local demo work.

## Setup Without Codex

### Prerequisites

Install:
- Docker Desktop or Docker Engine with Compose
- Git
- optional `curl` and `jq`

Clone the repository:

```bash
git clone <your-repo-url> FamilyLedger
cd FamilyLedger
```

Start everything:

```bash
docker compose up -d --build
```

Open:
- `http://localhost:8081`
- `http://localhost:5000/swagger`
- `http://localhost:8082`

Stop services:

```bash
docker compose down
```

Remove services and volumes:

```bash
docker compose down -v
```

## First Run Without Codex

### End user flow

1. Open `http://localhost:8081/#/user/register`
2. Register a user.
3. Create a profile.
4. Create an account.
5. Create a transaction.

The default UI values are valid for local testing:
- user email: `anna.family@example.com`
- user password: `Test1234!`

### Admin flow

There is no automatic admin seed in the repository.
Create a super admin explicitly.

Option A:
- open `http://localhost:8081/#/test`
- use `Register User`
- tick `Super admin`
- example email: `admin.family@example.com`
- example password: `Admin1234!`

Option B:

```bash
curl -X POST http://localhost:5000/api/v1/auth/register-user \
  -H "Content-Type: application/json" \
  -d '{"displayName":"Admin Family","email":"admin.family@example.com","password":"Admin1234!","isSuperAdmin":true}'
```

Then log in at `http://localhost:8081/#/admin/login`.

## View And Manage The Database

Open Adminer at `http://localhost:8082`.

Use:
- System: `PostgreSQL`
- Server: `db`
- Username: `fl`
- Password: `fl_dev`
- Database: `familyledger`

Useful tables:
- `Users`
- `Profiles`
- `Members`
- `Accounts`
- `Transactions`

## Setup With Codex Like This Environment

This repository includes `docker-compose.codex.yml` for running Codex against the same mounted project.

### Host preparation

Windows PowerShell example:

```powershell
New-Item -ItemType Directory -Force -Path C:\codexFamilyLedger\FamilyLedger
Set-Location C:\codexFamilyLedger\FamilyLedger
git clone <your-repo-url> .
```

### Start the Codex container

```powershell
docker compose -f docker-compose.yml -f docker-compose.codex.yml up -d --build codex
```

What this does:
- mounts the repo into the container at `/workspace/FamilyLedger`
- maps login callback port `1455:1455`
- shares the host Docker socket with Codex

### Login to Codex

```powershell
$env:OPENAI_API_KEY = "sk-your-key"
docker compose -f docker-compose.yml -f docker-compose.codex.yml exec codex sh -lc "npx -y @openai/codex --login"
docker compose -f docker-compose.yml -f docker-compose.codex.yml exec codex sh -lc "npx -y @openai/codex"
```

### Launch the app from inside Codex

Inside the Codex shell:

```bash
cd /workspace/FamilyLedger
docker compose up -d --build
```

Then use the same host URLs:
- `http://localhost:8081`
- `http://localhost:5000/swagger`
- `http://localhost:8082`

### Why this matches the current environment

The workspace is mounted from the host into the Codex container.
Codex sees the repo at `/workspace/FamilyLedger`, while Docker Compose still publishes the app ports back to the host machine.
That is the same operating model used in this session.

## Repository Alignment Notes

The repository is aligned with the current working project shape:
- separate landing, user, admin, and test routes in the web app
- admin overview endpoints
- Docker stack with Postgres, schema init, and Adminer
- Codex sidecar container flow

Two caveats remain:
- `FamilyLedger.sln` is effectively empty and is not a reliable build entry point.
- `schema.local.sql` is destructive during startup because it recreates tables.

For reliable build and run entry points, use:
- `src/FamilyLedger.API/FamilyLedger.API.csproj`
- `docker compose up -d --build`

## Smoke Test

End user:
1. Open `http://localhost:8081/#/user/register`
2. Register
3. Create profile
4. Create account
5. Create transaction

Admin:
1. Create a super admin account
2. Open `http://localhost:8081/#/admin/login`
3. Log in
4. Review overview, profiles, activity, and settings

Database:
1. Open `http://localhost:8082`
2. Inspect `Users`, `Profiles`, `Accounts`, and `Transactions`

## Main Files

- `docker-compose.yml`
- `docker-compose.codex.yml`
- `schema.local.sql`
- `src/FamilyLedger.API/Program.cs`
- `src/FamilyLedger.API/Controllers/*.cs`
- `src/FamilyLedger.Application/Services/AuthService.cs`
- `src/FamilyLedger.Infrastructure/Data/AppDbContext.cs`
- `src/FamilyLedger.Web/index.html`
- `src/FamilyLedger.Web/app.js`
- `src/FamilyLedger.Web/styles.css`
