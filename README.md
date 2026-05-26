# Wellness House - Health Center Management System

![Wellness House Banner](https://via.placeholder.com/1200x300.png?text=Wellness+House+Management+System)

A comprehensive, state-of-the-art Health and Wellness Center Management System. This project features a dual-interface approach: an **Admin Dashboard** for clinic staff and a **Client Portal** for patients.

It is built with a robust **ASP.NET Core 9 Web API (C#)** backend and a dynamic **React 19 (Vite) + TailwindCSS** frontend.

---

## ⚡ Quick Start on macOS (Apple Silicon — M1/M2/M3/M4)

If you just want to get the app running on your Mac, follow these steps in order. They were verified end-to-end on macOS (Apple Silicon) with the repo at `~/Downloads/Health-Center-Management-System-main`.

### 0. One-time tool install

You need **Homebrew**, **Node.js (v18+)**, the **.NET 9 SDK**, and a **MySQL/MariaDB server** — use **XAMPP**, **Laragon**, or **Docker** (any one). Open Terminal and run:

```bash
# Homebrew (skip if you already have it — check with: brew --version)
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"

# Node.js (skip if `node --version` is already 18 or higher)
brew install node

# A database — pick ONE: XAMPP, Laragon, or Docker Desktop. e.g.:
brew install --cask docker            # Docker (launch it once), or
# brew install --cask xampp           # XAMPP, or install Laragon (Windows)

# .NET 9 SDK — installed to ~/.dotnet, no sudo needed
curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
chmod +x /tmp/dotnet-install.sh
/tmp/dotnet-install.sh --channel 9.0 --install-dir "$HOME/.dotnet"

# Make dotnet permanently available in new terminals
cat >> ~/.zshrc <<'EOF'

# .NET SDK (installed via dotnet-install.sh)
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$DOTNET_ROOT:$DOTNET_ROOT/tools:$PATH"
EOF
source ~/.zshrc

# EF Core CLI (used for migrations)
dotnet tool install --global dotnet-ef --version "9.*"
```

Verify everything is installed:

```bash
node --version   # → v18 or higher
npm --version    # → 9 or higher
dotnet --version # → 9.0.x
```

### 1. Start the database, then one-shot launch

Start a MySQL/MariaDB server using **any one** of these (all use port 3306, root, empty password):

```bash
# XAMPP:   open XAMPP and click "Start MySQL"
# Laragon: open Laragon and click "Start All"   (Windows)
# Docker:  cd wellness-backend && docker compose up -d && cd ..
```

Create a database named `wellnessdb` (phpMyAdmin / HeidiSQL → New → `wellnessdb`;
with Docker it's created automatically). Then launch the app from the repo root:

```bash
./START.sh
```

> The defaults already work with all three — no `.env` needed. Copy `.env.example` to `.env` only to override DB credentials.

`START.sh` cleans up old processes, starts the backend on port `5077`, waits for it to be ready, starts the frontend on port `5173`, and opens your browser. Logs are written to `backend.log` and `frontend.log` in the repo root.

> **Windows:** use `START.ps1` instead — open PowerShell in the repo root and run `./START.ps1`. Start your database first (Laragon "Start All", XAMPP "Start MySQL", or `docker compose up -d` in `wellness-backend`) and create the `wellnessdb` database.

### 2. Open the app

| URL | What it is |
|-----|------------|
| http://localhost:5173 | The web app (login screen) |
| http://localhost:5173/portal/dashboard | Client portal entry point (after Klient login) |
| http://localhost:5077/swagger | Interactive API docs (try the API in the browser) |

### 3. Test logins (already seeded)

| Role | Email | Password |
|------|-------|----------|
| Admin | `admin@wellness.com` | `Admin123!` |
| Therapist | `therapist@wellness.com` | `Therapist123!` |
| Klient (patient) | `client@wellness.com` | `Client123!` |

### 4. Stopping & restarting

```bash
# Stop everything (Ctrl+C in the terminal running START.sh, or:)
lsof -ti :5077 :5173 | xargs kill -9

# Start again any time
cd ~/Downloads/Health-Center-Management-System-main && ./START.sh
```

### 5. Environment variables you should know

The repo ships with safe development defaults. Copy `.env.example` to `.env` (repo root) to override any of these — `START.sh`/`START.ps1` load it automatically. The only secret you typically configure is the JWT signing key.

| Variable | Where | Purpose | Default for dev |
|----------|-------|---------|-----------------|
| `Jwt__Key` (env) or `Jwt:Key` (config) | backend | Signs JWT access tokens. **Must not be the placeholder.** | A real key has been put in `wellness-backend/WellnessAPI/appsettings.Development.json` (gitignored). |
| `ConnectionStrings__DefaultConnection` | backend | MySQL/MariaDB connection (engine auto-detected) | `Server=localhost;Port=3306;Database=wellnessdb;User=root;Password=;` (XAMPP/Laragon/Docker default) |
| `ASPNETCORE_URLS` | backend | What URL Kestrel listens on | `http://localhost:5077` |
| `VITE_API_BASE_URL` | frontend | Where the React app calls the API | `http://localhost:5077/api` |

To use your own JWT key (recommended for non-dev use):

```bash
# Inside wellness-backend/WellnessAPI
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "$(openssl rand -base64 48)"
```

### 6. macOS troubleshooting

| Symptom | Likely cause | Fix |
|---------|-------------|-----|
| `command not found: dotnet` | PATH not loaded in this shell | `source ~/.zshrc` (or open a new Terminal tab) |
| `Address already in use :5077` or `:5173` | Old run still alive | `lsof -ti :5077 :5173 \| xargs kill -9` |
| Browser shows "This site can't be reached" | Backend crashed during startup | Check `backend.log` in repo root, fix the error, run `./START.sh` again |
| `Jwt:Key must be provided ...` exception | `appsettings.Development.json` missing | Recreate it (see "Environment variables" above) or set `export Jwt__Key="$(openssl rand -base64 48)"` before `dotnet run` |
| Login page returns 500 | API can't reach MySQL | Start MySQL: `cd wellness-backend && docker compose up -d`. Reset data if needed: `docker compose down -v && docker compose up -d` |
| Apple Silicon: "bad CPU type" running a binary | An old x86 dotnet was installed via Rosetta | Remove `/usr/local/share/dotnet`, use the `~/.dotnet` arm64 build above |

### 7. Folder layout (what each thing is)

```
Health-Center-Management-System-main/
├── START.sh                      # one-shot launcher (backend + frontend + browser)
├── README.md                     # this file
├── DEMO_SCENARIO.md              # presentation/demo script for the lab project
├── docker-compose.yml            # optional Docker stack (not needed for local dev)
│
├── wellness-backend/             # the backend solution
│   ├── Wellness.sln              # .NET solution file
│   ├── MYSQL_SETUP.md            # MySQL setup guide
│   └── WellnessAPI/              # ASP.NET Core 9 Web API (the API project)
│       ├── Program.cs            # boots the app, configures DI, JWT, CORS, Swagger
│       ├── appsettings.json      # default config (committed)
│       ├── appsettings.Development.json  # local secrets (gitignored)
│       ├── Controllers/          # REST endpoints (one per entity + AuthController)
│       ├── Models/               # EF Core entities (Domain + Identity user)
│       ├── DTOs/                 # Data-transfer objects exposed to the client
│       ├── Data/                 # ApplicationDbContext + SeedData
│       ├── Migrations/           # EF Core migrations (MySQL)
│       ├── MigrationsMySql/      # MySQL-only migrations (excluded from build)
│       ├── Services/             # TokenService, AuditService, EmailService, etc.
│       ├── Validators/           # FluentValidation rules
│       ├── Middleware/           # ErrorHandling + SecurityHeaders middleware
│       └── Hubs/                 # SignalR NotificationHub (real-time)
│
└── frontend/                     # React 19 + Vite 8 + Tailwind 3 single-page app
    ├── package.json
    ├── vite.config.js            # Vite config + manual code splitting
    ├── tailwind.config.cjs
    └── src/
        ├── main.jsx              # React entrypoint
        ├── App.jsx               # router + role-based layout switch
        ├── api/                  # axios instance + per-entity API wrappers
        ├── components/           # shared UI building blocks + layouts
        ├── pages/                # Admin dashboard pages
        │   └── portal/           # Client-portal pages (separate role)
        ├── store/                # Zustand stores (auth, theme, lang, notifications)
        ├── hooks/                # custom React hooks
        └── i18n/                 # translations (sq / en)
```

### 8. Important commands cheat-sheet

```bash
# --- Run everything ---
./START.sh

# --- Backend only ---
cd wellness-backend/WellnessAPI
dotnet run                     # build + run
dotnet build                   # just build (faster check)
dotnet test ../WellnessAPI.Tests   # run xUnit tests
dotnet ef migrations add MyChange  # add a new migration
dotnet ef database update      # apply migrations to MySQL

# --- Frontend only ---
cd frontend
npm install                    # install deps (run once)
npm run dev                    # dev server with hot reload
npm run build                  # production build into dist/
npm run preview                # preview the production build
npm run lint                   # ESLint check

# --- Reset the local database ---
# XAMPP/Laragon: drop & recreate in phpMyAdmin/HeidiSQL, or:
#   mysql -u root -e "DROP DATABASE wellnessdb; CREATE DATABASE wellnessdb;"
# Docker: cd wellness-backend && docker compose down -v && docker compose up -d
# Next `dotnet run` re-applies migrations and reseeds the data.
```

---

> 🤝 **Add Collaborators:** To add collaborators or generate an invite link for this project, navigate to: [Settings > Collaborators](https://github.com/AlpC18/Health-Center-Management-System/settings/access) and click "Add people".

## 🚀 Features

### Backend (ASP.NET Core 9 Web API)
- **JWT Authentication & Authorization**: Roles (Admin, Staff, Client) with refresh token rotation.
- **Entity Framework Core**: Code-first approach with MySQL (Pomelo provider).
- **Comprehensive CRUD Operations**: Fully functional endpoints for Clients, Therapists, Services (Sherbimet), Appointments (Terminet), Packages, Memberships, and Sales.
- **Client Portal API**: Specifically isolated API layer `api/portal/*` ensuring clients only have access to their personal data.
- **Security & Reliability**: Global Error Handling middleware, memory cache-based Rate Limiting, Audit Logging for tracking system changes.
- **Live Notifications**: Integration mapping ready.

### Frontend (React + Vite)
- **Admin Dashboard**: Analytics, charting, financial tracking, and real-time feeds.
- **Client Portal**: Dedicated patient view allowing them to book appointments, review memberships, and check purchase histories.
- **State Management**: **Zustand** stores for Authentication, Theme, Notifications, and i18n settings.
- **Premium UI / UX**: Modern design utilizing **TailwindCSS**, dynamic dark/light mode switching, responsive sidebar navigation, and glassmorphism elements.

## 📁 Repository Structure

This is a Monorepo containing both the backend API and the frontend client.

```
Health-Center-Management-System/
├── wellness-backend/
│   ├── WellnessAPI/              # Active ASP.NET Core 8 API
│   │   ├── Controllers/          # API Endpoints
│   │   ├── Models/               # Domain Entities & Identity
│   │   ├── Data/                 # DbContext & Seed Data
│   │   ├── Services/             # Business logic services
│   │   ├── Migrations/           # EF Core database migrations
│   │   └── Program.cs            # App startup & middleware
│   └── WellnessAPI.Tests/        # xUnit integration test project
└── frontend/
    ├── src/
    │   ├── components/           # UI components & Layouts
    │   ├── pages/                # Admin dashboard & Client portal views
    │   └── store/                # Zustand state management
    └── package.json              # React dependencies
```

---

## 🛠️ Step-by-Step Guide: How to Start & Run

### Prerequisites
Before you start, ensure you have the following installed on your machine:
- **[Node.js (v18+)](https://nodejs.org/)**: Required to run the React frontend environment.
- **[.NET 9 SDK](https://dotnet.microsoft.com/)**: Required to build and run the C# backend API.
- **MySQL/MariaDB** via **XAMPP**, **Laragon**, or **Docker** (any one): Required for the database.
- **Git**: To clone the repository and manage version control.

---

### 1. How to Start the Backend (API)

The backend uses **ASP.NET Core** and **Entity Framework Core**. It is responsible for serving data, authenticating users, and interacting with the MySQL database.

Open your terminal and follow these steps **(Terminal 1)**:

**Step 1: Navigate to the backend directory**
```bash
cd wellness-backend/WellnessAPI
```
*This places you in the root API folder where the `WellnessAPI.csproj` project file is located.*

**Step 2: Restore project dependencies**
```bash
dotnet restore
```
*What it does: This command downloads and installs all necessary NuGet packages (like Entity Framework, JWT Bearer, and FluentValidation) that the project needs to run locally.*

**Step 3: Start the database and apply migrations**
```bash
# Start MySQL/MariaDB with ANY one of: XAMPP (Start MySQL), Laragon (Start All),
# or Docker:  cd wellness-backend && docker compose up -d
# Ensure a database named "wellnessdb" exists, then:
dotnet ef database update
```
*What it does: Runs Entity Framework Core migrations against `wellnessdb` and seeds initial admin and client data. The app also applies migrations automatically on `dotnet run`, so this step is optional.*
*(Note: If the `dotnet ef` command is missing, install the tool globally by running `dotnet tool install --global dotnet-ef`)*

**Step 4: Run the API application**
```bash
dotnet run
```
*What it does: This compiles the code and starts the built-in Kestrel web server. You will see console logs confirming the application is listening for requests. The backend will actively run on `http://localhost:5077`.*

---

### 2. How to Start the Frontend

The frontend is a **React** application bundled by **Vite**. It is responsible for the user interface, routing, and communicating with the Backend API.

Open a **new terminal tab** (do not close the backend terminal) and follow these steps **(Terminal 2)**:

**Step 1: Navigate to the frontend directory**
```bash
cd frontend
```
*This places you in the directory where the `package.json` file is located.*

**Step 2: Install Node modules**
```bash
npm install
```
*What it does: This command reads the `package.json` file and downloads all required frontend dependencies (like React, TailwindCSS, Zustand, Axios, React-Router) into a new `node_modules` folder. You only need to run this once.*

**Step 3: Start the local development server**
```bash
npm run dev
```
*What it does: This starts the Vite development server with Hot Module Replacement (HMR). Any changes you make to the UI code will instantly update in the browser. The frontend will be accessible at `http://localhost:5173`.*

> **Quick Start Script (macOS/Linux):**
> You can also run both simultaneously using the `START.sh` file located in the root!

---

## 🔑 How to Log In & Navigate the App

By default, the application seeds test data in the database, including admin and standard client accounts. 

### Admin Interface (Staff Dashboard)
1. Go to `http://localhost:5173/login` in your browser.
2. Enter the Admin Credentials:
   - **Email:** `admin@wellness.com`
   - **Password:** `Admin123!`
3. After logging in, you will be redirected to the **Admin Dashboard** where you can manage clients, add therapists, view appointments, and configure services.

### Client Portal (Patient Dashboard)
1. You can access the client-side system by either creating a new account on the register page (`http://localhost:5173/register`) choosing the **Klient** role, OR logging into the seeded client:
   - **Email:** `client@wellness.com`
   - **Password:** `Client123!`
2. After logging in as a Client, you will be guided to the **Client Portal** (`/portal/dashboard`).
3. Here, you can **Book an Appointment**, track past appointments, and view memberships.

---

## 📡 Testing the API (via Swagger Documentation)

If you are a developer and want to interact with the database directly using the API endpoints, you can use the built-in Swagger interface.

**1. Access the Documentation:**
Make sure the backend is running, then open `http://localhost:5077/swagger` in your browser.

**2. How to Authenticate (Authorize) in Swagger:**
Most endpoints are protected with an `[Authorize]` safeguard. 
1. Scroll down to the **AuthController** and find `POST /api/auth/login`.
2. Click **Try it out**.
3. In the Request body, enter the Admin credentials:
   ```json
   {
     "email": "admin@wellness.com",
     "password": "Admin123!"
   }
   ```
4. Click **Execute**. Look at the Response Body and copy the **`accessToken`** string.
5. Scroll to the very top of the Swagger page and click the **Authorize** button.
6. In the input box, type: `Bearer YOUR_COPIED_TOKEN` (Ensure you write the word "Bearer", a space, and paste the token). 
7. Click **Authorize** and then **Close**.

**3. Running API Endpoints:**
You can now securely call any endpoint! For example:
- Expand **Klientet** > `GET /api/klientet`.
- Click **Try it out** > **Execute**.
- You will receive a `200 OK` status back containing the JSON array of all clients.

---

---

## 🧪 Running the Tests

The project includes an integration test suite in `wellness-backend/WellnessAPI.Tests/`.

Open a terminal and run:

```bash
cd wellness-backend
dotnet test
```

*What it does: Builds both projects, spins up an in-memory test server, runs all xUnit tests, and reports pass/fail results.*

---

## ✍️ Built With
- **C# .NET 9**
- **Entity Framework Core**
- **React 19 + Vite**
- **Tailwind CSS**
- **Zustand**