# Wellness House — Presentation Demo Scenario

## Before we begin
1. Is Backend running? → http://localhost:5077/swagger
2. Is Frontend running? → http://localhost:5173
3. Open two terminals, one for backend, one for frontend

## Demo Sequence (15 minutes)

### 1. Admin Login (2 min)
- http://localhost:5173/login
- Email: admin@wellness.com
- Password: Admin123!
- Dashboard opens → show statistics

### 2. Dashboard Overview (2 min)
- Total client count
- Today's appointments
- Active memberships
- Monthly revenue
- Charts

### 3. Client Management (3 min)
- Left menu → Klientët
- 12 clients in the table
- "Shto të re" → add new client
  - Emri: Demo, Mbiemri: Presentation
  - Email: demo@presentation.com
  - Gjinia: M
  - Save
- Edit the added client
- Delete (but cancel it)

### 4. Appointment Management (2 min)
- Left menu → Terminet
- Appointment list
- Add new appointment
  - Select Client
  - Select Service (Shërbim)
  - Select Therapist
  - Date and time
  - Save

### 5. Security Demonstration (1 min)
- F12 → Application → Local Storage
- wellness-auth → show accessToken and refreshToken
- Explain "Token is auto-refreshed"

### 6. Client Portal (3 min)
- Open new tab → http://localhost:5173/register
- First Name: Portal, Last Name: Klient
- Email: portal@test.com
- Password: Test123!
- Select Role: Klient
- Save → portal/dashboard opens
- Introduce the Portal menu
- Book Appointment (Rezervo Termin) → show the 4 steps
- Show Profile page

### 7. API Documentation (1 min)
- http://localhost:5077/swagger
- Show endpoints
- Test with Bearer token

## Live Coding Preparation

### Potential Questions and Answers:

QUESTION: What is a JWT token?
ANSWER: Services/TokenService.cs → Show GenerateAccessToken method
- 15 minutes lifetime
- Contains user details as claims
- Signed with HMAC-SHA256

QUESTION: How does the refresh token work?
ANSWER: 
- Services/TokenService.cs → RotateRefreshTokenAsync
- Stored in DB, single use
- 7 days lifetime
- Explain Frontend api.js → interceptor

QUESTION: How does CRUD work?
ANSWER: Open Controllers/KlientetController.cs
- GET, POST, PUT, DELETE
- [Authorize] attribute
- Database operations using DbContext

QUESTION: Add a new field
ANSWER: Add a simple endpoint to KlientetController:
[HttpGet("count")]
public async Task<IActionResult> GetCount()
{
    var count = await _db.Klientet.CountAsync();
    return Ok(new { count });
}

QUESTION: How is the token sent from the frontend?
ANSWER: Explain src/api/api.js
- Request interceptor
- Authorization: Bearer {token}
- Auto-refresh on 401

## Technical Glossary

- JWT (JSON Web Token): User authentication token
- Refresh Token: Long-lived token used to renew the access token
- Bearer: Method for passing tokens in the HTTP Authorization header
- Interceptor: Function that automatically intercepts HTTP requests
- Zustand: Lightweight state management library for React
- Lazy Loading: Technique of loading pages only when needed
- Migration: Managing database schema changes
- DbContext: Database communication layer using Entity Framework
- CORS: Controlling requests from different origins
- Seed Data: Initial test data
