# Codex Preparation — Complete Dummy-Proof Step-by-Step (Windows)

This guide is intentionally very explicit.
Follow every step in order.

---

## 1) Create the parent folder on `C:\`

1. Press **Start**.
2. Type **PowerShell**.
3. Open **Windows PowerShell**.
4. Run:

```powershell
Set-Location C:\
New-Item -ItemType Directory -Force -Path C:\codexFamilyLedger
Set-Location C:\codexFamilyLedger
Get-Location
```

Expected result: current location is `C:\codexFamilyLedger`.

---

## 2) Download repository files into `C:\codexFamilyLedger\FamilyLedger`

Choose **one** method.

### Method A (recommended): clone with Git

1. In the same PowerShell window, run:

```powershell
New-Item -ItemType Directory -Force -Path C:\codexFamilyLedger\FamilyLedger
Set-Location C:\codexFamilyLedger\FamilyLedger
git clone https://github.com/<your-org-or-user>/FamilyLedger.git .
```

2. Validate files exist:

```powershell
Get-ChildItem
Get-ChildItem docker-compose*.yml
```

### Method B: download ZIP and extract manually

1. Open your browser and go to your GitHub repo page.
2. Click **Code** -> **Download ZIP**.
3. Save the ZIP file.
4. In File Explorer, create: `C:\codexFamilyLedger\FamilyLedger`.
5. Right-click the ZIP -> **Extract All...**
6. Extract all files directly into `C:\codexFamilyLedger\FamilyLedger`.
7. Back in PowerShell, run:

```powershell
Set-Location C:\codexFamilyLedger\FamilyLedger
Get-ChildItem docker-compose*.yml
```

If `docker-compose.yml` is missing, extraction path is wrong.

---

## 3) Verify required tools are installed

Run:

```powershell
docker --version
docker compose version
git --version
```

If Docker commands fail, install/start Docker Desktop before continuing.

---

## 4) Build and start Codex container

From `C:\codexFamilyLedger\FamilyLedger`, run:

```powershell
docker compose -f docker-compose.yml -f docker-compose.codex.yml up -d --build codex
docker compose -f docker-compose.yml -f docker-compose.codex.yml ps -a
```

Expected: service `codex` is `Up`.

---

## 5) Login to Codex with your OpenAI key

1. Set key in the current PowerShell session:

```powershell
$env:OPENAI_API_KEY = "sk-your-real-key-here"
```

2. Run Codex login:

```powershell
docker compose -f docker-compose.yml -f docker-compose.codex.yml exec codex sh -lc "npx -y @openai/codex --login"
```

3. Start Codex interactive shell:

```powershell
docker compose -f docker-compose.yml -f docker-compose.codex.yml exec codex sh -lc "npx -y @openai/codex"
```

---

## 6) Give Codex GitHub access (inside container)

Run:

```powershell
docker compose -f docker-compose.yml -f docker-compose.codex.yml exec codex gh auth login
docker compose -f docker-compose.yml -f docker-compose.codex.yml exec codex gh auth status
```

Follow the prompts (browser/device code) until authenticated.

---

## 7) Troubleshooting quick fixes

### Error: `open C:\codexFamilyLedger\FamilyLedger\docker-compose.yml: The system cannot find the file specified`

You are in the wrong folder OR repo files were not cloned/extracted there.

Run:

```powershell
Set-Location C:\codexFamilyLedger\FamilyLedger
Get-ChildItem
Get-ChildItem docker-compose*.yml
```

### Error: `service "codex" is not running`

Run:

```powershell
docker compose -f docker-compose.yml -f docker-compose.codex.yml ps -a
docker compose -f docker-compose.yml -f docker-compose.codex.yml logs codex
docker compose -f docker-compose.yml -f docker-compose.codex.yml up -d --build codex
```

### `exec` still fails

Use one-shot fallback:

```powershell
docker compose -f docker-compose.yml -f docker-compose.codex.yml run --rm codex sh -lc "npx -y @openai/codex --login"
docker compose -f docker-compose.yml -f docker-compose.codex.yml run --rm codex sh -lc "npx -y @openai/codex"
```

---

## 8) Next step after Codex is ready

After Codex login works, continue with the main project setup guide:

- `FIRST_TIME_USER_SETUP.md`

