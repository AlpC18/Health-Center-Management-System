# 📋 Master Trello Board: Wellness House Project

This document provides a highly technical and exceptionally detailed breakdown of Trello tasks, uniquely tailored to the current codebase (React/Zustand + ASP.NET Core 8 + Entity Framework + Docker + SignalR). 

Assign these tasks in your Trello / Kanban board. Each member must complete their checklist prior to the final presentation/deployment.

---

## 👩‍💻 1. Lumturie Hyseni — Frontend Architecture & UI/UX

**Domain:** React Components, TailwindCSS, Client Portal, and User Flow.
**Main Objective:** Ensure the user interface is completely bug-free, perfectly localized, and visually premium for the demonstration.

### 📝 Task 1: Refactor Calendar Interactivity (`CalendarPage.jsx`)
- [ ] **Current State:** The `FullCalendar` component uses a basic browser `alert(...)` when an appointment event is clicked.
- [ ] **Action:** Remove the `alert()`. Import the custom `Modal` component from `../ui/index` and trigger a sleek Tailwind modal showing detailed information (Therapist Name, Service, Status color codes).
- [ ] **Code Target:** `src/pages/CalendarPage.jsx` inside the `eventClick` property.

### 📝 Task 2: Fix CSV Export Pagination Logic (`CrudPage.jsx`)
- [ ] **Current State:** The new "Export to CSV" button exports `items`. Due to API-side pagination (`LIMIT = 10`), it only exports the current visible page.
- [ ] **Action:** Modify the `handleExportCSV` function. Write logic to either: 1) Call API without `limit` just for export, OR 2) Show a toast: `Note: Exporting current page only`.
- [ ] **Code Target:** `handleExportCSV()` in `src/components/crud/CrudPage.jsx`.

### 📝 Task 3: Global State & Translations (`langStore.js` & `themeStore.js`)
- [ ] **Current State:** `sq` and `en` keys might have missing translation strings.
- [ ] **Action:** Audit `src/i18n/` dictionary files. Ensure that changing languages dynamically updates the new `ChatWidget.jsx` interface.
- [ ] **Action:** Perform a strict CSS audit in `Layout.jsx` and `PortalLayout.jsx` ensuring that when `.dark` mode is triggered, the inputs in the Create Modals do not have invisible black text on dark gray backgrounds.

---

## 👨‍💻 2. Andi Vrajolli — Backend API, Auth & Business Logic

**Domain:** ASP.NET Core, EF Core (SQLite), Controllers, JWT Authentication.
**Main Objective:** Guarantee API security, backend stability, and database relationships.

### 📝 Task 1: Advanced Appointment Validation (Overlap Detection)
- [ ] **Current State:** We can create `Terminet` (Appointments) via standard POST endpoints.
- [ ] **Action:** Review `TerminetController.cs` or FluentValidation rules. Are we checking if two appointments for the same `TerapistId` are scheduled at the exact same `OraFillimit` (Start Time)? If not, add overlapping detection logic that returns a `400 Bad Request`.
- [ ] **Code Target:** `WellnessAPI/Controllers/TerminetController.cs`.

### 📝 Task 2: Background Reminder Optimization (`AppointmentReminderService.cs`)
- [ ] **Current State:** The new background worker checks appointments tomorrow and logs an "email sent" message every 12 hours.
- [ ] **Action:** The 12-hour tick might execute during the night. Change `Task.Delay` logic to run exactly daily at `08:00 AM` using a `Cron`-like delay logic or a timer.
- [ ] **Action:** Inject the `IConfiguration` service into the constructor to read an `EnableReminders` boolean from `appsettings.json` so you can disable the background service via environment variables if needed.

### 📝 Task 3: Security & Interceptor Testing (`TokenService.cs` & `api.js`)
- [ ] **Current State:** The token is sent back on Login, but what happens when it expires after 15 minutes?
- [ ] **Action:** Verify how the frontend Axios interceptor (`src/api/api.js`) handles a `401 Unauthorized` response. Does it trigger `authApi.refreshToken()` automatically or force logout? Analyze and fix any edge cases.
- [ ] **Action:** Document the exact SQL script process needed to export `wellness.db` tables.

---

## 👨‍💻 3. Rilind Pirana — Real-time Comms, Docker & DevOps

**Domain:** Docker Compose, SignalR web-sockets, CI/CD, Deployment.
**Main Objective:** Ensure isolated infrastructure, perfect websocket networking, and deployment readiness.

### 📝 Task 1: Persistent History for SignalR Chat
- [ ] **Current State:** The `ChatWidget.jsx` connects to `NotificationHub.cs` and messages broadcast live. However, if a user refreshes the page, the chat history is lost (because SignalR only pushes events).
- [ ] **Action:** Architect a database table `ChatMessages`. Update `NotificationHub.SendMessage()` to `await _db.ChatMessages.AddAsync(...)`. Then, create a `GET /api/chat/history` endpoint so `ChatWidget` can fetch the last 50 messages during its `useEffect()` mount hook.
- [ ] **Code Target:** `WellnessAPI/Hubs/NotificationHub.cs` and frontend `ChatWidget.jsx`.

### 📝 Task 2: Docker Infrastructure Audit (`docker-compose.yml`)
- [ ] **Current State:** The container uses a relative bind mount (`./WellnessAPI/wellness.db:/app/wellness.db`). 
- [ ] **Action:** If this is built on a fresh server, the empty file mount might cause SQLite to fail if EF migration isn't run. Update the `Dockerfile` to automatically run `dotnet ef database update` on initial container startup, OR switch to a Named Volume that allows internal DB creation.
- [ ] **Action:** Verify the Nginx routing in `frontend/Dockerfile`. It uses `try_files $uri /index.html` which is perfect for React Router, but ensure the port mapping `5173:80` isn't blocked on the host production machine.

### 📝 Task 3: Production Build Performance
- [ ] **Current State:** Code is ready, but chunking needs validation.
- [ ] **Action:** Run `npm run build`. Inspect the Vite output logs to see if any chunk is over `500kb`. Because `App.jsx` uses `React.lazy()` for Pages, it should be fine. Validate that the React code is fully tree-shaken and runs optimally in the browser bundle. Prepare this metric for the presentation!
