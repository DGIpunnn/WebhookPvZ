@echo off
REM Build script for PvZFusionWebhookPlugin

echo Building PvZFusionWebhookPlugin...

REM Create output directory
if not exist ".\bin\Debug" mkdir ".\bin\Debug"

REM Check if .NET SDK is available
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo Error: .NET SDK is not installed or not in PATH
    exit /b 1
)

REM Restore packages
echo Restoring packages...
dotnet restore

REM Build the project
echo Building project...
dotnet build --configuration Debug --output .\bin\Debug

if errorlevel 0 (
    echo Build successful!
    echo Output files are located in .\bin\Debug\
    dir .\bin\Debug\
) else (
    echo Build failed!
    exit /b 1
)