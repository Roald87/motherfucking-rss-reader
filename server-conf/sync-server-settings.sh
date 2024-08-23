#!/bin/bash

# Script to copy server settings to the current directory

# Source files
NGINX_CONF="/etc/nginx/nginx.conf"
SYSTEMD_SERVICE="/etc/systemd/system/mfrssreader-server.service"

# Target directory (current directory)
TARGET_DIR=$(pwd)


cp -i "$NGINX_CONF" "$TARGET_DIR/nginx.conf"
cp -i "$SYSTEMD_SERVICE" "$TARGET_DIR/mfrssreader-server.service"
