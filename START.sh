#!/bin/bash
echo "🌿 Starting Wellness House..."

# Backend
echo "⚙️  Starting Backend..."
cd ~/Desktop/wellness-backend/WellnessAPI
dotnet run &
BACKEND_PID=$!
echo "Backend PID: $BACKEND_PID"

# Wait
sleep 5

# Frontend
echo "🎨 Starting Frontend..."
cd ~/Desktop/wellness-frontend
npm run dev &
FRONTEND_PID=$!
echo "Frontend PID: $FRONTEND_PID"

sleep 3

echo ""
echo "✅ Wellness House is ready!"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "🔧 Admin Panel:  http://localhost:5173"
echo "👤 Client Portal: http://localhost:5173/portal/dashboard"
echo "📚 API Docs:     http://localhost:5077/swagger"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "Admin: admin@wellness.com / Admin123!"
echo ""
echo "To stop: kill $BACKEND_PID $FRONTEND_PID"

# Open browser
sleep 2
open http://localhost:5173 2>/dev/null || xdg-open http://localhost:5173 2>/dev/null
