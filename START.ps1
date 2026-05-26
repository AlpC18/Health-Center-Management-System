# Wellness House - one-shot launcher for Windows (PowerShell).
#   Right-click > Run with PowerShell, or:  pwsh -File .\START.ps1
# Starts the .NET backend and the Vite frontend, then opens the browser.
$ErrorActionPreference = "Stop"

$Root        = Split-Path -Parent $MyInvocation.MyCommand.Path
$BackendDir  = Join-Path $Root "wellness-backend\WellnessAPI"
$FrontendDir = Join-Path $Root "frontend"

Write-Host "Wellness House - starting up..."
Write-Host "  (Make sure MySQL is running: cd wellness-backend; docker compose up -d)"
Write-Host "  Repo:     $Root"
Write-Host "  Backend:  $BackendDir"
Write-Host "  Frontend: $FrontendDir"
Write-Host ""

# Make sure dotnet is available (Program Files install is on PATH by default).
$dotnetUser = Join-Path $env:USERPROFILE ".dotnet"
if (Test-Path $dotnetUser) { $env:PATH = "$dotnetUser;$dotnetUser\tools;$env:PATH" }

# Load backend environment variables from .env (if present).
$envFile = Join-Path $Root ".env"
if (Test-Path $envFile) {
  Get-Content $envFile | ForEach-Object {
    $line = $_.Trim()
    if ($line -eq "" -or $line.StartsWith("#")) { return }
    $idx = $line.IndexOf("=")
    if ($idx -lt 1) { return }
    $k = $line.Substring(0, $idx)
    $v = $line.Substring($idx + 1)
    [System.Environment]::SetEnvironmentVariable($k, $v, "Process")
  }
}

# Development-only fallback so the API can start without committing local secrets.
if (-not $env:Jwt__Key) {
  $env:Jwt__Key = "WellnessHouseDevelopmentJwtSigningKey_ChangeBeforeProduction_2026"
}

# Free up the ports if a previous run left something running.
foreach ($port in 5077, 5173) {
  Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue |
    Select-Object -ExpandProperty OwningProcess -Unique |
    ForEach-Object { Stop-Process -Id $_ -Force -ErrorAction SilentlyContinue }
}

# Backend
Write-Host "Starting backend on http://localhost:5077 ..."
$backend = Start-Process -FilePath "dotnet" -ArgumentList "run" -WorkingDirectory $BackendDir `
  -PassThru -WindowStyle Hidden `
  -RedirectStandardOutput (Join-Path $Root "backend.log") `
  -RedirectStandardError  (Join-Path $Root "backend.err.log")
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ASPNETCORE_URLS = "http://localhost:5077"
Write-Host "  Backend PID:  $($backend.Id)  (log: backend.log)"

Write-Host "Waiting for backend to be ready ..."
for ($i = 0; $i -lt 40; $i++) {
  try {
    $r = Invoke-WebRequest -Uri "http://localhost:5077/swagger/index.html" -UseBasicParsing -TimeoutSec 2
    if ($r.StatusCode -eq 200) { Write-Host "  Backend is up."; break }
  } catch { Start-Sleep -Seconds 1 }
}

# Frontend
if (-not (Test-Path (Join-Path $FrontendDir "node_modules"))) {
  Write-Host "Installing frontend dependencies ..."
  Push-Location $FrontendDir
  if (Test-Path "package-lock.json") { npm ci } else { npm install }
  Pop-Location
}

Write-Host "Starting frontend on http://localhost:5173 ..."
$frontend = Start-Process -FilePath "npm" -ArgumentList "run", "dev" -WorkingDirectory $FrontendDir `
  -PassThru -WindowStyle Hidden `
  -RedirectStandardOutput (Join-Path $Root "frontend.log") `
  -RedirectStandardError  (Join-Path $Root "frontend.err.log")
Write-Host "  Frontend PID: $($frontend.Id)  (log: frontend.log)"

Start-Sleep -Seconds 3
Write-Host ""
Write-Host "Wellness House is ready."
Write-Host "------------------------------------------------"
Write-Host "  Admin / Staff app:  http://localhost:5173"
Write-Host "  Client portal:      http://localhost:5173/portal/dashboard"
Write-Host "  Swagger API docs:   http://localhost:5077/swagger"
Write-Host "------------------------------------------------"
Write-Host "  Admin login:    admin@wellness.com / Admin123!"
Write-Host "  Client login:   client@wellness.com / Client123!"
Write-Host "  Therapist:      therapist@wellness.com / Therapist123!"
Write-Host ""
Write-Host "To stop both servers:  Stop-Process -Id $($backend.Id), $($frontend.Id)"

Start-Process "http://localhost:5173"
