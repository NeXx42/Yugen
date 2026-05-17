#!/bin/sh

echo "Starting ASP.NET API..."
dotnet /app/backend/Yugen.Api.dll &
API_PID=$!

echo "Starting Next.js..."
cd /app/frontend
npm run start &
WEB_PID=$!

term_handler() {
  echo "Stopping services..."
  kill $API_PID $WEB_PID 2>/dev/null
  exit 0
}

trap term_handler INT TERM

wait