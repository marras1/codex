# FamilyLedger

FamilyLedger is a .NET 8 backend API for shared household finance management.

## What is included in this scaffold

- Clean architecture projects (`API`, `Application`, `Domain`, `Infrastructure`)
- Transaction flow (DTOs, service, repos, controller)
- Multi-profile membership foundation (one user can belong to many profiles)
- Superuser Back Office (admin dashboard + profile/member management endpoints)
- Docker compose + Postgres + starter schema
- Optional Dockerized Codex CLI helper (`docker-compose.codex.yml`)
- Mobile-first PWA web client (`src/FamilyLedger.Web`) with green theme
- Unit and integration test projects
- First-time setup guide: `FIRST_TIME_USER_SETUP.md`
- Dummy-proof Codex prep guide (Windows): `CODEX_PREPARATION_STEP_BY_STEP.md`

## Local startup

```bash
docker compose up -d --build db api web
```

PWA UI: `http://localhost:8081`
Swagger: `http://localhost:5000/swagger`

Dockerized Codex CLI:

```bash
docker compose -f docker-compose.yml -f docker-compose.codex.yml up -d codex
docker compose -f docker-compose.yml -f docker-compose.codex.yml exec codex sh -lc "npx -y @openai/codex --login"
docker compose -f docker-compose.yml -f docker-compose.codex.yml exec codex sh -lc "npx -y @openai/codex"
```

Note: `docker-compose.codex.yml` maps login callback port `1455:1455`.

Recommended order: start `codex` first, then run DB/API/web commands from inside Codex.

If `exec` reports the service is not running, inspect and run one-shot:

```bash
docker compose -f docker-compose.yml -f docker-compose.codex.yml ps -a
docker compose -f docker-compose.yml -f docker-compose.codex.yml logs codex
docker compose -f docker-compose.yml -f docker-compose.codex.yml run --rm codex sh -lc "npx -y @openai/codex --login"
```

Complete deterministic first-time flow (with fixed values + sensitive data guidance): `FIRST_TIME_USER_SETUP.md`.

Windows note: this project can be run from `C:\codexFamilyLedger\FamilyLedger` with the same Docker Compose commands.
