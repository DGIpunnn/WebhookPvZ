#!/bin/bash

# Build script for PvZ Fusion Webhook Plugin
echo "Building PvZFusionWebhookPlugin..."

# Change to the plugin directory
cd "$(dirname "$0")"

# Build the project
dotnet build

if [ $? -eq 0 ]; then
    echo "Build successful!"
    echo "DLL location: bin/Debug/net46/PvZFusionWebhookPlugin.dll"
else
    echo "Build failed!"
    exit 1
fi