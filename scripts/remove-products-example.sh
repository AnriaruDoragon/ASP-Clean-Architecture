#!/bin/bash
# Remove Products Example Feature
# Run from repository root: ./scripts/remove-products-example.sh

set -e

echo -e "\033[36mRemoving Products example feature...\033[0m"

# Files and folders to delete
delete_if_exists() {
    if [ -e "$1" ]; then
        rm -rf "$1"
        echo -e "  \033[33mDeleted: $1\033[0m"
    fi
}

# Delete migration files (glob pattern)
for f in Infrastructure/Data/Migrations/*_Products.cs Infrastructure/Data/Migrations/*_Products.Designer.cs; do
    delete_if_exists "$f"
done

# Delete other files and folders
delete_if_exists "Infrastructure/Data/Configs/ProductConfiguration.cs"
delete_if_exists "Application/Features/Products"
delete_if_exists "Web.API/Controllers/V1/ProductsController.cs"
delete_if_exists "Domain/Entities/Product.cs"
delete_if_exists "Domain/Events/ProductCreatedEvent.cs"
delete_if_exists "Domain/Events/ProductOutOfStockEvent.cs"

# Remove Products DbSet from IApplicationDbContext
INTERFACE_PATH="Application/Common/Interfaces/IApplicationDbContext.cs"
if [ -f "$INTERFACE_PATH" ]; then
    sed -i.bak '/\/\/ Example/,/public DbSet<Product> Products/d' "$INTERFACE_PATH"
    sed -i.bak '/using Domain\.Entities;/d' "$INTERFACE_PATH"
    rm -f "$INTERFACE_PATH.bak"
    echo -e "  \033[32mUpdated: $INTERFACE_PATH\033[0m"
fi

# Remove Products DbSet from ApplicationDbContext
CONTEXT_PATH="Infrastructure/Data/ApplicationDbContext.cs"
if [ -f "$CONTEXT_PATH" ]; then
    sed -i.bak '/\/\/ Example/,/public DbSet<Product> Products/d' "$CONTEXT_PATH"
    rm -f "$CONTEXT_PATH.bak"
    echo -e "  \033[32mUpdated: $CONTEXT_PATH\033[0m"
fi

echo -e "\n\033[32mProducts example removed successfully!\033[0m"
echo -e "\033[36mNext steps:\033[0m"
echo "  1. Run 'dotnet build' to verify no errors"
echo "  2. Run 'dotnet ef migrations add Initial --project Infrastructure --startup-project Web.API --output-dir Data/Migrations' to create fresh migration"
echo "  3. Delete the old Auth migration if starting fresh, or keep it for the auth tables"

# Self-destruct: remove both scripts and the scripts folder if empty
SCRIPTS_DIR="$(dirname "$0")"
rm -f "$SCRIPTS_DIR/remove-products-example.ps1"
rm -f "$SCRIPTS_DIR/remove-products-example.sh"
rmdir "$SCRIPTS_DIR" 2>/dev/null || true
echo -e "\n\033[90mCleanup scripts removed.\033[0m"
