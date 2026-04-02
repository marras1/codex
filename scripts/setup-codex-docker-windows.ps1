Param(
    [string]$WorkspacePath = "C:\codex\workspace",
    [string]$ContainerName = "codex-terminal",
    [string]$ImageName = "codex-local:latest",
    [int]$CodexLoginPort = 1455
)

$ErrorActionPreference = "Stop"

Write-Host "[1/10] Checking Docker installation..."
docker --version

docker compose version

Write-Host "[2/10] Preparing workspace folder..."
New-Item -ItemType Directory -Force -Path $WorkspacePath | Out-Null
Set-Location $WorkspacePath

Write-Host "[3/10] Writing Dockerfile (Codex runtime image)..."
@'
FROM node:20-bookworm

RUN apt-get update && apt-get install -y --no-install-recommends \
    git \
    curl \
    jq \
    ca-certificates \
    bash \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /workspace

# Use npx so the newest compatible Codex CLI is downloaded on demand.
CMD ["bash"]
'@ | Set-Content -Path Dockerfile -Encoding UTF8

Write-Host "[4/10] Building Docker image: $ImageName"
docker build -t $ImageName .

Write-Host "[5/10] Removing previous container if it exists..."
if ((docker ps -a --format '{{.Names}}' | Select-String -Pattern "^$ContainerName$" -Quiet)) {
    docker rm -f $ContainerName | Out-Null
}

Write-Host "[6/10] Starting isolated Codex container..."
docker run -d `
  --name $ContainerName `
  -w /workspace `
  -v "${WorkspacePath}:/workspace" `
  -p "${CodexLoginPort}:1455" `
  $ImageName `
  tail -f /dev/null

Write-Host "[7/10] Verifying container state..."
docker ps --filter "name=$ContainerName"

Write-Host "[8/10] Opening Codex login in container..."
docker exec -it $ContainerName bash -lc "npx -y @openai/codex --login"

Write-Host "[9/10] Starting Codex terminal session..."
docker exec -it $ContainerName bash -lc "cd /workspace && npx -y @openai/codex"

Write-Host "[10/10] Done. Useful follow-up commands:"
Write-Host "  docker exec -it $ContainerName bash"
Write-Host "  docker start $ContainerName"
Write-Host "  docker stop $ContainerName"
Write-Host "  docker rm -f $ContainerName"
