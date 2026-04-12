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
├── backend/
│   └── WellnessAPI/
│       ├── Controllers/      # API Endpoints
│       ├── Models/           # Domain Entities & Identity
│       └── Program.cs        # Middleware setup
└── frontend/
    ├── src/
    │   ├── components/       # UI, Layouts
    │   ├── pages/            # Admin & Client views
    │   └── store/            # Zustand state machines
    └── package.json          # React Dependencies
```

---

## 🛠️ Step-by-Step Guide: How to Start & Run

### Prerequisites
- [Node.js (v18+)](https://nodejs.org/)
- [.NET 8 SDK](https://dotnet.microsoft.com/)
- Git

### 1. How to Start the Backend (API)
The backend requires applying database migrations before the first run.
**Terminal 1:**
```bash
cd backend/WellnessAPI
dotnet restore
dotnet ef database update  # Creates wellness.db and seeds test data
dotnet run
```
*The backend will run on `http://localhost:5077`.*

### 2. How to Start the Frontend
The frontend requires installing NPM packages before the first run.
**Terminal 2:**
```bash
cd frontend
npm install
npm run dev
```
*The frontend will run on `http://localhost:5173`.*

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
   - **Email:** `client1@wellness.com`
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

## ✍️ Built With
- **C# .NET 8**
- **Entity Framework Core**
- **React 18**
- **Vite**
- **Tailwind CSS**
- **Zustand**