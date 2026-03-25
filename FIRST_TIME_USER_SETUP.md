# FamilyLedger — First-Time User Setup Instructions

This is a complete, terminal-first guide for first-time setup, running, API testing, automated tests, and database verification on localhost.

---

## 1) Prerequisites

Install the following tools first:

1. **.NET SDK 8.x**
   - Verify:
     ```bash
     dotnet --version
     ```
   - Expected: a version starting with `8.`

2. **Docker Desktop** (or Docker Engine + Compose plugin)
   - Verify:
     ```bash
     docker --version
     docker compose version
     ```

3. **PostgreSQL client tools (`psql`)**
   - Verify:
     ```bash
     psql --version
     ```

4. **curl** + **jq**
   - Verify:
     ```bash
     curl --version
     jq --version
     ```

5. (Optional) **Entity Framework CLI**
   - Install once:
     ```bash
     dotnet tool install --global dotnet-ef
     ```
   - Verify:
     ```bash
     dotnet ef --version
     ```

---

## 2) Clone and enter the project

```bash
git clone <YOUR_REPO_URL> familyledger
cd familyledger
```

Confirm structure:

```bash
find src tests -maxdepth 2 -type d
```

---

## 3) Start local infrastructure (PostgreSQL + API container)

From repository root:

```bash
docker compose up -d db
```

Check DB health:

```bash
docker compose ps
```

Expected: `db` service is `healthy`.

> Why DB-only first? It lets you run the API via `dotnet run` locally and still use the same Postgres.

---

## 4) Create schema (first-time DB init)

Current repository includes `schema.sql` for enum setup. Load it:

```bash
docker compose exec -T db psql -U fl -d familyledger < schema.sql
```

Verify enums exist:

```bash
docker compose exec -T db psql -U fl -d familyledger -c "SELECT typname FROM pg_type WHERE typname IN ('member_role','account_type','transaction_direction','transaction_category','allocation_status','recurring_frequency','monthly_record_status') ORDER BY typname;"
```

Expected rows:
- account_type
- allocation_status
- member_role
- monthly_record_status
- recurring_frequency
- transaction_category
- transaction_direction

---

## 5) Configure and run API on localhost

### Option A (recommended for development): run API directly with dotnet

```bash
cd src/FamilyLedger.API
dotnet restore
dotnet run
```

Expected startup endpoints:
- API base: `http://localhost:5000`
- Swagger UI: `http://localhost:5000/swagger`

### Option B: run API in Docker

From repo root:

```bash
docker compose up -d api
```

Check logs:

```bash
docker compose logs -f api
```

---

## 6) Use the API from terminal (localhost)

Open a second terminal at repo root.

### 6.1 Register

```bash
curl -s -X POST http://localhost:5000/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "displayName":"Anna Smith",
    "email":"anna@example.com",
    "password":"Test1234!",
    "profileName":"Smith Family",
    "currency":"EUR"
  }' | jq
```

### 6.2 Login and capture token

```bash
ACCESS_TOKEN=$(curl -s -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"anna@example.com","password":"Test1234!"}' | jq -r '.accessToken')

echo "$ACCESS_TOKEN" | cut -c1-40
```

### 6.3 Create an account

```bash
ACCOUNT_ID=$(curl -s -X POST http://localhost:5000/api/v1/accounts \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name":"SEB Savings",
    "type":"savings",
    "institution":"SEB",
    "balanceOverride":12500.00,
    "currency":"EUR"
  }' | jq -r '.id')

echo "$ACCOUNT_ID"
```

### 6.4 Log a transaction

```bash
curl -s -X POST http://localhost:5000/api/v1/transactions \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d "{
    \"accountId\": \"$ACCOUNT_ID\",
    \"amount\": 47.80,
    \"direction\": \"debit\",
    \"description\": \"Lidl groceries\",
    \"date\": \"2025-03-25\",
    \"category\": \"groceries\",
    \"note\": \"Weekly shop\"
  }" | jq
```

### 6.5 Query transactions for month

```bash
curl -s "http://localhost:5000/api/v1/transactions?from=2025-03-01&to=2025-03-31&limit=50&offset=0" \
  -H "Authorization: Bearer $ACCESS_TOKEN" | jq
```

