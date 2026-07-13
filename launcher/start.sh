#!/usr/bin/env bash
# ThesisBuilder one-click launcher
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"
cd "$PROJECT_DIR"

GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

BACKEND_PORT=8000
FRONTEND_PORT=3000
PYTHON=~/.venvs/thesis-builder/bin/python

cleanup() {
    echo -e "\n${YELLOW}Stopping...${NC}"
    [ -n "$BACKEND_PID" ] && kill $BACKEND_PID 2>/dev/null
    [ -n "$FRONTEND_PID" ] && kill $FRONTEND_PID 2>/dev/null
    wait 2>/dev/null
    echo -e "${GREEN}Stopped${NC}"
    exit 0
}
trap cleanup SIGINT SIGTERM

echo "=== ThesisBuilder ==="
lsof -ti:$BACKEND_PORT | xargs kill 2>/dev/null || true
lsof -ti:$FRONTEND_PORT | xargs kill 2>/dev/null || true
sleep 0.5

echo "[1/2] Backend..."
$PYTHON run_server.py &
BACKEND_PID=$!
for i in $(seq 1 15); do
    curl -s http://localhost:$BACKEND_PORT/api/config/default > /dev/null 2>&1 && echo "  OK" && break
    sleep 1
done

echo "[2/2] Frontend..."
cd frontend
node node_modules/vite/bin/vite.js --host 0.0.0.0 --port $FRONTEND_PORT &
FRONTEND_PID=$!
cd "$PROJECT_DIR"
for i in $(seq 1 10); do
    curl -s http://localhost:$FRONTEND_PORT > /dev/null 2>&1 && echo "  OK" && break
    sleep 1
done

echo ""
echo -e "${GREEN}Frontend: http://localhost:$FRONTEND_PORT${NC}"
echo -e "${GREEN}Backend:  http://localhost:$BACKEND_PORT${NC}"
echo "Ctrl+C to stop"
echo ""

command -v cmd.exe > /dev/null 2>&1 && cmd.exe /c start http://localhost:$FRONTEND_PORT 2>/dev/null
wait
