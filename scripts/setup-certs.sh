#!/bin/bash
# Generate mkcert certificates for local HTTPS development
# Usage: ./scripts/setup-certs.sh

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CERT_DIR="$SCRIPT_DIR/../docker/certs"
DOMAIN="aspclean.localhost"

echo "Setting up mkcert certificates for local HTTPS..."

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
mkcert -cert-file "${DOMAIN}.pem" -key-file "${DOMAIN}-key.pem" \
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
echo "    - ${DOMAIN}.pem"
echo "    - ${DOMAIN}-key.pem"
echo ""
echo "Next steps:"
echo "  1. Add this line to your hosts file:"
echo "     127.0.0.1 api.aspclean.localhost"
echo ""
echo "  Hosts file location:"
echo "    - Linux/Mac: /etc/hosts"
echo "    - Windows:   C:\\Windows\\System32\\drivers\\etc\\hosts"
echo ""
echo "  2. Start the development environment:"
echo "     task docker:up"
