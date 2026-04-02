# Codex in Docker on Windows (Terminal-Only, Container-Isolated)

This is a **dummy/tutorial setup** that gives you Codex in a terminal while keeping your work inside one Docker container.

---

## Goal

By the end of this guide, you will have:
1. Docker Desktop installed on Windows.
2. A custom Docker image for Codex tooling.
3. A running container that you enter through terminal.
4. Codex CLI running inside that container.

---

## Step 1 — Install Docker Desktop on Windows

1. Open: https://www.docker.com/products/docker-desktop/
2. Download **Docker Desktop for Windows**.
3. Run installer and keep defaults (including WSL2 integration when prompted).
4. Reboot if installer asks.
5. Start Docker Desktop and wait until engine shows "Running".

### Verify Docker from PowerShell

```powershell
docker --version
docker compose version
```

Expected: both commands return version output without errors.

---

## Step 2 — Create a dedicated folder for Codex workspace

Open **PowerShell** and run:

```powershell
New-Item -ItemType Directory -Force -Path C:\codex\workspace
Set-Location C:\codex\workspace
```

Why: this folder is mounted into the container as `/workspace`.

---

## Step 3 — Create the automation script

Save this file as:

`C:\codex\workspace\setup-codex-docker-windows.ps1`

Then paste the script from this repo file:

- `scripts/setup-codex-docker-windows.ps1`

Why: script automates image build, container startup, and Codex login/start.

---

## Step 4 — Allow local script execution (current shell only)

```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
```

Why: PowerShell blocks unsigned scripts by default in many Windows setups.

---

## Step 5 — Run the setup script

From `C:\codex\workspace` run:

```powershell
.\setup-codex-docker-windows.ps1
```

What it does:
- Checks Docker commands.
- Creates a `Dockerfile`.
- Builds image `codex-local:latest`.
- Starts container `codex-terminal`.
- Maps login callback port `1455`.
- Runs `npx -y @openai/codex --login`.
- Starts Codex CLI inside container.

---

## Step 6 — Complete Codex login

When login command runs, follow terminal prompt instructions.

If browser callback is needed, the mapped port is:
- Host: `localhost:1455`
- Container: `1455`

---

## Step 7 — Start Codex again later

```powershell
docker start codex-terminal
docker exec -it codex-terminal bash -lc "cd /workspace && npx -y @openai/codex"
```

---

## Step 8 — Enter shell in container (optional)

```powershell
docker exec -it codex-terminal bash
```

Inside container, your workspace is:

```bash
cd /workspace
```

---

## Step 9 — Stop/remove container when done

Stop:

```powershell
docker stop codex-terminal
```

Remove:

```powershell
docker rm -f codex-terminal
```

Remove image:

```powershell
docker rmi codex-local:latest
```

---

## Step 10 — Confirm isolation expectation

This setup keeps Codex running inside Docker container terminal context.

Important note:
- Container can still see files in mounted host folder (`C:\codex\workspace`) by design.
- To make it even stricter, mount a smaller folder or mount read-only.

Read-only mount example:

```powershell
docker run -d --name codex-terminal -w /workspace -v "C:\codex\workspace:/workspace:ro" -p 1455:1455 codex-local:latest tail -f /dev/null
```

