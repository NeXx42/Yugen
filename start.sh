#!/bin/bash
set -u

prefix() {
  local tag="$1" line
  while IFS= read -r line || [ -n "$line" ]; do
    printf '[%s] %s\n' "$tag" "$line"
  done
}

dotnet /app/backend/Yugen.Api.dll \
  > >(prefix api) \
  2> >(prefix api >&2) &
API_PID=$!

node /app/frontend/server.js \
  > >(prefix web) \
  2> >(prefix web >&2) &
WEB_PID=$!

shutdown() {
  trap - TERM INT
  kill -TERM "$API_PID" "$WEB_PID" 2>/dev/null
  wait "$API_PID" "$WEB_PID" 2>/dev/null
}
trap shutdown TERM INT

wait -n "$API_PID" "$WEB_PID"
EXIT_CODE=$?

echo "[start] a service exited with code $EXIT_CODE, stopping the other" >&2
shutdown
exit "$EXIT_CODE"