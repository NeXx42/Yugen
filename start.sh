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

  kill -TERM "$API_PID" 2>/dev/null || true
  kill -TERM "$WEB_PID" 2>/dev/null || true

  wait "$API_PID" 2>/dev/null || true
  wait "$WEB_PID" 2>/dev/null || true

  echo "Shutdown complete"
}

trap shutdown INT TERM

while kill -0 "$API_PID" 2>/dev/null && kill -0 "$WEB_PID" 2>/dev/null
do
  sleep 1
done

echo "One service exited, shutting down container..."
shutdown