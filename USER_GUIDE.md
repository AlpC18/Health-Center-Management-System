# Comprehensive User & Run Guide

Welcome to the comprehensive guide for the **Wellness House Management System**. This document provides detailed, step-by-step instructions on how to start the backend API, how to start the frontend application, how to log in, and how to interact with the API.

---

## 🏗️ 1. How to Start the Backend (API)

The backend is built using ASP.NET Core 8. It handles database connections, business logic, User authentication (JWT), and the core APIs.

### Prerequisites:
- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download) installed.
- Ensure the `5077` port on your local machine is available.

### Steps to Run:
**1. Open your terminal** and navigate into the backend directory:
```bash
cd backend/WellnessAPI
```

**2. Restore dependencies (Optional if auto-restored):**
```bash
dotnet restore
```

**3. Apply Database Migrations:**
This creates the actual SQLite database file (`wellness.db`) and applies the schema changes and automated seed data:
```bash
dotnet ef database update
```
*(Note: If the `dotnet ef` command is missing, you must install EF Core tools globally by running `dotnet tool install --global dotnet-ef`)*

**4. Start the API:**
```bash
dotnet run
```
You will see console logs indicating successful status. E.g., `Now listening on: http://localhost:5077`.

---

## 🎨 2. How to Start the Frontend (React Application)

The frontend is a Vite + React application styled with TailwindCSS.

### Prerequisites:
- [Node.js](https://nodejs.org/en/) installed (v18 or higher recommended).

### Steps to Run:
**1. Open a new terminal tab** (do not close the backend terminal) and navigate to the frontend directory:
```bash
cd frontend
```

**2. Install dependencies (First time only):**
```bash
npm install
```

**3. Start the Development Server:**
```bash
npm run dev
```
You should see a message saying `Vite ready in X ms` and your app is serving on `http://localhost:5173`. 
Open your web browser and go to `http://localhost:5173`.

---

## 🔑 3. How to Log In & Navigate the App

By default, the application seeds test data in the database, including administration and standard client accounts. 

### Admin Interface (Staff Dashboard)
1. Go to `http://localhost:5173/login` in your browser.
2. Enter the following Admin Credentials:
   - **Email:** `admin@wellness.com`
   - **Password:** `Admin123!`
3. After logging in, you will be redirected to the **Admin Dashboard** where you can view charts, manage clients, add therapists, view appointments, and configure services.

### Client Portal (Patient Dashboard)
1. You can access the client-side system by either creating a new account on the register page (`http://localhost:5173/register`) choosing the **Klient** role, OR logging into the seeded client:
   - **Email:** `client1@wellness.com`
   - **Password:** `Client123!`
2. After logging in as a Client, you will be guided to the **Client Portal** (`/portal/dashboard`).
3. Here, you can **Book an Appointment**, track past appointments, and view memberships/invoices.

---

## 📡 4. How to Test and Run the API (via Swagger)

If you are a developer and want to interact with the database directly using the API endpoints, you can use the built-in Swagger interface.

**1. Access the Documentation:**
Make sure the backend is running, then open `http://localhost:5077/swagger` in your browser.

**2. How to Authenticate (Authorize) in Swagger:**
Since most endpoints are protected with an `[Authorize]` safeguard, you have to pass a valid token.
1. Scroll down in Swagger to the **AuthController** section and find the `POST /api/auth/login` endpoint.
2. Click **Try it out**.
3. In the Request body, enter the Admin credentials:
   ```json
   {
     "email": "admin@wellness.com",
     "password": "Admin123!"
   }
   ```
4. Click **Execute**.
5. Look at the Response Body. If successful, copy the **`accessToken`** string (a long string beginning with `eyJ...`). Do NOT copy the quotes.
6. Scroll back to the very top of the Swagger page and click the green **Authorize** button.
7. In the value input box, type: `Bearer YOUR_COPIED_TOKEN` (Ensure you write the word "Bearer" followed by a space, and then paste the token). 
8. Click **Authorize** and then **Close**.

**3. Running API Endpoints:**
You can now securely call any endpoint!
- For instance, go to **Klientet** > `GET /api/klientet`.
- Click **Try it out** and then **Execute**.
- You will receive a `200 OK` status back containing the JSON array of all clients in the system.

*(Remember to login again and replace the token when your token expires after 15 minutes!)*
