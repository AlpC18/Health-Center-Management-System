# Wellness House - Health Center Management System

![Wellness House Banner](https://via.placeholder.com/1200x300.png?text=Wellness+House+Management+System)

A comprehensive, state-of-the-art Health and Wellness Center Management System. This project features a dual-interface approach: an **Admin Dashboard** for clinic staff and a **Client Portal** for patients. 

It is built with a robust **ASP.NET Core Web API (C#)** backend and a dynamic **React (Vite) + TailwindCSS** frontend.

> 🤝 **Add Collaborators:** To add collaborators or generate an invite link for this project, navigate to: [Settings > Collaborators](https://github.com/AlpC18/Health-Center-Management-System/settings/access) and click "Add people".

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
Before you start, ensure you have the following installed on your machine:
- **[Node.js (v18+)](https://nodejs.org/)**: Required to run the React frontend environment.
- **[.NET 8 SDK](https://dotnet.microsoft.com/)**: Required to build and run the C# backend API.
- **Git**: To clone the repository and manage version control.

---

### 1. How to Start the Backend (API)

The backend uses **ASP.NET Core** and **Entity Framework Core**. It is responsible for serving data, authenticating users, and interacting with the SQLite database.

Open your terminal and follow these steps **(Terminal 1)**:

**Step 1: Navigate to the backend directory**
```bash
cd backend/WellnessAPI
```
*This places you in the root API folder where the `WellnessAPI.csproj` project file is located.*

**Step 2: Restore project dependencies**
```bash
dotnet restore
```
*What it does: This command downloads and installs all necessary NuGet packages (like Entity Framework, JWT Bearer, and FluentValidation) that the project needs to run locally.*

**Step 3: Update the database and apply migrations**
```bash
dotnet ef database update
```
*What it does: This is a critical step. It executes Entity Framework Core migrations. It will automatically generate a new `wellness.db` SQLite database file on your machine and seed it with initial admin and client data so you can test the system immediately.*
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