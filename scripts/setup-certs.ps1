# Generate mkcert certificates for local HTTPS development
# Usage: .\scripts\setup-certs.ps1

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir = Join-Path $ScriptDir ".."
$CertDir = Join-Path $RootDir "docker\certs"
$EnvFile = Join-Path $RootDir ".env"

# Load API_DOMAIN from .env file if it exists
$ApiDomain = "api.aspclean.localhost"
if (Test-Path $EnvFile) {
    $EnvContent = Get-Content $EnvFile | Where-Object { $_ -match "^API_DOMAIN=" }
    if ($EnvContent) {
        $ApiDomain = ($EnvContent -split "=", 2)[1].Trim()
    }
}

# Extract base domain (remove api. prefix if present)
$Domain = $ApiDomain -replace "^api\.", ""

Write-Host "Setting up mkcert certificates for local HTTPS..." -ForegroundColor Cyan
Write-Host "  Domain: $ApiDomain" -ForegroundColor Cyan

# Create certs directory
New-Item -ItemType Directory -Force -Path $CertDir | Out-Null

# Check if mkcert is installed
if (-not (Get-Command mkcert -ErrorAction SilentlyContinue)) {
    Write-Host "Error: mkcert is not installed." -ForegroundColor Red
    Write-Host ""
    Write-Host "Please install mkcert first:"
    Write-Host "  - Using Chocolatey: choco install mkcert"
    Write-Host "  - Using Scoop:      scoop bucket add extras && scoop install mkcert"
    Write-Host "  - Using winget:     winget install FiloSottile.mkcert"
    Write-Host "  - Manual:           https://github.com/FiloSottile/mkcert#installation"
    Write-Host ""
    Write-Host "After installing, run this script again."
    exit 1
}

# Install local CA if not already done
Write-Host "Installing local CA (you may be prompted for admin access)..." -ForegroundColor Yellow
mkcert -install

# Generate certificates
Write-Host "Generating certificates..." -ForegroundColor Yellow
Push-Location $CertDir
try {
    mkcert -cert-file "$Domain.pem" -key-file "$Domain-key.pem" `
        $Domain `
        "*.$Domain" `
        "api.$Domain" `
        "localhost" `
        "127.0.0.1" `
        "::1"
}
finally {
    Pop-Location
}

Write-Host ""
Write-Host "Certificates generated successfully!" -ForegroundColor Green
Write-Host "  Location: $CertDir"
Write-Host "  Files:"
Write-Host "    - $Domain.pem"
Write-Host "    - $Domain-key.pem"

# Add hosts entry
$HostsFile = "C:\Windows\System32\drivers\etc\hosts"
$HostEntry = "127.0.0.1 $ApiDomain"

Write-Host ""
Write-Host "Configuring hosts file..." -ForegroundColor Yellow

# Check if entry already exists (match the domain anywhere in the file)
$HostsLines = Get-Content $HostsFile -ErrorAction SilentlyContinue
$EntryExists = $HostsLines | Where-Object { $_ -match "\s$([regex]::Escape($ApiDomain))(\s|$)" -or $_ -match "^$([regex]::Escape($ApiDomain))(\s|$)" }

if ($EntryExists) {
    Write-Host "  Hosts entry already exists for $ApiDomain" -ForegroundColor Green
} else {
    # Check if running as admin
    $IsAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

    if ($IsAdmin) {
        Add-Content -Path $HostsFile -Value $HostEntry
        Write-Host "  Added hosts entry: $HostEntry" -ForegroundColor Green
    } else {
        Write-Host "  Cannot add hosts entry - not running as Administrator" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "  To add manually, run PowerShell as Administrator and execute:" -ForegroundColor White
        Write-Host "    Add-Content -Path '$HostsFile' -Value '$HostEntry'" -ForegroundColor White
        Write-Host ""
        Write-Host "  Or re-run this script as Administrator to add automatically."
    }
}

Write-Host ""
Write-Host "Setup complete! Start the development environment:" -ForegroundColor Cyan
Write-Host "  task docker:up" -ForegroundColor White
