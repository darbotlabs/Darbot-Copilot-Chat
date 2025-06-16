#!/usr/bin/env bash

# Builds and runs the Chat Copilot backend.

set -e

# Function to log messages with timestamp
log() {
    echo "$(date '+%Y-%m-%d %H:%M:%S') - $1"
}

# Function to check if required commands are available
check_requirements() {
    if ! command -v dotnet &> /dev/null; then
        log "ERROR: dotnet command not found. Please install .NET SDK."
        exit 1
    fi
}

# Check for required tools
check_requirements

# Stop any existing backend API process
log "Stopping any existing backend processes..."
while pid=$(pgrep CopilotChatWebA 2>/dev/null); do
    log "Found existing process with PID: $pid"
    echo $pid | sed 's/\([0-9]\{4\}\) .*/\1/' | xargs kill
    sleep 1
done

# Get defaults and constants
SCRIPT_DIRECTORY="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Check if .env file exists
if [ ! -f "$SCRIPT_DIRECTORY/.env" ]; then
    log "ERROR: .env file not found at $SCRIPT_DIRECTORY/.env"
    log "Please run './configure.sh' first to set up the environment."
    exit 1
fi

. "$SCRIPT_DIRECTORY/.env"
cd "$SCRIPT_DIRECTORY/../webapi"

# Validate webapi directory
if [ ! -f "CopilotChatWebApi.csproj" ]; then
    log "ERROR: CopilotChatWebApi.csproj not found in $SCRIPT_DIRECTORY/../webapi"
    exit 1
fi

# Environment variable `ASPNETCORE_ENVIRONMENT` required to override appsettings.json with
# appsettings.$ENV_ASPNETCORE.json. See `webapi/ConfigurationExtensions.cs`
export ASPNETCORE_ENVIRONMENT=$ENV_ASPNETCORE
log "Using ASPNETCORE_ENVIRONMENT: $ASPNETCORE_ENVIRONMENT"

# Build and run the backend API server
log "Building backend..."
if ! dotnet build; then
    log "ERROR: Failed to build backend"
    exit 1
fi

log "Starting backend server..."
dotnet run
