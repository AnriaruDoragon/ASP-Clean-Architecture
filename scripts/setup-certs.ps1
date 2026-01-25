# Generate mkcert certificates for local HTTPS development
# Usage: .\scripts\setup-certs.ps1

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$CertDir = Join-Path $ScriptDir "..\docker\certs"
$Domain = "aspclean.localhost"

Write-Host "Setting up mkcert certificates for local HTTPS..." -ForegroundColor Cyan

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
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Add this line to your hosts file (run as Administrator):"
Write-Host "     127.0.0.1 api.aspclean.localhost" -ForegroundColor White
Write-Host ""
Write-Host "  Hosts file location:"
Write-Host "     C:\Windows\System32\drivers\etc\hosts"
Write-Host ""
Write-Host "  Quick command (run PowerShell as Administrator):"
Write-Host "     Add-Content -Path 'C:\Windows\System32\drivers\etc\hosts' -Value '127.0.0.1 api.aspclean.localhost'" -ForegroundColor White
Write-Host ""
Write-Host "  2. Start the development environment:"
Write-Host "     task docker:up" -ForegroundColor White
