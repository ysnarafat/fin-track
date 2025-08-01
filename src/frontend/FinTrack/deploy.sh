#!/bin/bash

# Configuration
REPO_PATH="/home/ubuntu/FinTrack"
BRANCH="main"

echo "🚀 Starting deployment at $(date)"

# Navigate to your project directory
cd $REPO_PATH || exit

# Fetch latest changes
echo "📥 Pulling latest changes from $BRANCH"
git fetch origin $BRANCH
git reset --hard origin/$BRANCH

# Build .NET backend
echo "🛠️ Building .NET backend"
cd backend/dotnet
dotnet restore
dotnet publish -c Release -o /var/www/FinTrack

# Restart the .NET service
sudo systemctl restart fintrack-dotnet.service

# Build Go backend
echo "🛠️ Building Go backend"
cd ../go
go build -o /usr/local/bin/fintrack

# Restart the Go service
sudo systemctl restart fintrack-go.service

# Build Blazor frontend
echo "🛠️ Building Blazor frontend"
cd ../../frontend
dotnet publish -c Release -o /var/www/FinTrackUI

# Restart Nginx (if used for Blazor hosting)
sudo systemctl reload nginx

echo "✅ Deployment completed successfully!"
