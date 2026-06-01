Therapy Center
A full-stack clinic management system with role-based access for admin, doctors, receptionists, and patients.

Frontend: React 18 + Vite 6
Backend: ASP.NET Core 10 Web API
Database: MySQL 8.0
Auth: JWT with role-based policies
Tests: xUnit

Features
Role	Capabilities
Admin	Manage therapies, doctors, staff, slot generation, patients, appointments
Receptionist	Register patients, book appointments, view schedules
Doctor	View appointments, write clinical findings
Patient / Guardian	Self-register, book appointments online
Quick Start
Prerequisites
.NET 10 SDK
Node.js 18+
MySQL 8.0 running locally
1. Database Setup
Update the connection string in therapy-center-backend/TherapyCenter/appsettings.json:


"DefaultConnection": "server=localhost;port=3306;database=therapycenter;user=root;password=YOUR_PASSWORD"
Run migrations:


cd therapy-center-backend/TherapyCenter
dotnet ef database update
2. Backend

cd therapy-center-backend/TherapyCenter
dotnet run --launch-profile https
Runs at https://localhost:7226

3. Frontend

cd therapy-center-frontend
npm install
npm run dev
Runs at http://localhost:5173 — proxies /api to the backend.

4. Run Tests

dotnet test therapy-center-tests/TherapyCenter.Tests.csproj
Project Structure

├── therapy-center-backend/          # .NET Web API
│   ├── Controllers/                 # 7 API controllers
│   ├── Services/                    # Business logic (8 services)
│   ├── Repositories/                # Data access (8 repositories)
│   ├── Entities/                    # Database models (User, Patient, Doctor, etc.)
│   ├── DTO_s/                       # Request/Response models
│   ├── Data/AppDbContext.cs         # EF Core context
│   └── Program.cs                   # Entry point, DI, middleware
│
├── therapy-center-frontend/         # React + Vite
│   └── src/
│       ├── pages/                   # admin/, doctor/, patient/, staff/
│       ├── components/              # Sidebar, ProtectedRoute, Modal
│       ├── context/AuthContext.jsx   # JWT auth management
│       └── api/api.js               # HTTP client with auto JWT
│
└── therapy-center-tests/            # xUnit tests (77 passing)
    └── Services/                    # One test file per service
API Endpoints
Controller	Method	Route	Auth
Auth	POST	/api/auth/register	Public
POST	/api/auth/login	Public
POST	/api/auth/create-staff	Admin
Admin	GET/POST/PUT/DELETE	/api/admin/therapies	Admin
POST	/api/admin/doctors/profile	Admin
POST	/api/admin/slots/generate	Admin
DELETE	/api/admin/doctors/{id} / /api/admin/staff/{id}	Admin
Appointment	POST	/api/appointment/book	Staff
POST	/api/appointment/book-online	Patient/Guardian
GET	/api/appointment/{id}/patient/{id}/doctor/{id}/date/{date}	Varies
PATCH	/api/appointment/{id}/status	Staff/Doctor
Doctor	GET	/api/doctor / {id} / {id}/slots	Any auth
GET	/api/doctor/my-appointments	Doctor
GET/PUT	/api/doctor/appointments/{id}/finding	Doctor
Patient	GET/POST	/api/patient	Staff
GET	/api/patient/me / guardian/{id}	Patient/Guardian
Payment	GET/POST	/api/payment/appointment/{id} / patient/{id}	Staff
PATCH	/api/payment/{appointmentId}/paid	Staff
Therapy	GET	/api/therapy	Any auth
Architecture

Frontend (React) ──HTTP/JSON──▶ Backend (ASP.NET Core)
                                   │
                              Controllers
                                   │
                              Services (business logic)
                                   │
                              Repositories (data access)
                                   │
                              AppDbContext (EF Core)
                                   │
                              MySQL 8.0
JWT Authentication — stateless, 24-hour tokens with role claims
Repository Pattern — clean separation of data access
Unit of Work — transaction management without coupling to EF Core
CORS — configured for localhost:5173 (Vite dev server)
Database Schema
8 tables: Users, Patients, Doctors, Therapies, Appointments, Slots, DoctorFindings, Payments

Key relationships:

User ↔ Doctor (1:1) · User ↔ Patient (optional 1:1)
Doctor → Slot (1:N) · Doctor → Appointment (1:N)
Patient → Appointment (1:N) · Therapy → Appointment (1:N)
Appointment ↔ DoctorFinding (1:1) · Appointment ↔ Payment (1:1)