# TherapyCenter Frontend

React + Vite frontend for the TherapyCenter ASP.NET backend.

## Setup

```bash
npm install
npm run dev
```

The dev server runs on `http://localhost:5173` and proxies all `/api` requests to `https://localhost:7226` (the .NET backend).

## Roles & Navigation

| Role | Home | Capabilities |
|------|------|-------------|
| **Admin** | `/admin` | Manage therapies, doctors, staff, patients, appointments, generate slots |
| **Receptionist** | `/staff` | View/create patients, view/update appointments, book appointments |
| **Doctor** | `/doctor` | View own schedule and appointment details |
| **Patient** | `/patient` | View own appointments, book online |
| **Guardian** | `/patient` | View appointments for dependants, book online |

## API Proxy

`vite.config.js` is configured to proxy `/api/*` → `http://localhost:5000/api/*`.

If your backend runs on a different port, edit `vite.config.js`:
```js
target: 'https://localhost:7226'
        secure: false,
```

## Project Structure

```
src/
├── api/api.js          # All API calls (auth, admin, doctors, patients, appointments)
├── context/AuthContext.jsx  # JWT auth state (login / logout / user)
├── components/
│   ├── Sidebar.jsx     # Role-aware navigation sidebar
│   ├── Modal.jsx       # Reusable modal dialog
│   └── ProtectedRoute.jsx
├── pages/
│   ├── LoginPage.jsx
│   ├── RegisterPage.jsx
│   ├── BookAppointment.jsx  # Shared for staff (offline) and patient (online)
│   ├── admin/          # Admin-only pages
│   ├── doctor/         # Doctor pages
│   ├── patient/        # Patient/Guardian pages
│   └── staff/          # Receptionist pages
└── index.css           # All styling (plain CSS, no framework)
```
