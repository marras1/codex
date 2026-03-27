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
- Windows parent folder: `C:\codexFamilyLedger`
- Project folder in that parent: `C:\codexFamilyLedger\FamilyLedger`

If you need to create it (Windows PowerShell):

```powershell
New-Item -ItemType Directory -Force -Path C:\codexFamilyLedger\FamilyLedger
Set-Location C:\codexFamilyLedger\FamilyLedger
# clone/copy project files into this folder (git clone ... .)
```

If you use Git Bash:

```bash
mkdir -p /c/codexFamilyLedger/FamilyLedger
cd /c/codexFamilyLedger/FamilyLedger
```

**Best practice for names in this flow:**
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

### Docker setup (if not installed yet)

1. Install **Docker Desktop** (Windows/macOS) or **Docker Engine + Docker Compose plugin** (Linux).
2. Start Docker.
3. Confirm daemon is running.

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

## 3) FIRST STEP — setup Codex in Docker at `C:\codexFamilyLedger`

Run this first in PowerShell:

```powershell
Set-Location C:\codexFamilyLedger\FamilyLedger
docker compose -f docker-compose.yml -f docker-compose.codex.yml up -d --build codex
docker compose -f docker-compose.yml -f docker-compose.codex.yml exec codex codex --login
docker compose -f docker-compose.yml -f docker-compose.codex.yml exec codex codex
```

Why first:
- Codex CLI stays in a dedicated container (`familyledger-codex`).
- Workspace inside container is `/workspace/FamilyLedger`, mapped from `C:\codexFamilyLedger\FamilyLedger`.
- Codex has Docker socket access and can run the rest of the setup for you from that workspace.

Stop Codex later (if needed):

```bash
docker compose -f docker-compose.yml -f docker-compose.codex.yml stop codex
```

---

## 4) From Codex, run the rest (DB + API + PWA UI)

Inside Codex terminal, run:

```bash
docker compose down -v
docker compose up -d --build db api web
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

## 5) Use the mobile-first PWA UI

1. Open `http://localhost:8081` on desktop or mobile browser.
2. Complete forms top-to-bottom:
   - Register User
   - Login
   - Create Profile
   - Create Account
   - Create Transaction
3. Watch API outputs in the in-page `Result log` panel.

---

## 6) Run CLI tests with fixed values (no edits needed)

```bash
curl -s -X POST http://localhost:5000/api/v1/auth/register-user \
  -H "Content-Type: application/json" \
  -d '{"displayName":"Anna Family","email":"anna.family@example.com","password":"Test1234!"}' | jq

USER_TOKEN=$(curl -s -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"anna.family@example.com","password":"Test1234!"}' | jq -r '.accessToken')

PROFILE_ID=$(curl -s -X POST http://localhost:5000/api/v1/auth/profiles \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name":"FamilyLedger","currency":"EUR"}' | jq -r '.profileId')

USER_PROFILE_TOKEN=$(curl -s -X POST http://localhost:5000/api/v1/auth/switch-profile/$PROFILE_ID \
  -H "Authorization: Bearer $USER_TOKEN" | jq -r '.accessToken')

ACCOUNT_ID=$(curl -s -X POST http://localhost:5000/api/v1/accounts \
  -H "Authorization: Bearer $USER_PROFILE_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name":"SEB Savings","type":"savings","currency":"EUR","balanceOverride":12500}' | jq -r '.id')

curl -s -X POST http://localhost:5000/api/v1/transactions \
  -H "Authorization: Bearer $USER_PROFILE_TOKEN" \
  -H "Content-Type: application/json" \
  -d "{\"accountId\":\"$ACCOUNT_ID\",\"amount\":47.80,\"direction\":\"debit\",\"description\":\"Groceries\",\"date\":\"2026-03-27\",\"category\":\"groceries\"}" | jq
```

---

## 7) Superuser BO flow (fixed values)

```bash
curl -s -X POST http://localhost:5000/api/v1/auth/register-user \
  -H "Content-Type: application/json" \
  -d '{"displayName":"Admin Family","email":"admin.family@example.com","password":"Admin1234!","isSuperAdmin":true}' | jq

ADMIN_TOKEN=$(curl -s -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin.family@example.com","password":"Admin1234!"}' | jq -r '.accessToken')

TARGET_USER_ID=$(curl -s -X POST http://localhost:5000/api/v1/auth/register-user \
  -H "Content-Type: application/json" \
  -d '{"displayName":"Bob Family","email":"bob.family@example.com","password":"Bob1234!"}' | jq -r '.userId')

BO_PROFILE_ID=$(curl -s -X POST http://localhost:5000/api/v1/admin/profiles \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d "{\"name\":\"FamilyLedger BO\",\"currency\":\"EUR\",\"ownerUserId\":\"$TARGET_USER_ID\"}" | jq -r '.profileId')

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

---

## 9) Stop and reset

```bash
docker compose down
docker compose down -v
```
