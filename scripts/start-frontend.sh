#!/usr/bin/env bash

# Builds and runs the Chat Copilot frontend.

set -e

# Function to log messages with timestamp
log() {
    echo "$(date '+%Y-%m-%d %H:%M:%S') - $1"
}

# Function to check if required commands are available
check_requirements() {
    if ! command -v yarn &> /dev/null && ! command -v npm &> /dev/null; then
        log "ERROR: Neither yarn nor npm found. Please install one of them."
        exit 1
    fi
}

# Check for required tools
check_requirements

SCRIPT_DIRECTORY="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIRECTORY/../webapp"

# Validate webapp directory
if [ ! -f "package.json" ]; then
    log "ERROR: package.json not found in $SCRIPT_DIRECTORY/../webapp"
    exit 1
fi

# Check if .env file exists
if [ ! -f ".env" ]; then
    log "WARNING: .env file not found. Frontend may not connect to backend correctly."
    log "Please run '../scripts/configure.sh' to set up the environment."
fi

# Build and run the frontend application
log "Installing frontend dependencies..."
if command -v yarn &> /dev/null; then
    yarn install
    log "Starting frontend development server..."
    yarn start
else
    npm install
    log "Starting frontend development server..."
    npm start
fi
