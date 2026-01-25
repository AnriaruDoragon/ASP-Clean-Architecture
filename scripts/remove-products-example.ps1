# Remove Products Example Feature
# Run from repository root: .\scripts\remove-products-example.ps1

$ErrorActionPreference = "Stop"

Write-Host "Removing Products example feature..." -ForegroundColor Cyan

# Files and folders to delete
$toDelete = @(
    "Infrastructure/Data/Migrations/*_Products.cs",
    "Infrastructure/Data/Migrations/*_Products.Designer.cs",
    "Infrastructure/Data/Configs/ProductConfiguration.cs",
    "Application/Features/Products",
    "Web.API/Controllers/V1/ProductsController.cs",
    "Domain/Entities/Product.cs",
    "Domain/Events/ProductCreatedEvent.cs",
    "Domain/Events/ProductOutOfStockEvent.cs",
    # Tests
    "Tests/Application.UnitTests/Features/Products",
    "Tests/Domain.UnitTests/Entities/ProductTests.cs",
    "Tests/Web.API.IntegrationTests/ProductsControllerTests.cs"
)

foreach ($path in $toDelete) {
    $resolved = Resolve-Path $path -ErrorAction SilentlyContinue
    if ($resolved) {
        foreach ($item in $resolved) {
            if (Test-Path $item) {
                Remove-Item $item -Recurse -Force
                Write-Host "  Deleted: $item" -ForegroundColor Yellow
            }
        }
    }
}

# Remove Products DbSet from IApplicationDbContext (keep using Domain.Entities; for other entities)
$interfacePath = "Application/Common/Interfaces/IApplicationDbContext.cs"
if (Test-Path $interfacePath) {
    $content = Get-Content $interfacePath -Raw
    $content = $content -replace "(?m)^\s*// Example\r?\n\s*public DbSet<Product> Products \{ get; \}\r?\n", ""
    Set-Content $interfacePath $content -NoNewline
    Write-Host "  Updated: $interfacePath" -ForegroundColor Green
}

# Remove Products DbSet from ApplicationDbContext
$contextPath = "Infrastructure/Data/ApplicationDbContext.cs"
if (Test-Path $contextPath) {
    $content = Get-Content $contextPath -Raw
    $content = $content -replace "(?m)^\s*// Example\r?\n\s*public DbSet<Product> Products => Set<Product>\(\);\r?\n", ""
    Set-Content $contextPath $content -NoNewline
    Write-Host "  Updated: $contextPath" -ForegroundColor Green
}

Write-Host "`nProducts example removed successfully!" -ForegroundColor Green
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Run 'dotnet build' to verify no errors"
Write-Host "  2. Run 'dotnet ef migrations add Initial --project Infrastructure --startup-project Web.API --output-dir Data/Migrations' to create fresh migration"
Write-Host "  3. Delete the old Auth migration if starting fresh, or keep it for the auth tables"

# Self-destruct: remove both scripts and the scripts folder if empty
$scriptsFolder = Split-Path $PSCommandPath -Parent
Remove-Item "$scriptsFolder/remove-products-example.ps1" -Force -ErrorAction SilentlyContinue
Remove-Item "$scriptsFolder/remove-products-example.sh" -Force -ErrorAction SilentlyContinue
if ((Get-ChildItem $scriptsFolder -ErrorAction SilentlyContinue | Measure-Object).Count -eq 0) {
    Remove-Item $scriptsFolder -Force
}
Write-Host "`nCleanup scripts removed." -ForegroundColor Gray
