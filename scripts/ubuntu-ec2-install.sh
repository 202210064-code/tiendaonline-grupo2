#!/usr/bin/env bash
set -euo pipefail
# Ubuntu 22.04/24.04, requiere salida HTTPS a repositorios desde la subred app.
sudo apt-get update
sudo apt-get install -y ca-certificates wget gnupg
. /etc/os-release
wget -q "https://packages.microsoft.com/config/ubuntu/${VERSION_ID}/packages-microsoft-prod.deb" -O /tmp/packages-microsoft-prod.deb
sudo dpkg -i /tmp/packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y aspnetcore-runtime-8.0
printf 'Runtime instalado. Publicar primero con dotnet publish -c Release -o publish desde tu equipo de desarrollo.\n'
