# FamilyLedger

FamilyLedger is a .NET 8 backend API for shared household finance management.

## What is included in this scaffold

- Clean architecture projects (`API`, `Application`, `Domain`, `Infrastructure`)
- Transaction flow (DTOs, service, repos, controller)
- Multi-profile membership foundation (one user can belong to many profiles)
- Superuser Back Office (admin dashboard + profile/member management endpoints)
- Docker compose + Postgres + starter schema
- Unit and integration test projects
- First-time setup guide: `FIRST_TIME_USER_SETUP.md`

## Local startup

```bash
docker compose up -d db
cd src/FamilyLedger.API
dotnet restore
dotnet run
```

Swagger: `http://localhost:5000/swagger`
