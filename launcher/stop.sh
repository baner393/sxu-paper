#!/usr/bin/env bash
echo "Stopping..."
lsof -ti:8000 | xargs kill 2>/dev/null && echo "  Backend stopped" || echo "  Backend not running"
lsof -ti:3000 | xargs kill 2>/dev/null && echo "  Frontend stopped" || echo "  Frontend not running"
echo "Done"
