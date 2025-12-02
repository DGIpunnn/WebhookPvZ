#!/bin/bash

# Build script for PvZFusionWebhookPlugin
echo "Building PvZFusionWebhookPlugin..."

# Create output directory
mkdir -p ./bin/Debug/net46

# Check if .NET SDK is available
if ! command -v dotnet &> /dev/null; then
    echo "Error: .NET SDK is not installed or not in PATH"
    exit 1
fi

# Restore packages
echo "Restoring packages..."
dotnet restore

# Build the project
echo "Building project..."
dotnet build --configuration Debug

if [ $? -eq 0 ]; then
    echo "Build successful!"
    echo "Output files are located in ./bin/Debug/net46/"
    ls -la ./bin/Debug/net46/
else
    echo "Build failed!"
    exit 1
fi