---

## 7) Test scripts (copy/paste ready)

Run these from repo root.

### 7.1 Unit tests

```bash
dotnet test tests/FamilyLedger.UnitTests
```

### 7.2 Integration tests

```bash
docker compose up -d db
dotnet test tests/FamilyLedger.IntegrationTests
```

### 7.3 Full test run with coverage

```bash
dotnet test --collect:"XPlat Code Coverage"
```

### 7.4 Quick smoke test (API + DB + login)

```bash
set -euo pipefail

# health via swagger document
curl -sf http://localhost:5000/swagger/v1/swagger.json >/dev/null && echo "Swagger reachable"

# login should return access token key
curl -s -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"anna@example.com","password":"Test1234!"}' | jq -e '.accessToken' >/dev/null

echo "Login endpoint OK"
```

---

## 8) Verify database records from test/API actions

Use these SQL commands to verify data written by API calls and tests.

### 8.1 Connect to DB shell

```bash
docker compose exec db psql -U fl -d familyledger
```

### 8.2 Verify profile + members

```sql
SELECT id, name, currency, created_at
FROM profile
ORDER BY created_at DESC
LIMIT 5;

SELECT id, profile_id, display_name, email, role, joined_at
FROM member
ORDER BY joined_at DESC
LIMIT 10;
```

Expected: newly registered owner/member row should exist.

### 8.3 Verify accounts

```sql
SELECT id, profile_id, name, type, institution, balance_override, currency, is_active, created_at
FROM account
ORDER BY created_at DESC
LIMIT 10;
```

Expected: `SEB Savings` appears with `balance_override = 12500.00`.

### 8.4 Verify transactions

```sql
SELECT id, account_id, logged_by, amount, direction, description, date, category, created_at
FROM transaction
ORDER BY created_at DESC
LIMIT 20;
```

Expected: `Lidl groceries` debit transaction exists.

### 8.5 Verify monthly linking

```sql
SELECT mt.id, mt.monthly_record_id, mt.transaction_id
FROM monthly_transaction mt
ORDER BY mt.id DESC
LIMIT 20;
```

Expected: new rows appear when transaction date matches open monthly record.

### 8.6 Verify account effective balance logic (manual query)

```sql
SELECT
  a.id,
  a.name,
  a.balance_override,
  COALESCE(SUM(CASE WHEN t.direction='credit' THEN t.amount ELSE -t.amount END), 0) AS derived_balance,
  CASE WHEN a.balance_override IS NOT NULL
       THEN a.balance_override
       ELSE COALESCE(SUM(CASE WHEN t.direction='credit' THEN t.amount ELSE -t.amount END), 0)
  END AS effective_balance
FROM account a
LEFT JOIN transaction t ON t.account_id = a.id
GROUP BY a.id
ORDER BY a.created_at DESC;
```

Expected: if `balance_override` is set, `effective_balance` equals override.

---

## 9) End-to-end validation checklist

Run in order and verify each item:

1. `docker compose up -d db` succeeds.
2. `schema.sql` applies with no SQL errors.
3. API starts and `http://localhost:5000/swagger` loads.
4. Register/login returns JWT token.
5. Create account returns `201` with account id.
6. Log transaction returns `201` with transaction id.
7. DB query confirms transaction row inserted.
8. `dotnet test` commands pass (unit + integration).

---

## 10) Troubleshooting

### `dotnet: command not found`
Install .NET 8 SDK and restart terminal.

### `connection refused` on localhost:5000
API not running. Start API via `dotnet run` or `docker compose up -d api`.

### `psql: command not found`
Install PostgreSQL client tools or use `docker compose exec db psql ...` only.

### JWT 401 on protected endpoints
- token missing `Bearer ` prefix, or
- access token expired, or
- wrong issuer/audience/key settings.

### DB auth errors
Ensure compose credentials match:
- DB: `familyledger`
- user: `fl`
- password: `fl_dev`

---

## 11) Stop and clean up

```bash
docker compose down
# optional full reset (deletes data volume)
docker compose down -v
```

