# FamilyLedger

FamilyLedger is a .NET 8 clean-architecture backend API for shared household finance management.

## Solution structure

- `src/FamilyLedger.API` - ASP.NET Core Web API, controllers, middleware, auth, swagger.
- `src/FamilyLedger.Application` - DTOs, interfaces, services, validators, mapping.
- `src/FamilyLedger.Domain` - entities, enums, domain exceptions.
- `src/FamilyLedger.Infrastructure` - EF Core DbContext, entity config, repositories.
- `tests/*` - unit and integration test projects.

## Local run

```bash
docker compose up -d
```

API: `http://localhost:5000`
Swagger: `http://localhost:5000/swagger`
