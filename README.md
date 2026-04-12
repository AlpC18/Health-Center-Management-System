# Wellness House - Health Center Management System

![Wellness House Banner](https://via.placeholder.com/1200x300.png?text=Wellness+House+Management+System)

A comprehensive, state-of-the-art Health and Wellness Center Management System. This project features a dual-interface approach: an **Admin Dashboard** for clinic staff and a **Client Portal** for patients. 

It is built with a robust **ASP.NET Core Web API (C#)** backend and a dynamic **React (Vite) + TailwindCSS** frontend.

## 🚀 Features

### Backend (ASP.NET Core 8 Web API)
- **JWT Authentication & Authorization**: Roles (Admin, Staff, Client) with refresh token rotation.
- **Entity Framework Core**: Code-first approach with SQLite (easily translatable to MSSQL).
- **Comprehensive CRUD Operations**: Fully functional endpoints for Clients, Therapists, Services (Sherbimet), Appointments (Terminet), Packages, Memberships, and Sales.
- **Client Portal API**: Specifically isolated API layer `api/portal/*` ensuring clients only have access to their personal data.
- **Security & Reliability**: Global Error Handling middleware, memory cache-based Rate Limiting, Audit Logging for tracking system changes.
- **Live Notifications**: Integration mapping ready (SignalR included).

### Frontend (React + Vite)
- **Admin Dashboard**: Analytics, charting, financial tracking, and real-time feeds.
- **Client Portal**: Dedicated patient view allowing them to book appointments, review memberships, and check purchase histories.
- **State Management**: **Zustand** stores for Authentication, Theme, Notifications, and i18n settings (persisted).
- **Premium UI / UX**: Modern design utilizing **TailwindCSS**, dynamic dark/light mode switching, responsive sidebar navigation, and glassmorphism elements.
- **Performance Optimized**: React Lazy loading (`Suspense`), modular component chunks, and generic Axios interceptor logic.

## 📁 Repository Structure

This is a Monorepo containing both the backend API and the frontend client.

```
Health-Center-Management-System/
├── backend/
│   └── WellnessAPI/
│       ├── Controllers/      # API Endpoints
│       ├── Models/           # Domain Entities & Identity
│       ├── Services/         # Auth, Email, Upload interfaces
│       └── Program.cs        # Bootstrapper & Middleware setup
└── frontend/
    ├── src/
    │   ├── api/              # Axios configuration
    │   ├── components/       # UI, Layouts, Forms
    │   ├── pages/            # Admin & Client views
    │   └── store/            # Zustand state machines
    └── package.json          # React Dependencies
```

## 🛠️ Getting Started

### Prerequisites
- [Node.js (v18+)](https://nodejs.org/)
- [.NET 8 SDK](https://dotnet.microsoft.com/)
- Git

### 1. Run the Backend
```bash
cd backend/WellnessAPI
dotnet restore
dotnet ef database update  # Apply entity migrations locally
dotnet run
```
*The backend will generally run on `http://localhost:5077` and swagger documentation can be found at `/swagger`.*

### 2. Run the Frontend
```bash
cd frontend
npm install
npm run dev
```
*The frontend will run on `http://localhost:5173`.*

> **Quick Start Script (macOS/Linux):**
> You can also run both simultaneously using the `START.sh` file located in the root!

## 🧪 Default Credentials
After database migration and seeding, you will have the following accounts available to test:

**Admin Account:**
- *Email*: `admin@wellness.com`
- *Password*: `Admin123!`

**Test Client Portal:**
- *Email*: `client1@wellness.com`
- *Password*: `Client123!`

## ✍️ Built With
- **C# .NET 8**
- **Entity Framework Core**
- **React 18**
- **Vite**
- **Zustand**
- **Tailwind CSS**
- **Axios**

---
*Developed for a modern, digital-first healthcare management experience.*