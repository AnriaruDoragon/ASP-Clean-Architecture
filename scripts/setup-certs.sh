#!/bin/bash
# Generate mkcert certificates for local HTTPS development
# Usage: ./scripts/setup-certs.sh

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$SCRIPT_DIR/.."
CERT_DIR="$ROOT_DIR/docker/certs"
ENV_FILE="$ROOT_DIR/.env"

# Load API_DOMAIN from .env file if it exists
API_DOMAIN="api.aspclean.localhost"
if [ -f "$ENV_FILE" ]; then
    ENV_DOMAIN=$(grep "^API_DOMAIN=" "$ENV_FILE" 2>/dev/null | cut -d'=' -f2 | tr -d ' ')
    if [ -n "$ENV_DOMAIN" ]; then
        API_DOMAIN="$ENV_DOMAIN"
    fi
fi

# Extract base domain (remove api. prefix if present)
DOMAIN="${API_DOMAIN#api.}"

echo "Setting up mkcert certificates for local HTTPS..."
echo "  Domain: $API_DOMAIN"

# Create certs directory
mkdir -p "$CERT_DIR"

# Check if mkcert is installed
if ! command -v mkcert &> /dev/null; then
    echo "Error: mkcert is not installed."
    echo ""
    echo "Please install mkcert first:"
    echo "  - macOS:  brew install mkcert"
    echo "  - Linux:  See https://github.com/FiloSottile/mkcert#installation"
    echo ""
    echo "After installing, run this script again."
    exit 1
fi

# Install local CA if not already done
echo "Installing local CA (you may be prompted for your password)..."
mkcert -install

# Generate certificates
echo "Generating certificates..."
cd "$CERT_DIR"
mkcert -cert-file "local.pem" -key-file "local-key.pem" \
    "${DOMAIN}" \
    "*.${DOMAIN}" \
    "api.${DOMAIN}" \
    "localhost" \
    "127.0.0.1" \
    "::1"

echo ""
echo "Certificates generated successfully!"
echo "  Location: $CERT_DIR"
echo "  Files:"
echo "    - local.pem"
echo "    - local-key.pem"

# Add hosts entry
HOSTS_FILE="/etc/hosts"
HOST_ENTRY="127.0.0.1 ${API_DOMAIN}"

echo ""
echo "Configuring hosts file..."

if grep -q "${API_DOMAIN}" "$HOSTS_FILE" 2>/dev/null; then
    echo "  Hosts entry already exists: $HOST_ENTRY"
else
    if [ -w "$HOSTS_FILE" ]; then
        echo "$HOST_ENTRY" >> "$HOSTS_FILE"
        echo "  Added hosts entry: $HOST_ENTRY"
    elif command -v sudo &> /dev/null; then
        echo "  Adding hosts entry (sudo required)..."
        echo "$HOST_ENTRY" | sudo tee -a "$HOSTS_FILE" > /dev/null
        echo "  Added hosts entry: $HOST_ENTRY"
    else
        echo "  Cannot add hosts entry - no write permission"
        echo ""
        echo "  Please add manually to $HOSTS_FILE:"
        echo "    $HOST_ENTRY"
    fi
fi

echo ""
echo "Setup complete! Start the development environment:"
echo "  task docker:up"
