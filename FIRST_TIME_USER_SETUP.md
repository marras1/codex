# FamilyLedger — First-Time User Setup (User + Superuser BO)

This guide now includes:
- standard user self-setup (register -> create profile -> enter data)
- superuser back-office setup (create profiles, assign/revoke members)
- quick test scripts
- DB verification queries
- **Dockerized project run**
- **Codex CLI setup (host + Docker)**

## 1. Prerequisites

- .NET SDK 8+
- Docker + Docker Compose
- curl + jq
- psql (or use `docker compose exec db psql`)
- Node.js 20+ (only needed if installing Codex CLI directly on host)

Verify:

```bash
dotnet --version
docker --version
docker compose version
curl --version
jq --version
node --version
npm --version
```

---

## 2. Run FamilyLedger fully in Docker

### Option A (recommended): API + DB via Docker Compose

```bash
docker compose up -d --build db api
```

Check health/logs:

```bash
docker compose ps
docker compose logs -f api
```

API: `http://localhost:5000`
Swagger: `http://localhost:5000/swagger`

> If tables are missing, apply schema once:

```bash
docker compose exec -T db psql -U fl -d familyledger < schema.sql
```

### Option B: DB in Docker + API from local .NET

```bash
docker compose up -d db
cd src/FamilyLedger.API
dotnet restore
dotnet run
```

---

## 3. Install and run Codex CLI for this project

### 3.1 Host install (fastest)

From your terminal:

```bash
npm install -g @openai/codex
codex --login
```

Then run Codex in this repo:

```bash
cd /workspace/codex
codex
```

### 3.2 Dockerized Codex CLI (no host Node install)

Use the included helper compose file:

```bash
docker compose -f docker-compose.yml -f docker-compose.codex.yml run --rm codex
```

This starts an ephemeral Codex CLI container mounted to this repository at `/workspace`, so Codex can operate on project files.

If you prefer direct Docker run:

```bash
docker run --rm -it \
  -v "$PWD":/workspace \
  -w /workspace \
  -e OPENAI_API_KEY \
  node:20-bullseye \
  bash -lc "npm install -g @openai/codex && codex"
```

---

## 4. Flow A — normal user self-onboarding in app/API

### Step A1) Register user

```bash
curl -s -X POST http://localhost:5000/api/v1/auth/register-user \
  -H "Content-Type: application/json" \
  -d '{"displayName":"Anna","email":"anna@example.com","password":"Test1234!"}' | jq
```

### Step A2) Login (without active profile yet)

```bash
USER_TOKEN=$(curl -s -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"anna@example.com","password":"Test1234!"}' | jq -r '.accessToken')
```

### Step A3) Create profile as logged-in user

```bash
PROFILE_ID=$(curl -s -X POST http://localhost:5000/api/v1/auth/profiles \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name":"Smith Family","currency":"EUR"}' | jq -r '.profileId')
```

### Step A4) Switch token context to that profile

```bash
USER_PROFILE_TOKEN=$(curl -s -X POST http://localhost:5000/api/v1/auth/switch-profile/$PROFILE_ID \
  -H "Authorization: Bearer $USER_TOKEN" | jq -r '.accessToken')
```

### Step A5) Create account + add transaction

```bash
ACCOUNT_ID=$(curl -s -X POST http://localhost:5000/api/v1/accounts \
  -H "Authorization: Bearer $USER_PROFILE_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name":"SEB Savings","type":"savings","currency":"EUR","balanceOverride":12500}' | jq -r '.id')

curl -s -X POST http://localhost:5000/api/v1/transactions \
  -H "Authorization: Bearer $USER_PROFILE_TOKEN" \
  -H "Content-Type: application/json" \
  -d "{\"accountId\":\"$ACCOUNT_ID\",\"amount\":47.80,\"direction\":\"debit\",\"description\":\"Groceries\",\"date\":\"2026-03-26\",\"category\":\"groceries\"}" | jq
```

---

## 5. Flow B — superuser BO setup for other members

### Step B1) Register superuser

```bash
curl -s -X POST http://localhost:5000/api/v1/auth/register-user \
  -H "Content-Type: application/json" \
  -d '{"displayName":"Admin","email":"admin@example.com","password":"Admin1234!","isSuperAdmin":true}' | jq
```

### Step B2) Login as superuser

```bash
ADMIN_TOKEN=$(curl -s -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@example.com","password":"Admin1234!"}' | jq -r '.accessToken')
```

### Step B3) Register another user to be assigned

```bash
TARGET_USER_ID=$(curl -s -X POST http://localhost:5000/api/v1/auth/register-user \
  -H "Content-Type: application/json" \
  -d '{"displayName":"Bob","email":"bob@example.com","password":"Bob1234!"}' | jq -r '.userId')
```

### Step B4) BO: create profile and assign owner

```bash
BO_PROFILE_ID=$(curl -s -X POST http://localhost:5000/api/v1/admin/profiles \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d "{\"name\":\"Johnson Family\",\"currency\":\"EUR\",\"ownerUserId\":\"$TARGET_USER_ID\"}" | jq -r '.profileId')
```

### Step B5) BO: assign/revoke additional members

```bash
curl -s -X PUT http://localhost:5000/api/v1/admin/profiles/$BO_PROFILE_ID/members/$TARGET_USER_ID \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"role":"Editor"}' -i

curl -s -X DELETE http://localhost:5000/api/v1/admin/profiles/$BO_PROFILE_ID/members/$TARGET_USER_ID \
  -H "Authorization: Bearer $ADMIN_TOKEN" -i
```

### Step B6) BO dashboard (usage/stats overview)

```bash
curl -s http://localhost:5000/api/v1/admin/dashboard \
  -H "Authorization: Bearer $ADMIN_TOKEN" | jq
```

Expected fields:
- `totalUsers`
- `totalProfiles`
- `totalMemberships`
- `totalTransactions`

---

## 6. Test scripts

```bash
# unit tests
dotnet test tests/FamilyLedger.UnitTests

# integration tests
docker compose up -d db
dotnet test tests/FamilyLedger.IntegrationTests
```

---

## 7. Database verification after tests

Open psql:

```bash
docker compose exec db psql -U fl -d familyledger
```

Run checks:

```sql
-- users (supports global users that can join many profiles)
SELECT id, display_name, email, is_super_admin, created_at FROM "Users" ORDER BY created_at DESC;

-- profile memberships (same user_id can appear across multiple profile_id rows)
SELECT user_id, profile_id, role, joined_at FROM "Members" ORDER BY joined_at DESC;

-- profiles
SELECT id, name, currency, created_at FROM "Profiles" ORDER BY created_at DESC;

-- transactions
SELECT id, account_id, logged_by, amount, direction, description, date, category, created_at
FROM "Transactions"
ORDER BY created_at DESC;
```

What success looks like:
1. Same `user_id` appears in multiple profile rows if user belongs to multiple families.
2. Superuser row has `is_super_admin = true`.
3. Admin dashboard totals increase after each create/assign/transaction action.

---

## 8. Stop environment

```bash
docker compose down
# full reset (optional)
docker compose down -v
```
