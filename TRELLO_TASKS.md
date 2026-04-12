# Trello Tasks & Responsibilities

This document outlines the tasks and responsibilities assigned to each team member for the final review and presentation preparation of the **Wellness House Management System**.

## 🗂️ 1. Lumturie Hyseni
**Area:** UI/UX Polish, Frontend Testing, and Presentation Scenario
**Card Title:** `Frontend Optimization & Presentation Demos`

**Checklist:**
- [ ] Test the Dark/Light theme switching functionality across all frontend pages (Clients, Appointments, etc.).
- [ ] Test the newly added CSV Export feature on tables and verify the downloaded data format in Excel.
- [ ] Ensure the mobile/responsive layout of the sidebar and top navigation works flawlessly on smaller screens.
- [ ] Rehearse the `DEMO_SCENARIO.md` steps from start to finish before the presentation. Capture screenshot artifacts for the presentation slides.

---

## 🗂️ 2. Andi Vrajolli
**Area:** Backend Operations, API Testing, and Service Control
**Card Title:** `Backend API Security & Background Worker Integration`

**Checklist:**
- [ ] Log in through Swagger (`http://localhost:5077/swagger`) using the Auth token and test the API endpoint restrictions for Admin vs. Client roles.
- [ ] Monitor console logs to verify that the newly added `AppointmentReminderService` (Background Emailing Service) triggers correctly without throwing errors.
- [ ] Verify the Entity Framework Core database `wellness.db` is perfectly seeded with the initial data.
- [ ] Prepare technical explanations for the system architecture (Security algorithms, JWT lifecycle, Entity Framework implementation) for presentation Q&A.

---

## 🗂️ 3. Rilind Pirana
**Area:** Infrastructure, Deployment, and SignalR
**Card Title:** `Docker Infrastructure & SignalR Chat Management`

**Checklist:**
- [ ] Verify the project builds and runs flawlessly locally via Docker using the new `docker-compose.yml` (`docker-compose up -d`).
- [ ] Test the Live Chat (Floating Chat widget) built with SignalR by opening two different browser sessions (e.g., normal and incognito). Ensure real-time socket delivery works.
- [ ] Review the updated `README.md` and `USER_GUIDE.md` on GitHub; correct any missing instructions regarding setup.
- [ ] Research and prepare a short technical response for: "What would the exact process be if we transition from SQLite to PostgreSQL natively?" 

---
