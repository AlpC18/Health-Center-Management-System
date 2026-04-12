# Wellness House Yönetim Sistemi

UBT Kolegji — Lab Course 1 (2025/2026) frontend project.

## Tech Stack

| Technology | Version |
|---|---|
| React | 18 |
| Vite | 5 |
| React Router DOM | v6 |
| Tailwind CSS | 3.4 |
| Zustand | — |
| Axios | — |
| React Hot Toast | — |
| Lucide React | — |

## Setup

```bash
npm install
npm run dev      # http://localhost:5173
npm run build
```

## Folder Structure

```
src/
├── api/
│   └── api.js              # Axios instance + all API calls
├── components/
│   ├── crud/
│   │   ├── CrudPage.jsx    # Generic list/create/edit/delete page
│   │   └── Forms.jsx       # All entity form components
│   ├── layout/
│   │   └── Layout.jsx      # Sidebar + header shell
│   └── ui/
│       └── index.jsx       # Shared UI: Spinner, Modal, StatusBadge, etc.
├── pages/
│   ├── AuthPages.jsx       # LoginPage, RegisterPage
│   ├── DashboardPage.jsx   # Stats overview
│   └── EntityPages.jsx     # All CRUD entity pages (named exports)
├── store/
│   └── authStore.js        # Zustand auth store (persisted)
├── App.jsx
├── index.css
└── main.jsx
```

## API Endpoints

Base URL: `https://localhost:5001/api`

### Auth

| Method | Endpoint | Description |
|---|---|---|
| POST | /auth/login | Login, returns accessToken + refreshToken |
| POST | /auth/register | Register new user |
| POST | /auth/logout | Logout |
| POST | /auth/refresh | Refresh access token |

### Entities (all support GET / POST / PUT / DELETE)

| Resource | Endpoint |
|---|---|
| Klientet | /klientet |
| Sherbimet | /sherbimet |
| Terapistet | /terapistet |
| Terminet | /terminet |
| Pakata Wellness | /paketawellness |
| Anëtarësimet | /anetaresimet |
| Programet | /programet |
| Produktet | /produktet |
| Shitjet | /shitjet |
| Vlerësimet | /vlereisimet |

### Dashboard

| Method | Endpoint | Description |
|---|---|---|
| GET | /dashboard/stats | Aggregated stats for dashboard cards |

## JWT Auth Flow

1. Login → server returns `accessToken` (short-lived) + `refreshToken` (long-lived)
2. Zustand store persists both tokens to `localStorage` (key: `wellness-auth`)
3. Axios request interceptor injects `Authorization: Bearer <accessToken>` on every request
4. On 401 response, Axios response interceptor calls `/auth/refresh` once
5. Pending requests are queued (`failedQueue`) during refresh; replayed on success
6. On refresh failure, all tokens cleared and user redirected to `/login`
