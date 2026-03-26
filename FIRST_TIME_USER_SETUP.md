# FamilyLedger — First-Time Setup (Docker + Codex CLI + Browser UI)

This setup is written so you can copy/paste every command **without editing values**.

**Target end state after this guide:**
1. Docker containers running for **DB**, **API**, and **PWA UI**.
2. A Codex CLI container attached to this workspace.
3. App accessible in browser at `http://localhost:8081` and testable immediately.

---

## 0) Fixed workspace and naming convention

Use this exact workspace path/name:

- Workspace folder name: `FamilyLedger`
- Local absolute path expected in this guide: `/workspace/codex`

If you use another folder name, commands still work if you `cd` to your repo root first.  
**Best practice for other names:**
- profile name: `FamilyLedger`
- first user: `anna.family@example.com`
- admin user: `admin.family@example.com`
- member user: `bob.family@example.com`
- account name: `SEB Savings`

---

## 1) Prerequisites

Install:
- Docker + Docker Compose
- curl + jq

Verify:

```bash
docker --version
docker compose version
curl --version
jq --version
```

---

## 2) Important sensitive values (read before running)

The project contains default local-development secrets. They are okay for local demos but must be changed for real deployments.

### Sensitive values currently used

- DB password: `fl_dev`
- JWT key: `your-local-dev-secret-key-min-32-chars`
- OpenAI API key: from your shell env var `OPENAI_API_KEY`

### How to change them

1. Edit `docker-compose.yml` under `api.environment` and `db.environment`.
2. If DB password changes, also update `ConnectionStrings__DefaultConnection` in `docker-compose.yml`.
3. Export your OpenAI key before launching Codex container:

```bash
export OPENAI_API_KEY="sk-your-real-key-here"
```

> Never commit production credentials to git.

---

## 3) Start all project containers (DB + API + PWA UI)

From repo root:

```bash
cd /workspace/codex
docker compose down -v
docker compose up -d --build db api web
```

Check status:

```bash
docker compose ps
```

Expected services up:
- `db`
- `api`
- `web`

Open in browser:
- PWA UI: `http://localhost:8081`
- Swagger API: `http://localhost:5000/swagger`

If you are starting with a fresh DB, load schema once:

```bash
docker compose exec -T db psql -U fl -d familyledger < schema.sql
```

---

## 4) Start Codex CLI in Docker (attached to this project)

From repo root:

```bash
cd /workspace/codex
docker compose -f docker-compose.yml -f docker-compose.codex.yml run --rm codex
```

This mounts your project into the Codex container at `/workspace`.

---

## 5) Use the mobile-first PWA UI

1. Open `http://localhost:8081` on desktop or mobile browser.
2. Complete forms top-to-bottom:
   - Register User
   - Login
   - Create Profile
   - Create Account
   - Create Transaction
3. Watch API outputs in the in-page `Result log` panel.

Theme notes:
- green-first palette
- simple cards, mobile-first layout
- installable PWA manifest + service worker caching

---

## 6) Run CLI tests with fixed values (no edits needed)

### 6.1 Register default user

```bash
curl -s -X POST http://localhost:5000/api/v1/auth/register-user \
  -H "Content-Type: application/json" \
  -d '{"displayName":"Anna Family","email":"anna.family@example.com","password":"Test1234!"}' | jq
```

### 6.2 Login

```bash
USER_TOKEN=$(curl -s -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"anna.family@example.com","password":"Test1234!"}' | jq -r '.accessToken')
```

### 6.3 Create profile

```bash
PROFILE_ID=$(curl -s -X POST http://localhost:5000/api/v1/auth/profiles \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name":"FamilyLedger","currency":"EUR"}' | jq -r '.profileId')
```

### 6.4 Switch profile

```bash
USER_PROFILE_TOKEN=$(curl -s -X POST http://localhost:5000/api/v1/auth/switch-profile/$PROFILE_ID \
  -H "Authorization: Bearer $USER_TOKEN" | jq -r '.accessToken')
```

### 6.5 Create account

```bash
ACCOUNT_ID=$(curl -s -X POST http://localhost:5000/api/v1/accounts \
  -H "Authorization: Bearer $USER_PROFILE_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name":"SEB Savings","type":"savings","currency":"EUR","balanceOverride":12500}' | jq -r '.id')
```

### 6.6 Create transaction

```bash
curl -s -X POST http://localhost:5000/api/v1/transactions \
  -H "Authorization: Bearer $USER_PROFILE_TOKEN" \
  -H "Content-Type: application/json" \
  -d "{\"accountId\":\"$ACCOUNT_ID\",\"amount\":47.80,\"direction\":\"debit\",\"description\":\"Groceries\",\"date\":\"2026-03-26\",\"category\":\"groceries\"}" | jq
```

---

## 7) Superuser BO flow (fixed values)

### 7.1 Register superuser

```bash
curl -s -X POST http://localhost:5000/api/v1/auth/register-user \
  -H "Content-Type: application/json" \
  -d '{"displayName":"Admin Family","email":"admin.family@example.com","password":"Admin1234!","isSuperAdmin":true}' | jq
```

### 7.2 Login superuser

```bash
ADMIN_TOKEN=$(curl -s -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin.family@example.com","password":"Admin1234!"}' | jq -r '.accessToken')
```

### 7.3 Register another member

```bash
TARGET_USER_ID=$(curl -s -X POST http://localhost:5000/api/v1/auth/register-user \
  -H "Content-Type: application/json" \
  -d '{"displayName":"Bob Family","email":"bob.family@example.com","password":"Bob1234!"}' | jq -r '.userId')
```

### 7.4 BO create profile + assign owner

```bash
BO_PROFILE_ID=$(curl -s -X POST http://localhost:5000/api/v1/admin/profiles \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d "{\"name\":\"FamilyLedger BO\",\"currency\":\"EUR\",\"ownerUserId\":\"$TARGET_USER_ID\"}" | jq -r '.profileId')
```

### 7.5 BO dashboard check

```bash
curl -s http://localhost:5000/api/v1/admin/dashboard \
  -H "Authorization: Bearer $ADMIN_TOKEN" | jq
```

---

## 8) Database verification (expected proof)

```bash
docker compose exec db psql -U fl -d familyledger -c 'SELECT id, display_name, email, is_super_admin FROM "Users" ORDER BY created_at DESC;'
docker compose exec db psql -U fl -d familyledger -c 'SELECT user_id, profile_id, role FROM "Members" ORDER BY joined_at DESC;'
docker compose exec db psql -U fl -d familyledger -c 'SELECT id, name, currency FROM "Profiles" ORDER BY created_at DESC;'
docker compose exec db psql -U fl -d familyledger -c 'SELECT id, account_id, amount, direction, description, date FROM "Transactions" ORDER BY created_at DESC;'
```

Expected:
- users for Anna/Admin/Bob present
- at least one profile called `FamilyLedger`
- memberships linking users to profiles
- at least one groceries transaction

---

## 9) Stop and reset

```bash
docker compose down
docker compose down -v
```
