#!/bin/sh
set -e

echo "Starting ASP.NET API..."
dotnet /app/backend/Yugen.Api.dll &
API_PID=$!

echo "Starting Next.js..."
node /app/frontend/server.js &
WEB_PID=$!

shutdown() {
  echo "Stopping services..."

  kill -TERM "$API_PID" 2>/dev/null
  kill -TERM "$WEB_PID" 2>/dev/null

  wait "$API_PID" 2>/dev/null
  wait "$WEB_PID" 2>/dev/null

  echo "Shutdown complete"
}

trap shutdown INT TERM

wait -n "$API_PID" "$WEB_PID"

echo "One service exited, shutting down container..."
shutdown