#!/bin/bash
# Wellness House — one-shot launcher for macOS / Linux
# Starts the .NET backend and the Vite frontend, then opens the browser.
set -e

# Resolve the directory this script lives in (works no matter where it is called from)
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BACKEND_DIR="$ROOT/wellness-backend/WellnessAPI"
FRONTEND_DIR="$ROOT/frontend"

# Make sure dotnet (installed via dotnet-install.sh in ~/.dotnet) is on PATH for this shell
export DOTNET_ROOT="${DOTNET_ROOT:-$HOME/.dotnet}"
export PATH="$DOTNET_ROOT:$DOTNET_ROOT/tools:$PATH"

echo "Wellness House — starting up..."
echo "  Repo:     $ROOT"
echo "  Backend:  $BACKEND_DIR"
echo "  Frontend: $FRONTEND_DIR"
echo ""

# Free up the ports if a previous run left something running
lsof -ti :5077 | xargs kill -9 2>/dev/null || true
lsof -ti :5173 | xargs kill -9 2>/dev/null || true

# Backend
echo "Starting backend on http://localhost:5077 ..."
(
  cd "$BACKEND_DIR"
  ASPNETCORE_ENVIRONMENT=Development \
  ASPNETCORE_URLS="http://localhost:5077" \
  dotnet run > "$ROOT/backend.log" 2>&1
) &
BACKEND_PID=$!
echo "  Backend PID:  $BACKEND_PID  (log: $ROOT/backend.log)"

# Wait until the backend is actually listening
echo "Waiting for backend to be ready ..."
for i in {1..40}; do
  if curl -ks -o /dev/null -w "%{http_code}" http://localhost:5077/swagger/index.html | grep -q 200; then
    echo "  Backend is up."
    break
  fi
  sleep 1
done

# Frontend
echo "Starting frontend on http://localhost:5173 ..."
(
  cd "$FRONTEND_DIR"
  npm run dev > "$ROOT/frontend.log" 2>&1
) &
FRONTEND_PID=$!
echo "  Frontend PID: $FRONTEND_PID  (log: $ROOT/frontend.log)"

# Wait until Vite is up
for i in {1..20}; do
  if curl -ks -o /dev/null -w "%{http_code}" http://localhost:5173/ | grep -q 200; then
    break
  fi
  sleep 1
done

echo ""
echo "Wellness House is ready."
echo "------------------------------------------------"
echo "  Admin / Staff app:  http://localhost:5173"
echo "  Client portal:      http://localhost:5173/portal/dashboard"
echo "  Swagger API docs:   http://localhost:5077/swagger"
echo "------------------------------------------------"
echo "  Admin login:    admin@wellness.com / Admin123!"
echo "  Client login:   client@wellness.com / Client123!"
echo "  Therapist:      therapist@wellness.com / Therapist123!"
echo ""
echo "To stop both servers:    kill $BACKEND_PID $FRONTEND_PID"
echo "To stop everything else: lsof -ti :5077 :5173 | xargs kill -9"
echo ""

# Open the app in the default browser (macOS = open, Linux = xdg-open)
sleep 1
(open http://localhost:5173 2>/dev/null || xdg-open http://localhost:5173 2>/dev/null) || true

# Keep this script in the foreground so Ctrl+C cleanly stops both children
trap "echo ''; echo 'Stopping...'; kill $BACKEND_PID $FRONTEND_PID 2>/dev/null; exit 0" INT TERM
wait
