# Wellness House — Trello Board Content

## Board Name: Wellness House Management System

## Columns:
1. Backlog
2. In Progress
3. Testing
4. Done

## Cards and Labels:

### DONE ✅ (Add these cards to the "Done" column)

[Backend] [Database]
Card: Database design and migration
Description: MSSQL/SQLite, all entities, FK relations, unique indexes

[Backend] [Auth]
Card: JWT Authentication system
Description: Access token (15min), Refresh token (7days, in DB), token rotation

[Backend] [Auth]
Card: ASP.NET Core Identity integration
Description: Users, Roles, UserRoles, UserClaims, UserTokens tables

[Backend] [CRUD]
Card: Klientet CRUD API
Description: GET, POST, PUT, DELETE endpoints, email uniqueness check

[Backend] [CRUD]
Card: Sherbimet CRUD API
Description: Wellness services management

[Backend] [CRUD]
Card: Terapistet CRUD API
Description: Therapist management, license tracking

[Backend] [CRUD]
Card: Terminet CRUD API
Description: Appointment management, conflict checking

[Backend] [CRUD]
Card: PaketaWellness CRUD API
Description: Wellness packages management

[Backend] [CRUD]
Card: Anetaresimet CRUD API
Description: Membership management

[Backend] [CRUD]
Card: Programet CRUD API
Description: Wellness programs

[Backend] [CRUD]
Card: Produktet CRUD API
Description: Product management, stock tracking

[Backend] [CRUD]
Card: Shitjet CRUD API
Description: Sales records

[Backend] [CRUD]
Card: Vlereisimet CRUD API
Description: Rating system, 1-5 stars

[Backend] [Dashboard]
Card: Dashboard statistics API
Description: Total clients, today's appointments, monthly revenue, etc.

[Backend] [Security]
Card: Rate limiting implementation
Description: Login 5/5min, Register 3/hour, general 60/min

[Backend] [Security]
Card: Global error handling middleware
Description: Consistent JSON response for all errors

[Backend] [Security]
Card: Audit log system
Description: Record of who, when, and what was changed

[Backend] [Portal]
Card: Client Portal API
Description: /api/portal/* endpoints, clients access their own data

[Frontend] [UI]
Card: Admin panel layout and navigation
Description: Sidebar, responsive design, dark mode

[Frontend] [Auth]
Card: Login and Register pages
Description: JWT token saved in localStorage, role-based routing

[Frontend] [State]
Card: Zustand state management
Description: Auth store, token management, persist middleware

[Frontend] [API]
Card: Axios interceptor
Description: Bearer token on every request, auto-refresh on 401

[Frontend] [CRUD]
Card: All entity CRUD pages
Description: 10 entities, search, sorting, pagination

[Frontend] [Dashboard]
Card: Admin dashboard
Description: Statistics cards, charts, activity feed

[Frontend] [Portal]
Card: Client portal pages
Description: Dashboard, appointment booking, profile, purchasing products

[Frontend] [Performance]
Card: Lazy loading and code splitting
Description: React.lazy, Suspense, Vite manualChunks

[Frontend] [UX]
Card: Dark/Light theme
Description: Tailwind darkMode: class, localStorage persist

[Frontend] [UX]
Card: English/Albanian language support
Description: i18n system, language toggle button

[Testing]
Card: API endpoint testing
Description: all endpoints tested using curl

[Git]
Card: GitHub repository setup
Description: wellness-backend and wellness-frontend repos
