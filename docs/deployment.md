# Deployment

## Docker Compose (Production)

```bash
# Configure production values in .env
task prod:up     # Build and start production stack
task prod:logs   # View logs
task prod:down   # Stop
```

**Production services:**
- **PostgreSQL 18** — Database (internal network only)
- **Redis 8** — Distributed caching (internal network only, optional)
- **API** — ASP.NET Core application
- **Nginx** — Reverse proxy with HTTPS termination

## VM/EC2 Deployment

Build a self-contained package for deployment to VMs:

```bash
# Build for Linux x64 (default)
task deploy:build

# Build for different runtime
task deploy:build DEPLOY_RUNTIME=linux-arm64  # ARM64 (AWS Graviton)
task deploy:build DEPLOY_RUNTIME=win-x64      # Windows

# Create deployment package
task deploy:package          # Creates .tar.gz (Linux/Mac)
task deploy:package:windows  # Creates .zip (Windows)
```

### Systemd Service

Example unit file (`/etc/systemd/system/myapp.service`):

```ini
[Unit]
Description=ASP.NET Clean Architecture API
After=network.target

[Service]
Type=notify
User=www-data
WorkingDirectory=/opt/myapp
ExecStart=/opt/myapp/Web.API
Restart=always
RestartSec=10
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5000

[Install]
WantedBy=multi-user.target
```

```bash
sudo systemctl enable myapp
sudo systemctl start myapp
sudo systemctl status myapp
```
