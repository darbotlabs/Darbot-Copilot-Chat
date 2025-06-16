#!/usr/bin/env bash

# Initializes and runs both the backend and frontend for Copilot Chat.

set -e

ScriptDir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$ScriptDir"

# Function to log messages with timestamp
log() {
    echo "$(date '+%Y-%m-%d %H:%M:%S') - $1"
}

# Function to check if required commands are available
check_requirements() {
    local missing_tools=()
    
    if ! command -v nc &> /dev/null; then
        missing_tools+=("nc (netcat)")
    fi
    
    if ! command -v dotnet &> /dev/null; then
        missing_tools+=("dotnet")
    fi
    
    if ! command -v yarn &> /dev/null && ! command -v npm &> /dev/null; then
        missing_tools+=("yarn or npm")
    fi
    
    if [ ${#missing_tools[@]} -gt 0 ]; then
        log "ERROR: Missing required tools: ${missing_tools[*]}"
        log "Please install the missing tools and try again."
        exit 1
    fi
}

# Check for required tools
check_requirements

# get the port from the REACT_APP_BACKEND_URI env variable
if [ ! -f "../webapp/.env" ]; then
    log "WARNING: webapp/.env file not found. Using default port 40443."
    backendPort=40443
else
    envContent=$(grep -v '^#' ../webapp/.env 2>/dev/null | xargs || true)
    backendPort=$(echo "$envContent" | sed -n 's/.*:\([0-9]*\).*/\1/p')
    if [ -z "$backendPort" ]; then
        log "WARNING: Could not parse backend port from .env file. Using default port 40443."
        backendPort=40443
    fi
fi

log "Using backend port: $backendPort"

# Start backend (in background)
log "Starting backend..."
./start-backend.sh &
backend_pid=$!

# Function to cleanup background processes on exit
cleanup() {
    log "Cleaning up background processes..."
    if [ ! -z "$backend_pid" ] && kill -0 "$backend_pid" 2>/dev/null; then
        kill "$backend_pid" 2>/dev/null || true
    fi
}

trap cleanup EXIT

maxRetries=5
retryCount=0
retryWait=5  # set the number of seconds to wait before retrying

log "Waiting for backend to start on port $backendPort..."
# while the backend is not running wait.
while [ $retryCount -lt $maxRetries ]
do
  if nc -z localhost "$backendPort"
  then
    # if the backend is running, start the frontend and break out of the loop
    log "Backend is running. Starting frontend..."
    ./start-frontend.sh
    break
  else
    # if the backend is not running, sleep, then increment the retry count
    log "Backend not ready yet. Waiting $retryWait seconds... (attempt $((retryCount+1))/$maxRetries)"
    sleep $retryWait
    retryCount=$((retryCount+1))
  fi
done

# if we have exceeded the number of retries
if [ $retryCount -eq $maxRetries ]
then
# write to the console that the backend is not running and we have exceeded the number of retries and we are exiting
  log "ERROR: Backend failed to start after $maxRetries attempts."
  log "Please check the backend logs for errors."
  log "You can try running './start-backend.sh' manually to see detailed error messages."
  exit 1
fi
